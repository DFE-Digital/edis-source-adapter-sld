using System;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.SubmitLearnerDataApi
{
    public class SldApiException : Exception
    {
        public int HttpStatusCode { get; }

        public SldApiException(string message, int httpStatusCode)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}