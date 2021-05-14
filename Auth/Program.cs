using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

string stringKey = "";
Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging => logging.AddConsole())
    .ConfigureWebHostDefaults(webBuilder => webBuilder
    .ConfigureAppConfiguration((hostingContext, config) => stringKey = config.Build().GetValue<string>("Secret"))
    .ConfigureServices(services =>
    {
        var key = Encoding.UTF8.GetBytes(stringKey);
        services.AddSingleton<JWTAuthenticationManager>(new JWTAuthenticationManager(key));
        services.AddCors();
        services.AddControllers();
    })
    .Configure(app =>
    {
        app.UseRouting();
        app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed((host) => true).AllowCredentials());
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    })).Build().Run();
