using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.SubmitLearnerDataApi
{
    public class SubmitLearnerDataApiClient : ISldClient
    {
        private readonly HttpClient _httpClient;
        private readonly ISubmitLearnerDataAuthenticator _submitLearnerDataAuthenticator;
        private readonly SubmitLearnerDataConfiguration _configuration;
        private readonly ILogger<SubmitLearnerDataApiClient> _logger;

        public SubmitLearnerDataApiClient(
            HttpClient httpClient,
            ISubmitLearnerDataAuthenticator submitLearnerDataAuthenticator,
            SubmitLearnerDataConfiguration configuration,
            ILogger<SubmitLearnerDataApiClient> logger)
        {
            _httpClient = httpClient;
            _submitLearnerDataAuthenticator = submitLearnerDataAuthenticator;
            _configuration = configuration;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_configuration.BaseUrl);
        }

        public async Task<string[]> ListAcademicYearsAsync(CancellationToken cancellationToken)
        {
            var bearerToken = await _submitLearnerDataAuthenticator.GetBearerTokenAsync(cancellationToken);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("/api/v1/academic-years", UriKind.Relative),
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", bearerToken),
                },
            };
            var result = await SendAndParseAsync<int[]>(request, cancellationToken);

            if (result.IsInPeriodEnd)
            {
                _logger.LogWarning("Unable to list academic years as SLD is currently in period end");
                return new string[0];
            }

            return result.Data.Select(x => x.ToString()).ToArray();
        }

        public async Task<SldPagedResult<int>> ListProvidersThatHaveSubmittedSince(
            string academicYear,
            DateTime? submittedSince,
            int pageNumber,
            CancellationToken cancellationToken)
        {
            var bearerToken = await _submitLearnerDataAuthenticator.GetBearerTokenAsync(cancellationToken);
            var url = $"/api/v1/ilr-data/providers/{academicYear}?pageNumber={pageNumber}";
            if (submittedSince.HasValue)
            {
                url += $"&startDateTime={submittedSince.Value.ToUniversalTime():yyyy-MM-ddTHH:mm:ss}Z";
            }

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url, UriKind.Relative),
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", bearerToken),
                },
            };
            var result = await SendAndParseAsync<int[]>(request, cancellationToken);

            if (result.IsInPeriodEnd)
            {
                _logger.LogWarning($"Unable to list providers in {academicYear} that have changed since {submittedSince} as SLD is currently in period end");
                return new SldPagedResult<int>
                {
                    Items = new int[0],
                    TotalNumberOfItems = 0,
                    PageNumber = 0,
                    PageSize = 0,
                    TotalNumberOfPages = 0,
                };
            }

            return new SldPagedResult<int>
            {
                Items = result.Data,
                TotalNumberOfItems = result.PaginationInfo?.TotalItems ?? 0,
                PageNumber = result.PaginationInfo?.PageNumber ?? 0,
                PageSize = result.PaginationInfo?.PageSize ?? 0,
                TotalNumberOfPages = result.PaginationInfo?.TotalPages ?? 0,
            };
        }

        private Task<SldResult<T>> SendAndParseAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return SendAndParseAsync(request, default(T), cancellationToken);
        }

        private async Task<SldResult<T>> SendAndParseAsync<T>(HttpRequestMessage request, T defaultValue, CancellationToken cancellationToken)
        {
            // Call server
            var response = await SendAndHandleErrorsAsync(request, cancellationToken);

            // Handle 404
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new SldResult<T>
                {
                    Data = defaultValue,
                };
            }

            // Check for SLD period end status
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return new SldResult<T>
                {
                    IsInPeriodEnd = true,
                    Data = defaultValue,
                };
            }

            // Parse content
            var content = response.Content != null ? await response.Content.ReadAsStringAsync() : null;
            var statusCode = (int) response.StatusCode;

            if (string.IsNullOrEmpty(content))
            {
                throw new SldApiException($"Unexpected result from SLD API ({request.RequestUri}). Status {statusCode} is success but no content.", statusCode);
            }

            SldPaginationInfo paginationInfo = null;
            var paginationHeader = response.Headers.Contains("X-Pagination")
                ? response.Headers.GetValues("X-Pagination")?.FirstOrDefault()
                : null;
            if (!string.IsNullOrEmpty(paginationHeader))
            {
                try
                {
                    paginationInfo = JsonSerializer.Deserialize<SldPaginationInfo>(paginationHeader);
                }
                catch (Exception ex)
                {
                    throw new SldApiException($"Error parsing pagination header from SLD API ({request.RequestUri}) - {ex.Message}. " +
                                              $"Http status = {statusCode}, Pagination header = {paginationHeader}",
                        statusCode);
                }
            }

            try
            {
                var data = JsonSerializer.Deserialize<T>(content);
                return new SldResult<T>
                {
                    Data = data,
                    PaginationInfo = paginationInfo,
                };
            }
            catch (Exception ex)
            {
                throw new SldApiException($"Error parsing response from SLD API ({request.RequestUri}) - {ex.Message}. " +
                                          $"Http status = {statusCode}, Content = {content}",
                    statusCode);
            }
        }

        private async Task<HttpResponseMessage> SendAndHandleErrorsAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
            {
                var content = response.Content != null ? await response.Content.ReadAsStringAsync() : null;
                var statusCode = (int) response.StatusCode;

                throw new SldApiException($"Error occured calling SLD API ({request.RequestUri}). Http status = {statusCode}, Content = {content}",
                    statusCode);
            }

            return response;
        }

        private class SldResult<T>
        {
            public bool IsInPeriodEnd { get; set; }
            public T Data { get; set; }
            public SldPaginationInfo PaginationInfo { get; set; }
        }

        private class SldPaginationInfo
        {
            [JsonPropertyName("totalItems")] public int TotalItems { get; set; }

            [JsonPropertyName("pageNumber")] public int PageNumber { get; set; }

            [JsonPropertyName("pageSize")] public int PageSize { get; set; }

            [JsonPropertyName("totalPages")] public int TotalPages { get; set; }
        }
    }
}