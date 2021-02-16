using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;

namespace Dfe.Edis.SourceAdapter.Sld.AcceptanceTests.TestHarness
{
    public class InMemorySldClient : ISldClient
    {
        private readonly Dictionary<string, List<Learner>> _learners = new Dictionary<string, List<Learner>>();

        public Task<string[]> ListAcademicYearsAsync(CancellationToken cancellationToken)
        {
            var academicYears = _learners.Keys.OrderByDescending(x => x).ToArray();
            return Task.FromResult(academicYears);
        }

        public Task<SldPagedResult<int>> ListProvidersThatHaveSubmittedSinceAsync(string academicYear, DateTime? submittedSince, int pageNumber,
            CancellationToken cancellationToken)
        {
            var learnersForAcademicYear = GetLearnersOfAcademicYear(academicYear);
            var providers = learnersForAcademicYear
                .Select(l => l.Ukprn)
                .Distinct()
                .ToArray();

            var page = GetPageOfItems(providers, pageNumber, 20);
            return Task.FromResult(page);
        }

        public Task<SldPagedResult<Learner>> ListLearnersForProviderAsync(string academicYear, int ukprn, int pageNumber, CancellationToken cancellationToken)
        {
            var learnersForAcademicYear = GetLearnersOfAcademicYear(academicYear);
            var learnersForProvider = learnersForAcademicYear.Where(l => l.Ukprn == ukprn).ToArray();
            
            var page = GetPageOfItems(learnersForProvider, pageNumber, 20);
            return Task.FromResult(page);
        }

        public void Reset()
        {
            _learners.Clear();
        }

        public void AddLearnersToAcademicYear(string academicYear, params Learner[] learners)
        {
            var learnersInAcademicYear = GetLearnersOfAcademicYear(academicYear);
            foreach (var learner in learners)
            {
                var existingLearner = learnersInAcademicYear.SingleOrDefault(l => l.Ukprn == learner.Ukprn && l.LearnRefNumber == learner.LearnRefNumber);
                if (existingLearner != null)
                {
                    learnersInAcademicYear.Remove(existingLearner);
                }

                learnersInAcademicYear.Add(learner);
            }
        }

        private List<Learner> GetLearnersOfAcademicYear(string academicYear)
        {
            if (!_learners.ContainsKey(academicYear.ToLower()))
            {
                _learners.Add(academicYear.ToLower(), new List<Learner>());
            }

            return _learners[academicYear.ToLower()];
        }

        private SldPagedResult<T> GetPageOfItems<T>(T[] allItems, int pageNumber, int pageSize)
        {
            var skip = (pageNumber - 1) * pageSize;
            return new SldPagedResult<T>
            {
                Items = allItems.Skip(skip).Take(pageSize).ToArray(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalNumberOfItems = allItems.Length,
                TotalNumberOfPages = (int) Math.Ceiling(allItems.Length / (float) pageSize),
            };
        }
    }
}