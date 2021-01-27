using System.Text.Json.Serialization;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.SubmitLearnerDataApi
{
    public class OAuthErrorResult
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }
        
        [JsonPropertyName("error_description")]
        public string ErrorDescription { get; set; }
        
        [JsonPropertyName("error_codes")]
        public int[] ErrorCodes { get; set; }
        
        [JsonPropertyName("trace_id")]
        public string TraceId { get; set; }
        
        [JsonPropertyName("correlation_id")]
        public string CorrelationId { get; set; }
    }
}