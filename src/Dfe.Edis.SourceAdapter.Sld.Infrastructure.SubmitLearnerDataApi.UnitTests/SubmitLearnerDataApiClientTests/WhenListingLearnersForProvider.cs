using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;
using Microsoft.Extensions.Logging;
using MockTheWeb;
using Moq;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.SubmitLearnerDataApi.UnitTests.SubmitLearnerDataApiClientTests
{
    public class WhenListingLearnersForProvider
    {
        private HttpClientMock _httpClientMock;
        private Mock<ISubmitLearnerDataAuthenticator> _submitLearnerDataAuthenticatorMock;
        private SubmitLearnerDataConfiguration _configuration;
        private Mock<ILogger<SubmitLearnerDataApiClient>> _loggerMock;
        private SubmitLearnerDataApiClient _apiClient;

        [SetUp]
        public void Arrange()
        {
            _httpClientMock = new HttpClientMock();
            _httpClientMock.SetDefaultResponse(
                ResponseBuilder.Json(new[]
                {
                    new Learner(),
                }, new SystemTextMtwJsonSerializer()));

            _submitLearnerDataAuthenticatorMock = new Mock<ISubmitLearnerDataAuthenticator>();
            _submitLearnerDataAuthenticatorMock.Setup(authenticator => authenticator.GetBearerTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("bearer-token");

            _configuration = new SubmitLearnerDataConfiguration
            {
                BaseUrl = "https://localhost:12345/sld",
            };

            _loggerMock = new Mock<ILogger<SubmitLearnerDataApiClient>>();

            _apiClient = new SubmitLearnerDataApiClient(
                _httpClientMock.AsHttpClient(),
                _submitLearnerDataAuthenticatorMock.Object,
                _configuration,
                _loggerMock.Object);
        }

        [Test]
        public async Task ThenItShouldUseBearerTokenFromAuthenticator()
        {
            var bearerToken = Guid.NewGuid().ToString();
            _submitLearnerDataAuthenticatorMock.Setup(authenticator => authenticator.GetBearerTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(bearerToken);
            var cancellationToken = new CancellationToken();

            await _apiClient.ListLearnersForProviderAsync("2021", 12345678, 1, cancellationToken);

            _submitLearnerDataAuthenticatorMock.Verify(authenticator => authenticator.GetBearerTokenAsync(cancellationToken),
                Moq.Times.Once);
            _httpClientMock.Verify(req =>
                req.Headers.Contains("Authorization") &&
                req.Headers.GetValues("Authorization").Single() == $"Bearer {bearerToken}");
        }

        [Test]
        public async Task ThenItShouldCallV1LearnersEndpoint()
        {
            await _apiClient.ListLearnersForProviderAsync("2021", 12345678, 1, CancellationToken.None);

            var expectedUrl = new Uri(
                new Uri(_configuration.BaseUrl, UriKind.Absolute),
                new Uri($"/api/v1/ilr-data/learners/2021?ukprn=12345678&pageNumber=1", UriKind.Relative)).ToString();
            _httpClientMock.Verify(req =>
                req.RequestUri.ToString().Equals(expectedUrl, StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public async Task ThenItShouldReturnLearnersFromServer()
        {
            var learner = new Learner
            {
                Ukprn = 12345678,
                LearnRefNumber = "ABC123",
                Uln = 9010000000,
                FamilyName = "User",
                GivenNames = "Test",
                DateOfBirth = DateTime.Today,
                NiNumber = "AB123456A",
                LearningDeliveries = new[]
                {
                    new LearningDelivery
                    {
                        AimType = 99,
                        LearnStartDate = DateTime.Today.AddDays(-10),
                        LearnPlanEndDate = DateTime.Today.AddDays(-1),
                        FundModel = 23,
                        StdCode = 9658,
                        DelLocPostCode = "AB12 3LK",
                        EpaOrgId = "KSM63",
                        CompStatus = 81,
                        LearnActEndDate = DateTime.Today.AddDays(-2),
                        WithdrawReason = 25,
                        Outcome = 851,
                        AchDate = DateTime.Today.AddDays(-63),
                        OutGrade = "pass",
                        ProgType = 54,
                    },
                },
            };
            _httpClientMock.SetDefaultResponse(
                ResponseBuilder.Json(new[] {learner}, new SystemTextMtwJsonSerializer()));
        
            var actual = await _apiClient.ListLearnersForProviderAsync("2021", 12345678, 1, CancellationToken.None);
        
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Items);
            Assert.AreEqual(1, actual.Items.Length);
            Assert.AreEqual(learner.Ukprn, actual.Items[0].Ukprn);
            Assert.AreEqual(learner.LearnRefNumber, actual.Items[0].LearnRefNumber);
            Assert.AreEqual(learner.Uln, actual.Items[0].Uln);
            Assert.AreEqual(learner.FamilyName, actual.Items[0].FamilyName);
            Assert.AreEqual(learner.GivenNames, actual.Items[0].GivenNames);
            Assert.AreEqual(learner.DateOfBirth, actual.Items[0].DateOfBirth);
            Assert.AreEqual(learner.NiNumber, actual.Items[0].NiNumber);
            Assert.IsNotNull(actual.Items[0].LearningDeliveries);
            Assert.AreEqual(1, actual.Items[0].LearningDeliveries.Length);
            Assert.AreEqual(learner.LearningDeliveries[0].AimType, actual.Items[0].LearningDeliveries[0].AimType);
            Assert.AreEqual(learner.LearningDeliveries[0].LearnStartDate, actual.Items[0].LearningDeliveries[0].LearnStartDate);
            Assert.AreEqual(learner.LearningDeliveries[0].LearnPlanEndDate, actual.Items[0].LearningDeliveries[0].LearnPlanEndDate);
            Assert.AreEqual(learner.LearningDeliveries[0].FundModel, actual.Items[0].LearningDeliveries[0].FundModel);
            Assert.AreEqual(learner.LearningDeliveries[0].StdCode, actual.Items[0].LearningDeliveries[0].StdCode);
            Assert.AreEqual(learner.LearningDeliveries[0].DelLocPostCode, actual.Items[0].LearningDeliveries[0].DelLocPostCode);
            Assert.AreEqual(learner.LearningDeliveries[0].EpaOrgId, actual.Items[0].LearningDeliveries[0].EpaOrgId);
            Assert.AreEqual(learner.LearningDeliveries[0].CompStatus, actual.Items[0].LearningDeliveries[0].CompStatus);
            Assert.AreEqual(learner.LearningDeliveries[0].LearnActEndDate, actual.Items[0].LearningDeliveries[0].LearnActEndDate);
            Assert.AreEqual(learner.LearningDeliveries[0].WithdrawReason, actual.Items[0].LearningDeliveries[0].WithdrawReason);
            Assert.AreEqual(learner.LearningDeliveries[0].Outcome, actual.Items[0].LearningDeliveries[0].Outcome);
            Assert.AreEqual(learner.LearningDeliveries[0].AchDate, actual.Items[0].LearningDeliveries[0].AchDate);
            Assert.AreEqual(learner.LearningDeliveries[0].OutGrade, actual.Items[0].LearningDeliveries[0].OutGrade);
            Assert.AreEqual(learner.LearningDeliveries[0].ProgType, actual.Items[0].LearningDeliveries[0].ProgType);
        }

        [Test]
        public async Task ThenItShouldReturnEmptyPageOfResultsIfServerInPeriodEnd()
        {
            _httpClientMock.SetDefaultResponse(
                ResponseBuilder.Response().WithStatus(HttpStatusCode.NoContent));
        
            var actual = await _apiClient.ListLearnersForProviderAsync("2021", 12345678, 1, CancellationToken.None);
            
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Items);
            Assert.AreEqual(0, actual.Items.Length);
            Assert.AreEqual(0, actual.PageNumber);
            Assert.AreEqual(0, actual.PageSize);
            Assert.AreEqual(0, actual.TotalNumberOfItems);
            Assert.AreEqual(0, actual.TotalNumberOfPages);
        }

        [Test]
        public async Task ThenItShouldIncludePaginationInfoIfSentFromServer()
        {
            var totalItems = 10;
            var pageSize = 1;
            var pageNumber = 2;
            var totalPages = 9;
            _httpClientMock.SetDefaultResponse(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new[] {new Learner()})),
                    Headers =
                    {
                        {
                            "X-Pagination",
                            "{" +
                            $"\"totalItems\":{totalItems}," +
                            $"\"pageSize\":{pageSize}," +
                            $"\"pageNumber\":{pageNumber}," +
                            $"\"totalPages\":{totalPages}" +
                            "}"
                        }
                    }
                });

            var actual = await _apiClient.ListLearnersForProviderAsync("2021", 12345678, 1, CancellationToken.None);

            Assert.IsNotNull(actual);
            Assert.AreEqual(totalItems, actual.TotalNumberOfItems);
            Assert.AreEqual(pageSize, actual.PageSize);
            Assert.AreEqual(pageNumber, actual.PageNumber);
            Assert.AreEqual(totalPages, actual.TotalNumberOfPages);
        }

        [Test]
        public async Task ThenItShouldZeroPaginationInfoIfNotSentFromServer()
        {
            _httpClientMock.SetDefaultResponse(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new[] {new Learner()})),
                });

            var actual = await _apiClient.ListLearnersForProviderAsync("2021", 12345678, 1, CancellationToken.None);

            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.TotalNumberOfItems);
            Assert.AreEqual(0, actual.PageSize);
            Assert.AreEqual(0, actual.PageNumber);
            Assert.AreEqual(0, actual.TotalNumberOfPages);
        }

        [Test]
        public async Task ThenItShouldReturnEmptyResultIfServerInPeriodEnd()
        {
            _httpClientMock.SetDefaultResponse(
                ResponseBuilder.Response().WithStatus(HttpStatusCode.NoContent));

            var actual = await _apiClient.ListLearnersForProviderAsync("2021", 12345678, 1, CancellationToken.None);

            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.TotalNumberOfItems);
            Assert.AreEqual(0, actual.PageSize);
            Assert.AreEqual(0, actual.PageNumber);
            Assert.AreEqual(0, actual.TotalNumberOfPages);
            Assert.IsNotNull(actual.Items);
            Assert.AreEqual(0, actual.Items.Length);
        }
    }
}