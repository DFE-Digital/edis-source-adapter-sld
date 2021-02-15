using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;

namespace Dfe.Edis.SourceAdapter.Sld.AcceptanceTests.TestHarness
{
    public class InMemorySldDataReceiver : ISldDataReceiver
    {
        public Task SendLearnerAsync(Learner learner, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}