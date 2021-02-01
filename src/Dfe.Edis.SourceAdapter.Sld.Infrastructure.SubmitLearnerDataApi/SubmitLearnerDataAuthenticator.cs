using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.Configuration;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.SubmitLearnerDataApi
{
    public interface ISubmitLearnerDataAuthenticator
    {
        Task<string> GetBearerTokenAsync(CancellationToken cancellationToken);
    }

    public class SubmitLearnerDataAuthenticator : ISubmitLearnerDataAuthenticator
    {
        private static readonly SemaphoreSlim TokenLock = new SemaphoreSlim(1, 1);
        
        private readonly HttpClient _httpClient;
        private readonly SubmitLearnerDataConfiguration _configuration;
        private readonly ILogger<SubmitLearnerDataAuthenticator> _logger;
        private OAuth2Token _token;

        public SubmitLearnerDataAuthenticator(
            HttpClient httpClient,
            SubmitLearnerDataConfiguration configuration,
            ILogger<SubmitLearnerDataAuthenticator> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetBearerTokenAsync(CancellationToken cancellationToken)
        {
            if (_token != null && !_token.HasExpired())
            {
                _logger.LogDebug("Returning cached token");
                return _token.AccessToken;
            }

            _logger.LogDebug("Getting new token. Acquiring lock...");
            await TokenLock.WaitAsync(cancellationToken);
            try
            {
                if (_token == null || _token.HasExpired())
                {
                    _logger.LogDebug("Acquiring token from server");
                    _token = await AcquireTokenAsync(cancellationToken);
                    _logger.LogDebug("Acquired token from server");
                }
                else
                {
                    _logger.LogDebug("Token already re-acquired in another thread");
                }
            }
            catch (SldAuthenticationException ex)
            {
                _logger.LogError("Error getting OAuth token for SLD API. Http status = {StatusCode}, Error code = {ErrorCode}, " +
                                 "Error description = {ErrorDescription}, Trace id = {TraceId}, Correlation Id = {CorrelationId}",
                    ex.HttpStatusCode,
                    ex.ErrorResult?.Error ?? "Unknown",
                    ex.ErrorResult?.ErrorDescription ?? "Unknown",
                    ex.ErrorResult?.TraceId ?? "Unknown",
                    ex.ErrorResult?.CorrelationId ?? "Unknown");
                throw;
            }
            finally
            {
                _logger.LogDebug("Releasing lock...");
                TokenLock.Release();
            }

            _logger.LogDebug("Returning new token");
            return _token.AccessToken;
        }

        private async Task<OAuth2Token> AcquireTokenAsync(CancellationToken cancellationToken)
        {
            var body = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"client_id", _configuration.OAuthClientId},
                {"client_secret", _configuration.OAuthClientSecret},
                {"scope", _configuration.OAuthScope},
            };
            var response = await _httpClient.PostAsync(_configuration.OAuthTokenEndpoint, new FormUrlEncodedContent(body), cancellationToken);

            var content = response.Content != null ? await response.Content.ReadAsStringAsync() : null;
            var statusCode = (int) response.StatusCode;
            if (!response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrEmpty(content))
                {
                    OAuthErrorResult errorResult;
                    try
                    {
                        errorResult = JsonSerializer.Deserialize<OAuthErrorResult>(content);
                    }
                    catch
                    {
                        throw new SldAuthenticationException($"Unexpected error occured when requesting auth token. Content = {content}", statusCode);
                    }
                    throw new SldAuthenticationException(errorResult, statusCode);
                }

                throw new SldAuthenticationException($"Unexpected error occured when requesting auth token", statusCode);
            }

            if (string.IsNullOrEmpty(content))
            {
                throw new SldAuthenticationException($"Unexpected error occured when requesting auth token. Status code ({statusCode}) was a success but not content returned", statusCode);
            }

            try
            {
                return JsonSerializer.Deserialize<OAuth2Token>(content);
            }
            catch (Exception ex)
            {
                throw new SldAuthenticationException($"Error parsing content from auth server - {ex.Message}. Content = {content}", statusCode, ex);
            }
        }
    }
}