using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.Configuration;
using Microsoft.Extensions.Logging;
using MockTheWeb;
using Moq;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.SubmitLearnerDataApi.UnitTests.SubmitLearnerDataApiClientTests
{
    public class WhenListingProvidersThatHaveSubmittedSince
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
                ResponseBuilder.Json(new[] {10000000, 10000001}, new SystemTextMtwJsonSerializer()));

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

            await _apiClient.ListProvidersThatHaveSubmittedSince("2021", null, 1, cancellationToken);

            _submitLearnerDataAuthenticatorMock.Verify(authenticator => authenticator.GetBearerTokenAsync(cancellationToken),
                Moq.Times.Once);
            _httpClientMock.Verify(req =>
                req.Headers.Contains("Authorization") &&
                req.Headers.GetValues("Authorization").Single() == $"Bearer {bearerToken}");
        }

        [TestCase("2021", 1)]
        [TestCase("2021", 2)]
        [TestCase("2021", 3)]
        [TestCase("2020", 1)]
        [TestCase("2020", 2)]
        [TestCase("2020", 3)]
        public async Task ThenItShouldCallV1ProvidersEndpointWithAcademicYearAndPageNumber(string academicYear, int pageNumber)
        {
            await _apiClient.ListProvidersThatHaveSubmittedSince(academicYear, null, pageNumber, CancellationToken.None);

            var expectedUrl = new Uri(
                new Uri(_configuration.BaseUrl, UriKind.Absolute),
                new Uri($"/api/v1/ilr-data/providers/{academicYear}?pageNumber={pageNumber}", UriKind.Relative)).ToString();
            _httpClientMock.Verify(req =>
                req.RequestUri.ToString().Equals(expectedUrl, StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public async Task ThenItShouldCallV1ProvidersEndpointIncludingStartTimeIfSubmittedSinceNotNull()
        {
            await _apiClient.ListProvidersThatHaveSubmittedSince("2021", new DateTime(2021, 1, 28), 1, CancellationToken.None);

            var expectedUrl = new Uri(
                new Uri(_configuration.BaseUrl, UriKind.Absolute),
                new Uri($"/api/v1/ilr-data/providers/2021?pageNumber=1&startDateTime=2021-01-28T00:00:00Z", UriKind.Relative)).ToString();
            _httpClientMock.Verify(req =>
                req.RequestUri.ToString().Equals(expectedUrl, StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public async Task ThenItShouldReturnUkprnsFromServer()
        {
            var ukprn1 = 10000000;
            var ukprn2 = 10000001;
            var ukprn3 = 10000002;
            _httpClientMock.SetDefaultResponse(
                ResponseBuilder.Json(new[] {ukprn1, ukprn2, ukprn3}, new SystemTextMtwJsonSerializer()));

            var actual = await _apiClient.ListProvidersThatHaveSubmittedSince("2021", null, 1, CancellationToken.None);

            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Items);
            Assert.AreEqual(3, actual.Items.Length);
            Assert.AreEqual(ukprn1, actual.Items[0]);
            Assert.AreEqual(ukprn2, actual.Items[1]);
            Assert.AreEqual(ukprn3, actual.Items[2]);
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
                    Content = new StringContent(JsonSerializer.Serialize(new[] {10000000})),
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

            var actual = await _apiClient.ListProvidersThatHaveSubmittedSince("2021", null, 1, CancellationToken.None);

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
                    Content = new StringContent(JsonSerializer.Serialize(new[] {10000000})),
                });

            var actual = await _apiClient.ListProvidersThatHaveSubmittedSince("2021", null, 1, CancellationToken.None);

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

            var actual = await _apiClient.ListProvidersThatHaveSubmittedSince("2021", null, 1, CancellationToken.None);

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