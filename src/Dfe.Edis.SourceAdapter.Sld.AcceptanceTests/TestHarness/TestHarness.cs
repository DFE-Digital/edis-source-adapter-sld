using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Sld.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Sld.Domain.Queuing;
using Dfe.Edis.SourceAdapter.Sld.Domain.StateManagement;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;
using Dfe.Edis.SourceAdapter.Sld.WebJobs;
using Dfe.Edis.SourceAdapter.Sld.WebJobs.Functions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dfe.Edis.SourceAdapter.Sld.AcceptanceTests.TestHarness
{
    public class TestHarness
    {
        private ServiceProvider _serviceProvider;

        public TestHarness()
        {
            // Get service collection to setup
            var serviceCollection = new ServiceCollection();

            // Create dummy configuration
            var configuration = new RootAppConfiguration
            {
                Queuing = new QueuingConfiguration(),
                State = new StateConfiguration(),
                SubmitLearnerData = new SubmitLearnerDataConfiguration(),
                DataServicePlatform = new DataServicePlatformConfiguration(),
            };

            // Setup application
            var startup = new Startup();
            startup.Configure(serviceCollection, configuration);

            SetupServiceForTestHarness(serviceCollection);

            // Get service provider
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public void Reset()
        {
            State.Reset();
            
            Sld.Reset();
        }

        public async Task Poll()
        {
            var pollFunction = _serviceProvider.GetService<TimedCheckForChange>();
            await pollFunction.RunAsync(
                new TimerInfo(new ConstantSchedule(new TimeSpan(1, 0, 0, 0)), new ScheduleStatus(), false),
                CancellationToken.None);
        }

        public InMemoryStateStore State => (InMemoryStateStore) _serviceProvider.GetService<IStateStore>();
        public InMemorySldClient Sld => (InMemorySldClient) _serviceProvider.GetService<ISldClient>();


        private void SetupServiceForTestHarness(ServiceCollection serviceCollection)
        {
            // Replace stubbed services
            serviceCollection.RemoveAll(typeof(IStateStore));
            serviceCollection.AddSingleton<IStateStore, InMemoryStateStore>();

            serviceCollection.RemoveAll(typeof(ISldClient));
            serviceCollection.AddSingleton<ISldClient, InMemorySldClient>();

            serviceCollection.RemoveAll(typeof(IProviderQueue));
            serviceCollection.AddSingleton<IProviderQueue, InProcessProviderQueue>();

            serviceCollection.RemoveAll(typeof(ILearnerQueue));
            serviceCollection.AddSingleton<ILearnerQueue, InProcessLearnerQueue>();

            serviceCollection.RemoveAll(typeof(ISldDataReceiver));
            serviceCollection.AddSingleton<ISldDataReceiver, InMemorySldDataReceiver>();

            // Add functions (as they are done by runtime we are not running within)
            var functions = GetFunctions();
            foreach (var function in functions)
            {
                serviceCollection.AddScoped(function);
            }
        }

        private Type[] GetFunctions()
        {
            var functionTriggerTypes = new[]
            {
                typeof(TimerTriggerAttribute),
                typeof(QueueTriggerAttribute),
            };

            var allParameters = typeof(Startup).Assembly
                .GetTypes()
                .SelectMany(t => t.GetMethods())
                .SelectMany(m => m.GetParameters());
            var matchingParameters = allParameters
                .Where(p => p.CustomAttributes.Any(a => functionTriggerTypes.Any(t => t == a.AttributeType)))
                .ToArray();
            var functionTypes = matchingParameters
                .Select(m => m.Member.DeclaringType)
                .ToArray();

            return functionTypes;
        }
    }
}