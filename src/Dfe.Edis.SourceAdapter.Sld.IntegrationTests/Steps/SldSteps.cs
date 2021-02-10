using TechTalk.SpecFlow;

namespace Dfe.Edis.SourceAdapter.Sld.IntegrationTests.StepDefinitions
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
            
        }
    }
}