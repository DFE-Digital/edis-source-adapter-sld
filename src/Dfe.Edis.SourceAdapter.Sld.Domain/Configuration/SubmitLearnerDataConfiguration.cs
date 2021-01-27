namespace Dfe.Edis.SourceAdapter.Sld.Domain.Configuration
{
    public class SubmitLearnerDataConfiguration
    {
        public string BaseUrl { get; set; }
        public string OAuthTokenEndpoint { get; set; }
        public string OAuthClientId { get; set; }
        public string OAuthClientSecret { get; set; }
        public string OAuthScope { get; set; }
    }
}