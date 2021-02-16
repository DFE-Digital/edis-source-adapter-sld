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
            _harness.Context.Ukprn = RandomDataGenerator.Number(10000000, 99999999);
            _harness.Context.Learners = Enumerable.Range(1, 5).Select(index =>
                RandomDataGenerator.Learner(_harness.Context.Ukprn)).ToList();
            _harness.Sld.AddLearnersToAcademicYear("2021", _harness.Context.Learners.ToArray());
        }
    }
}