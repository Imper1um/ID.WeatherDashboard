using System;
using System.Net;

namespace ID.WeatherDashboard.API.Services
{
    public class QueryFailureException : Exception
    {
        public HttpStatusCode? StatusCode { get; }
        public string? ResponseContent { get; }

        public QueryFailureException()
            : base("The query operation failed.") { }

        public QueryFailureException(string message)
            : base(message) { }

        public QueryFailureException(string message, Exception innerException)
            : base(message, innerException) { }

        public QueryFailureException(string message, HttpStatusCode statusCode, string? responseContent = null)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }

        public QueryFailureException(string message, HttpStatusCode statusCode, string? responseContent, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }
    }
}
