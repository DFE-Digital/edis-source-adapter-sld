using System.Collections.Generic;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;

namespace Dfe.Edis.SourceAdapter.Sld.AcceptanceTests.TestHarness
{
    public class TestContext
    {
        public int Ukprn { get; set; }
        public List<Learner> Learners { get; set; } = new List<Learner>();
        
        public void Reset()
        {
            Ukprn = 0;
            Learners.Clear();
        }
    }
}