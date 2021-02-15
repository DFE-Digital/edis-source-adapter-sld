using TechTalk.SpecFlow;

namespace Dfe.Edis.SourceAdapter.Sld.AcceptanceTests.Steps
{
    [Binding]
    public class ProducerSteps
    {
        private readonly TestHarness.TestHarness _harness;

        public ProducerSteps(TestHarness.TestHarness harness)
        {
            _harness = harness;
        }
        
        [Then("the providers data should be published to Kafka")]
        public void ThenTheProvidersDataShouldBePublishedToKafka()
        {
            
        }
    }
}