using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Models;

Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging => logging.AddConsole())
    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseUrls("http://0.0.0.0:5500")
    .ConfigureServices(services =>
    {
        services.AddControllers();       
        services.AddMassTransitHostedService().AddMassTransit(x =>
        {
            x.AddRequestClient<PredictionRequest>();
            x.AddBus(context => Bus.Factory.CreateUsingRabbitMq(c =>
            {
                c.Host("rabbitmq://localhost");
                c.ConfigureEndpoints(context);
            }));
        });
    })
    .Configure(app =>
    {
        app.UseRouting();
        app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed((host) => true).AllowCredentials());
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    })).Build().Run();