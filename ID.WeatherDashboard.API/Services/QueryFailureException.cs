using System;
using System.Net;

namespace ID.WeatherDashboard.API.Services
{
    /// <summary>
    ///     Represents a failure during an HTTP query operation.
    /// </summary>
    public class QueryFailureException : Exception
    {
        /// <summary>
        ///     Gets the HTTP status code returned by the service, if any.
        /// </summary>
        public HttpStatusCode? StatusCode { get; }

        /// <summary>
        ///     Gets the response content returned by the service, if any.
        /// </summary>
        public string? ResponseContent { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryFailureException"/> class with a default message.
        /// </summary>
        public QueryFailureException()
            : base("The query operation failed.") { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryFailureException"/> class with a custom message.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public QueryFailureException(string message)
            : base(message) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryFailureException"/> class with a custom message and an inner exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public QueryFailureException(string message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryFailureException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="statusCode">The HTTP status code returned.</param>
        /// <param name="responseContent">The body of the response.</param>
        public QueryFailureException(string message, HttpStatusCode statusCode, string? responseContent = null)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryFailureException"/> class with an inner exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="statusCode">The HTTP status code returned.</param>
        /// <param name="responseContent">The body of the response.</param>
        /// <param name="innerException">The inner exception.</param>
        public QueryFailureException(string message, HttpStatusCode statusCode, string? responseContent, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }
    }
}
