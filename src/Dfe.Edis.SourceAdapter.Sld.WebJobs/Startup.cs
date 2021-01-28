using System.Net.Http;
using Dfe.Edis.Kafka;
using Dfe.Edis.SourceAdapter.Sld.Application;
using Dfe.Edis.SourceAdapter.Sld.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Sld.Domain.Queuing;
using Dfe.Edis.SourceAdapter.Sld.Domain.StateManagement;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;
using Dfe.Edis.SourceAdapter.Sld.Infrastructure.AzureStorage.Queuing;
using Dfe.Edis.SourceAdapter.Sld.Infrastructure.AzureStorage.StateManagement;
using Dfe.Edis.SourceAdapter.Sld.Infrastructure.SubmitLearnerDataApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Sld.WebJobs
{
    public class Startup
    {
        public void Configure(IServiceCollection services, RootAppConfiguration configuration)
        {
            AddConfiguration(services, configuration);

            services.AddHttpClient();

            AddState(services);
            AddQueuing(services);
            AddSubmitLearnerData(services);

            services
                .AddScoped<IChangeProcessor, ChangeProcessor>();
        }

        private void AddConfiguration(IServiceCollection services, RootAppConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddSingleton(configuration.State);
            services.AddSingleton(configuration.Queuing);
            services.AddSingleton(configuration.SubmitLearnerData);
        }

        private void AddState(IServiceCollection services)
        {
            services.AddScoped<IStateStore, TableStateStore>();
        }

        private void AddQueuing(IServiceCollection services)
        {
            services.AddScoped<IProviderQueue, StorageProviderQueue>();
        }

        private void AddSubmitLearnerData(IServiceCollection services)
        {
            services.AddSingleton<ISubmitLearnerDataAuthenticator>(sp =>
            {
                var httpClientFactory = sp.GetService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient();
                var configuration = sp.GetService<SubmitLearnerDataConfiguration>();
                var logger = sp.GetService<ILogger<SubmitLearnerDataAuthenticator>>();
                return new SubmitLearnerDataAuthenticator(httpClient, configuration, logger);
            });
            services.AddScoped<ISldClient>(sp =>
            {
                var httpClientFactory = sp.GetService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient();
                var authenticator = sp.GetService<ISubmitLearnerDataAuthenticator>();
                var configuration = sp.GetService<SubmitLearnerDataConfiguration>();
                var logger = sp.GetService<ILogger<SubmitLearnerDataApiClient>>();

                return new SubmitLearnerDataApiClient(httpClient, authenticator, configuration, logger);
            });
        }
    }
}