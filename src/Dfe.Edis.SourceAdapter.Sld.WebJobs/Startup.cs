using Dfe.Edis.Kafka;
using Dfe.Edis.SourceAdapter.Sld.Application;
using Dfe.Edis.SourceAdapter.Sld.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Sld.Domain.StateManagement;
using Dfe.Edis.SourceAdapter.Sld.Infrastructure.AzureStorage.StateManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.Edis.SourceAdapter.Sld.WebJobs
{
    public class Startup
    {
        public void Configure(IServiceCollection services, RootAppConfiguration configuration)
        {
            AddConfiguration(services, configuration);

            services.AddHttpClient();

            AddState(services);

            services
                .AddScoped<IChangeProcessor, ChangeProcessor>();
        }

        private void AddConfiguration(IServiceCollection services, RootAppConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddSingleton(configuration.State);
        }

        private void AddState(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IStateStore, TableStateStore>();
        }
    }
}