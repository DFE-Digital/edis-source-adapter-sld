using System;
using System.IO;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dfe.Edis.SourceAdapter.Sld.WebJobs
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder();
            var defaultLogLevel = LogLevel.Information;
            
#if DEBUG
            hostBuilder.UseEnvironment("development");
            defaultLogLevel = LogLevel.Debug;
#endif

            var configuration = LoadConfiguration();

            JsonConvert.DefaultSettings = () =>
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };

            hostBuilder.ConfigureWebJobs((webJobBuilder) =>
            {
                webJobBuilder.AddTimers();
                webJobBuilder.AddAzureStorageCoreServices();
                webJobBuilder.AddAzureStorage();
            });
            hostBuilder.ConfigureLogging((context, logBuilder) =>
            {
                // Having issues with setting log level with environment variable as per documentation
                // (https://docs.microsoft.com/en-us/dotnet/core/extensions/logging#set-log-level-by-command-line-environment-variables-and-other-configuration)
                // So using same variable name, but doing manually
                LogLevel logLevel;
                if (!Enum.TryParse(Environment.GetEnvironmentVariable("Logging__LogLevel__Default"), true, out logLevel))
                {
                    logLevel = defaultLogLevel;
                }
                logBuilder.SetMinimumLevel(logLevel);
                
                logBuilder.AddConsole();

                var appInsightsKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
                if (!string.IsNullOrWhiteSpace(appInsightsKey))
                {
                    logBuilder.AddApplicationInsights(appInsightsKey);
                }
            });
            hostBuilder.ConfigureServices((context, services) =>
            {
                var startup = new Startup();
                startup.Configure(services, configuration);
            });

            using var host = hostBuilder.Build();
            await host.RunAsync();
        }

        static RootAppConfiguration LoadConfiguration()
        {
            var configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .Build();
            
            var configuration = new RootAppConfiguration();
            configurationRoot.Bind(configuration);

            return configuration;
        }
    }
}