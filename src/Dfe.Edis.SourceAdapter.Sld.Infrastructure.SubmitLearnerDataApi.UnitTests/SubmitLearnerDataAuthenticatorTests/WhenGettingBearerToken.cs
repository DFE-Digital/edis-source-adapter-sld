using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.Configuration;
using Microsoft.Extensions.Logging;
using MockTheWeb;
using Moq;
using NUnit.Framework;
using Times = Moq.Times;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.SubmitLearnerDataApi.UnitTests.SubmitLearnerDataAuthenticatorTests
{
    public class WhenGettingBearerToken
    {
        private HttpClientMock _httpClientMock;
        private SubmitLearnerDataConfiguration _configuration;
        private Mock<ILogger<SubmitLearnerDataAuthenticator>> _loggerMock;
        private SubmitLearnerDataAuthenticator _authenticator;

        [SetUp]
        public void Arrange()
        {
            _httpClientMock = new HttpClientMock();
            _httpClientMock
                .SetDefaultResponse(ResponseBuilder.Json(
                    new OAuth2Token
                    {
                        AccessToken = "someaccesstoken",
                        ExpiresIn = 3600,
                    }, new SystemTextMtwJsonSerializer()));

            _configuration = new SubmitLearnerDataConfiguration
            {
                OAuthTokenEndpoint = "https://localhost:12345/oauth2/token",
                OAuthClientId = "some-client-id",
                OAuthClientSecret = "super-secure-secret",
                OAuthScope = "work",
            };

            _loggerMock = new Mock<ILogger<SubmitLearnerDataAuthenticator>>();

            _authenticator = new SubmitLearnerDataAuthenticator(
                _httpClientMock.AsHttpClient(),
                _configuration,
                _loggerMock.Object);
        }

        [Test]
        public async Task ThenItShouldGetTokenFromServerOnFirstCallAndReturnTheAccessToken()
        {
            var oAuth2Token = new OAuth2Token
            {
                AccessToken = Guid.NewGuid().ToString(),
                ExpiresIn = 3600,
            };
            _httpClientMock
                .SetDefaultResponse(ResponseBuilder.Json(oAuth2Token, new SystemTextMtwJsonSerializer()));

            var actual = await _authenticator.GetBearerTokenAsync(CancellationToken.None);

            Assert.AreEqual(oAuth2Token.AccessToken, actual);
            _httpClientMock.Verify(req => req.RequestUri.ToString().Equals(_configuration.OAuthTokenEndpoint, StringComparison.InvariantCultureIgnoreCase), 
                MockTheWeb.Times.Once());
        }

        [Test]
        public async Task ThenItShouldReturnCachedTokenIfNotExpired()
        {
            var oAuth2Token = new OAuth2Token
            {
                AccessToken = Guid.NewGuid().ToString(),
                ExpiresIn = 3600,
            };
            _httpClientMock
                .SetDefaultResponse(ResponseBuilder.Json(oAuth2Token, new SystemTextMtwJsonSerializer()));

            await _authenticator.GetBearerTokenAsync(CancellationToken.None);
            var actual = await _authenticator.GetBearerTokenAsync(CancellationToken.None);

            Assert.AreEqual(oAuth2Token.AccessToken, actual);
            _httpClientMock.Verify(req => req.RequestUri.ToString().Equals(_configuration.OAuthTokenEndpoint, StringComparison.InvariantCultureIgnoreCase), 
                MockTheWeb.Times.Once());
        }

        [Test]
        public async Task ThenItShouldGetNewTokenItPreviousTokenExpired()
        {
            var oAuth2Token = new OAuth2Token
            {
                AccessToken = Guid.NewGuid().ToString(),
                ExpiresIn = 10,
            };
            _httpClientMock
                .SetDefaultResponse(ResponseBuilder.Json(oAuth2Token, new SystemTextMtwJsonSerializer()));

            await _authenticator.GetBearerTokenAsync(CancellationToken.None);
            await Task.Delay(1000);
            var actual = await _authenticator.GetBearerTokenAsync(CancellationToken.None);

            Assert.AreEqual(oAuth2Token.AccessToken, actual);
            _httpClientMock.Verify(req => req.RequestUri.ToString().Equals(_configuration.OAuthTokenEndpoint, StringComparison.InvariantCultureIgnoreCase), 
                MockTheWeb.Times.Twice());
        }

        [Test]
        public void ThenItShouldThrowAnExceptionIfTheServerReturnsAnErrorResponseCode()
        {
            var errorDetails = new OAuthErrorResult
            {
                Error = "unit-test",
                ErrorDescription = "unit testing error",
                CorrelationId = Guid.NewGuid().ToString(),
                TraceId = Guid.NewGuid().ToString(),
            };
            _httpClientMock.SetDefaultResponse(
                ResponseBuilder
                    .Response()
                    .WithStatus(HttpStatusCode.BadRequest)
                    .WithJsonContent(errorDetails, new SystemTextMtwJsonSerializer()));

            var actual = Assert.ThrowsAsync<SldAuthenticationException>(async () =>
                await _authenticator.GetBearerTokenAsync(CancellationToken.None));
            Assert.AreEqual((int)HttpStatusCode.BadRequest, actual.HttpStatusCode);
            Assert.AreEqual(errorDetails.Error, actual.ErrorResult.Error);
            Assert.AreEqual(errorDetails.ErrorDescription, actual.ErrorResult.ErrorDescription);
            Assert.AreEqual(errorDetails.CorrelationId, actual.ErrorResult.CorrelationId);
            Assert.AreEqual(errorDetails.TraceId, actual.ErrorResult.TraceId);
        }

        [Test]
        public void ThenItShouldThrowAnExceptionIfTheServerReturnsAnErrorResponseCodeWithNonParsableContent()
        {
            _httpClientMock.SetDefaultResponse(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent("not-json"),
                });

            var actual = Assert.ThrowsAsync<SldAuthenticationException>(async () =>
                await _authenticator.GetBearerTokenAsync(CancellationToken.None));
            Assert.AreEqual((int)HttpStatusCode.BadGateway, actual.HttpStatusCode);
            Assert.IsNull(actual.ErrorResult);
        }

        [Test]
        public void ThenItShouldThrowAnExceptionIfTheServerReturnsAnErrorResponseCodeWithNoContent()
        {
            _httpClientMock.SetDefaultResponse(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadGateway,
                });

            var actual = Assert.ThrowsAsync<SldAuthenticationException>(async () =>
                await _authenticator.GetBearerTokenAsync(CancellationToken.None));
            Assert.AreEqual((int)HttpStatusCode.BadGateway, actual.HttpStatusCode);
            Assert.IsNull(actual.ErrorResult);
        }

        [Test]
        public void ThenItShouldThrowAnExceptionIfTheServerReturnsAnSuccessResponseCodeWithNoContent()
        {
            _httpClientMock.SetDefaultResponse(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                });

            var actual = Assert.ThrowsAsync<SldAuthenticationException>(async () =>
                await _authenticator.GetBearerTokenAsync(CancellationToken.None));
            Assert.AreEqual((int)HttpStatusCode.OK, actual.HttpStatusCode);
            Assert.IsNull(actual.ErrorResult);
        }

        [Test]
        public void ThenItShouldThrowAnExceptionIfTheServerReturnsAnOkResponseCodeWithNonParsableContent()
        {
            _httpClientMock.SetDefaultResponse(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("not-json"),
                });

            var actual = Assert.ThrowsAsync<SldAuthenticationException>(async () =>
                await _authenticator.GetBearerTokenAsync(CancellationToken.None));
            Assert.AreEqual((int)HttpStatusCode.OK, actual.HttpStatusCode);
            Assert.IsNull(actual.ErrorResult);
        }
    }
}