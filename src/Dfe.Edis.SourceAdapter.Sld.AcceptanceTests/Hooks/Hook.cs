using TechTalk.SpecFlow;

namespace Dfe.Edis.SourceAdapter.Sld.AcceptanceTests.Hooks
{
    [Binding]
    public class Hooks
    {
        private readonly TestHarness.TestHarness _harness;

        public Hooks(TestHarness.TestHarness harness)
        {
            _harness = harness;
        }
        
        [BeforeScenario]
        public void BeforeScenarioResetTestHarness()
        {
            _harness.Reset();
        }
    }
}