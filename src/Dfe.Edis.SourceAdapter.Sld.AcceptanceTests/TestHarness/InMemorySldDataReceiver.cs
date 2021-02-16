using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;

namespace Dfe.Edis.SourceAdapter.Sld.AcceptanceTests.TestHarness
{
    public class InMemorySldDataReceiver : ISldDataReceiver
    {
        private readonly List<Learner> _sentLearners = new List<Learner>();
        public IEnumerable<Learner> SentLearners => _sentLearners;

        public Task SendLearnerAsync(Learner learner, CancellationToken cancellationToken)
        {
            _sentLearners.Add(learner);
            return Task.CompletedTask;
        }

        public void Reset()
        {
            _sentLearners.Clear();
        }
    }
}