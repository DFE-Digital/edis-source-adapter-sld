using System;
using System.Text.Json.Serialization;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.SubmitLearnerDataApi
{
    public class OAuth2Token
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        
        [JsonIgnore]
        public DateTime AcquiredAt { get; set; } = DateTime.Now;

        public bool HasExpired(int toleranceInSeconds = 10)
        {
            if (ExpiresIn < 1)
            {
                return false;
            }

            var expiresAt = AcquiredAt.AddSeconds(ExpiresIn - toleranceInSeconds);
            return DateTime.Now >= expiresAt;
        }
    }
}