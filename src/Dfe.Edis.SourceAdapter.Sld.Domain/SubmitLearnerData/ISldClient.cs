using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData
{
    public interface ISldClient
    {
        Task<string[]> ListAcademicYearsAsync(CancellationToken cancellationToken);
        Task<SldPagedResult<int>> ListProvidersThatHaveSubmittedSinceAsync(string academicYear, DateTime? submittedSince, int pageNumber, CancellationToken cancellationToken);
        Task<SldPagedResult<Learner>> ListLearnersForProviderAsync(string academicYear, int ukprn, int pageNumber, CancellationToken cancellationToken);
    }
}