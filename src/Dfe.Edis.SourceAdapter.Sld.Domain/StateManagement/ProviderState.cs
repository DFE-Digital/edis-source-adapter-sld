namespace Dfe.Edis.SourceAdapter.Sld.Domain.StateManagement
{
    public class ProviderState
    {
        public string AcademicYear { get; set; }
        public int Ukprn { get; set; }
        public int NumberOfLearners { get; set; }
        public int IgnoredSubmissionCount { get; set; }
    }
}