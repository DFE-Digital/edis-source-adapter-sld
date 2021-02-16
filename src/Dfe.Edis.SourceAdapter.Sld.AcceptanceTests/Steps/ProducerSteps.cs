using System.Linq;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;
using NUnit.Framework;
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
            Assert.AreEqual(_harness.Context.Learners.Count, _harness.DataReceiver.SentLearners.Count());
            foreach (var expectedLearner in _harness.Context.Learners)
            {
                var actualLearner = _harness.DataReceiver.SentLearners.SingleOrDefault(l =>
                    l.Ukprn == expectedLearner.Ukprn &&
                    l.LearnRefNumber == expectedLearner.LearnRefNumber);
                
                Assert.IsNotNull(actualLearner, $"Expected learner with UKPRN {expectedLearner.Ukprn} and LearnRefNumber {expectedLearner.LearnRefNumber}");
                Assert.AreEqual(expectedLearner.Uln, actualLearner.Uln);
                Assert.AreEqual(expectedLearner.FamilyName, actualLearner.FamilyName);
                Assert.AreEqual(expectedLearner.GivenNames, actualLearner.GivenNames);
                Assert.AreEqual(expectedLearner.DateOfBirth, actualLearner.DateOfBirth);
                Assert.AreEqual(expectedLearner.NiNumber, actualLearner.NiNumber);
            }
        }
    }
}