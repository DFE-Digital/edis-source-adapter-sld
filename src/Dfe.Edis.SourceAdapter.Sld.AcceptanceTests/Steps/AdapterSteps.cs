using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Dfe.Edis.SourceAdapter.Sld.AcceptanceTests.Steps
{
    [Binding]
    public class AdapterSteps
    {
        private readonly TestHarness.TestHarness _harness;

        public AdapterSteps(TestHarness.TestHarness harness)
        {
            _harness = harness;
        }

        [When("the system polls for providers")]
        public async Task WhenTheSystemPollsForProviders()
        {
            await _harness.Poll();
        }
    }
}