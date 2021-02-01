using System;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.SubmitLearnerDataApi
{
    public class SldAuthenticationException : Exception
    {
        public OAuthErrorResult ErrorResult { get; }
        public int HttpStatusCode { get; }

        public SldAuthenticationException(string message, int httpStatusCode)
            :base(message)
        {
            HttpStatusCode = httpStatusCode;
        }

        public SldAuthenticationException(string message, int httpStatusCode, Exception innerException)
            :base(message, innerException)
        {
            HttpStatusCode = httpStatusCode;
        }

        public SldAuthenticationException(OAuthErrorResult errorResult, int httpStatusCode)
            : base(errorResult.ErrorDescription)
        {
            ErrorResult = errorResult;
            HttpStatusCode = httpStatusCode;
        }
    }
}