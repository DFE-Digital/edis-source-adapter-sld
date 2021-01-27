using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.Configuration;
using Microsoft.Extensions.Logging;
using MockTheWeb;
using Moq;
using NUnit.Framework;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.SubmitLearnerDataApi.UnitTests.SubmitLearnerDataApiClientTests
{
    public class WhenListingAcademicYears
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
                ResponseBuilder.Json(new[] {2021}, new SystemTextMtwJsonSerializer()));

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

            await _apiClient.ListAcademicYearsAsync(cancellationToken);
            
            _submitLearnerDataAuthenticatorMock.Verify(authenticator=>authenticator.GetBearerTokenAsync(cancellationToken),
                Moq.Times.Once);
            _httpClientMock.Verify(req => 
                req.Headers.Contains("Authorization") &&
                req.Headers.GetValues("Authorization").Single() == $"Bearer {bearerToken}");
        }

        [Test]
        public async Task ThenItShouldCallV1AcademicYearsEndpoint()
        {
            await _apiClient.ListAcademicYearsAsync(CancellationToken.None);

            var expectedUrl = new Uri(
                new Uri(_configuration.BaseUrl, UriKind.Absolute),
                new Uri("/api/v1/academic-years", UriKind.Relative)).ToString();
            _httpClientMock.Verify(req => 
                req.RequestUri.ToString().Equals(expectedUrl, StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public async Task ThenItShouldReturnListOfAcademicYearsFromServer()
        {
            _httpClientMock.SetDefaultResponse(
                ResponseBuilder.Json(new[] {2020, 2019}, new SystemTextMtwJsonSerializer()));

            var actual = await _apiClient.ListAcademicYearsAsync(CancellationToken.None);
            
            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Length);
            Assert.AreEqual("2020", actual[0]);
            Assert.AreEqual("2019", actual[1]);
        }

        [Test]
        public async Task ThenItShouldReturnEmptyArrayIfServerInPeriodEnd()
        {
            _httpClientMock.SetDefaultResponse(
                ResponseBuilder.Response().WithStatus(HttpStatusCode.NoContent));

            var actual = await _apiClient.ListAcademicYearsAsync(CancellationToken.None);
            
            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Length);
        }
    }
}