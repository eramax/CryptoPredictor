using MassTransit;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

class PredictionRequestConsumer : IConsumer<PredictionRequest>
{
    public async Task Consume(ConsumeContext<PredictionRequest> context)
    {
        Console.WriteLine($"Value: {context.Message.currency} - {context.Message.tframe}");
        var data = await FetchData(context.Message);
        PredictionResult result = Forcast(data);
        await context.RespondAsync(result);
    }

    static readonly HttpClient client = new();
    public record InputModel(float Close);
    static string host = "http://localhost:5200";
    static async Task<IEnumerable<InputModel>> FetchData(PredictionRequest request)
    {
        try
        {
            var response = await 
                client.GetAsync($"{host}/data/{request.currency}?tframe={request.tframe}&limit={request.limit}&endDate={request.endDate}");
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<List<CandleStick>>();
            var converted = data.Select(d => new InputModel(decimal.ToSingle(d.Close)));
            return converted;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
            return default;
        }
    }

    static PredictionResult Forcast(IEnumerable<InputModel> data)
    {
        int series = 7;
        int window = 2;
        int horizon = 20;
        float confidenceLevel = 0.75f;
        if (data == default) return default;
        var context = new MLContext();
        var dataview = context.Data.LoadFromEnumerable(data);

        var pipeline = context.Forecasting.ForecastBySsa(
            nameof(PredictionResult.Value),
            nameof(InputModel.Close),
            windowSize: window,
            seriesLength: series,
            trainSize: data.Count(),
            horizon: horizon,
            confidenceLevel: confidenceLevel,
            confidenceLowerBoundColumn: nameof(PredictionResult.Low),
            confidenceUpperBoundColumn: nameof(PredictionResult.High)
            );
        var model = pipeline.Fit(dataview);
        var forecastingEngine = model.CreateTimeSeriesEngine<InputModel, PredictionResult>(context);
        var forecasts = forecastingEngine.Predict();
        return forecasts;
    }
}