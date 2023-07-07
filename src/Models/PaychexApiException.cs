using System;
using System.Collections.Generic;
using System.Linq;

namespace Paychex.Api.Models
{
    /// <summary>
    /// Exception thrown when an error occurs during calls to the Paychex Api.
    /// Subclasses AggregateException because the errors returned by the Api are a collection, so there's the 'possibility' of more than one being returned.
    /// </summary>
    [Serializable]
    public class PaychexApiException : AggregateException
    {
        /// <summary>
        /// The uri that caused the exception
        /// </summary>
        public string ResponseUri { get; }

        /// <summary>
        /// Create from the Api Response's error collection
        /// </summary>
        /// <param name="responseUri"></param>
        /// <param name="errors"></param>
        public PaychexApiException(Uri responseUri, IEnumerable<ApiError> errors) :
            base(errors?.Select(x => new PaychexException(x)) ?? new PaychexException[] { } )
        {
            ResponseUri = responseUri?.OriginalString ?? "URL not available";
        }

        /// <summary>
        /// Default empty constructor
        /// </summary>
        public PaychexApiException() { }

        /// <summary>
        /// Override of base class constructor
        /// </summary>
        public PaychexApiException(IEnumerable<Exception> innerExceptions) : base(innerExceptions) { }

        /// <summary>
        /// Override of base class constructor
        /// </summary>
        public PaychexApiException(params Exception[] innerExceptions) : base(innerExceptions) { }

        /// <summary>
        /// Override of base class constructor
        /// </summary>
        public PaychexApiException(Uri responseUri, string message) : base(message) {
            ResponseUri = responseUri.OriginalString;
        }

        /// <summary>
        /// Override of base class constructor
        /// </summary>
        public PaychexApiException(string message, IEnumerable<Exception> innerExceptions) : 
            base(message, innerExceptions) { }

        /// <summary>
        /// Override of base class constructor
        /// </summary>
        public PaychexApiException(string message, Exception innerException) :
            base(message, innerException) { }

        /// <summary>
        /// Override of base class constructor
        /// </summary>
        public PaychexApiException(string message, params Exception[] innerExceptions) :
            base(message, innerExceptions) { }

        /// <summary>
        /// n/a
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected PaychexApiException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) :
            base(info, context)
        { }

        /// <summary>
        /// ToString override
        /// </summary>
        /// <returns>This exception's message plus all inner exception messages</returns>
        public override string ToString() =>
            string.Join("\n", new[] { base.Message }
                  .Concat(InnerExceptions.Select(x => x.ToString())
                                         .ToArray())
            );

        /// <summary>
        /// Hack
        /// </summary>
        public override string Message => ToString();
    }
}
