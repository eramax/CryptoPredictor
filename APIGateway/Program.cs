using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;
using Ocelot.Provider.Polly;

string stringKey = "";
Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging => logging.AddConsole())
    .ConfigureWebHostDefaults(webBuilder => webBuilder
    .ConfigureAppConfiguration((hostingContext, config) => stringKey = config.Build().GetValue<string>("Secret"))
    .ConfigureServices(services =>
    {
        var key = Encoding.UTF8.GetBytes(stringKey);
        services.AddCors().AddOcelot().AddCacheManager(settings => settings.WithDictionaryHandle()).AddPolly();
        services.AddAuthentication(x => x.DefaultAuthenticateScheme = x.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
    })
    .Configure(app =>
    {
        app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed((host) => true).AllowCredentials());
        app.UseAuthentication();
        app.UseWebSockets();
        app.UseOcelot().Wait();
    })).Build().Run();
