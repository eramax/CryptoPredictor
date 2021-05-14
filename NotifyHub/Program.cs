using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging => logging.AddConsole())
    .ConfigureWebHostDefaults(webBuilder => webBuilder
    .ConfigureServices(services =>
    {
        services.AddCors();
        services.AddSignalR();
    })
    .Configure(app =>
    {
        app.UseRouting();
        app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed((host) => true).AllowCredentials());
        app.UseEndpoints(endpoints => endpoints.MapHub<AppHub>("/ws"));
    })).Build().Run();