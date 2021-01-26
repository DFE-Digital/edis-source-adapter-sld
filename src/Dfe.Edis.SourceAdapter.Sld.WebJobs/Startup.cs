using Dfe.Edis.Kafka;
using Dfe.Edis.SourceAdapter.Sld.Application;
using Dfe.Edis.SourceAdapter.Sld.Domain.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.Edis.SourceAdapter.Sld.WebJobs
{
    public class Startup
    {
        public void Configure(IServiceCollection services, RootAppConfiguration configuration)
        {
            AddConfiguration(services, configuration);

            services.AddHttpClient();

            services
                .AddScoped<IChangeProcessor, ChangeProcessor>();
        }

        private void AddConfiguration(IServiceCollection services, RootAppConfiguration configuration)
        {
            services.AddSingleton(configuration);
        }
    }
}