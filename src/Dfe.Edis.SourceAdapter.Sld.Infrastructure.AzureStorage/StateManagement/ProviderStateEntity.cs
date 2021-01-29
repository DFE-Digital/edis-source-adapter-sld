using Microsoft.Azure.Cosmos.Table;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.AzureStorage.StateManagement
{
    public class ProviderStateEntity : TableEntity
    {
        public string AcademicYear { get; set; }
        public int Ukprn { get; set; }
        public int NumberOfLearners { get; set; }
        public int IgnoredSubmissionCount { get; set; }
    }
}