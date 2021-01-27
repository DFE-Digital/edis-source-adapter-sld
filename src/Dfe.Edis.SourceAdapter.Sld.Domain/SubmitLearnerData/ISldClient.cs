using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData
{
    public interface ISldClient
    {
        Task<string[]> ListAcademicYearsAsync(CancellationToken cancellationToken);
        Task<SldPagedResult<int>> ListProvidersThatHaveSubmittedSince(string academicYear, DateTime? submittedSince, int pageNumber, CancellationToken cancellationToken);
    }
}