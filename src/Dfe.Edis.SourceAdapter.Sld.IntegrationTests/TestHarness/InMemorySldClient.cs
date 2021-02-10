using System;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;

namespace Dfe.Edis.SourceAdapter.Sld.IntegrationTests.TestHarness
{
    public class InMemorySldClient : ISldClient
    {
        public Task<string[]> ListAcademicYearsAsync(CancellationToken cancellationToken)
        {
            var academicYears = new string[0];
            return Task.FromResult(academicYears);
        }

        public Task<SldPagedResult<int>> ListProvidersThatHaveSubmittedSinceAsync(string academicYear, DateTime? submittedSince, int pageNumber, CancellationToken cancellationToken)
        {
            var providers = new SldPagedResult<int>
            {
                Items = new int[0],
                PageNumber = pageNumber,
                PageSize = 0,
                TotalNumberOfItems = 0,
                TotalNumberOfPages = 0,
            };
            return Task.FromResult(providers);
        }

        public Task<SldPagedResult<Learner>> ListLearnersForProviderAsync(string academicYear, int ukprn, int pageNumber, CancellationToken cancellationToken)
        {
            var learners = new SldPagedResult<Learner>
            {
                Items = new Learner[0],
                PageNumber = pageNumber,
                PageSize = 0,
                TotalNumberOfItems = 0,
                TotalNumberOfPages = 0,
            };
            return Task.FromResult(learners);
        }
    }
}