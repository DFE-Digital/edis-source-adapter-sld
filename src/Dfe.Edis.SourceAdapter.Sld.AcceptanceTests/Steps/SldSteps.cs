using System.Linq;
using Dfe.Edis.SourceAdapter.Sld.AcceptanceTests.Helpers;
using TechTalk.SpecFlow;

namespace Dfe.Edis.SourceAdapter.Sld.AcceptanceTests.Steps
{
    [Binding]
    public class SldSteps
    {
        private readonly TestHarness.TestHarness _harness;

        public SldSteps(TestHarness.TestHarness harness)
        {
            _harness = harness;
        }
        
        [Given("A provider submits data for the first time")]
        public void GivenAProviderSubmitsDataForTheFirstTime()
        {
            var ukprn = RandomDataGenerator.Number(10000000, 99999999);
            var learners = Enumerable.Range(1, 5).Select(index =>
                RandomDataGenerator.Learner(ukprn)).ToArray();
            _harness.Sld.AddLearnersToAcademicYear("2021", learners);
        }
    }
}