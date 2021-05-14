using BinanceExchange.API.Client;
using BinanceExchange.API.Client.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

string apiKey = "";
string secretKey = "";
Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging => logging.AddConsole())
    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseUrls("http://0.0.0.0:5200")
    .ConfigureAppConfiguration((hostingContext, config) => 
    { apiKey = config.Build().GetValue<string>("apiKey"); secretKey = config.Build().GetValue<string>("secretKey"); })
    .ConfigureServices(services =>
    {
        var binanceClient = new BinanceClient(new ClientConfiguration() { ApiKey = apiKey, SecretKey = secretKey });
        services.AddResponseCompression();
        services.AddApiVersioning(config =>
        { config.DefaultApiVersion = new ApiVersion(1, 0); config.AssumeDefaultVersionWhenUnspecified = true; });
        services.AddSingleton<IBinanceClient>(binanceClient);
        services.AddCors();
        services.AddResponseCaching();
        services.AddControllers();
    })
    .Configure(app =>
    {
        app.UseRouting();
        app.UseResponseCompression();
        app.UseResponseCaching();
        app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed((host) => true).AllowCredentials());
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    })).Build().Run();
