namespace Dfe.Edis.SourceAdapter.Sld.Domain.Configuration
{
    public class RootAppConfiguration
    {
        public StateConfiguration State { get; set; }
        public QueuingConfiguration Queuing { get; set; }
        public SubmitLearnerDataConfiguration SubmitLearnerData { get; set; }
    }
}