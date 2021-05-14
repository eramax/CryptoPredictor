namespace Shared.Models
{
    public record PredictionRequest(string currency, int? limit, long? endDate, string tframe);
}
