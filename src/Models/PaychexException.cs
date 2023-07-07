using System;

namespace Paychex.Api.Models
{
    /// <summary>
    /// Create an exception from the Paychex Api error collection
    /// </summary>
    [Serializable]
    public class PaychexException : Exception
    {
        /// <summary>
        /// Override of base constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public PaychexException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Create exception based on ApiError POCO
        /// </summary>
        /// <param name="e"></param>
        public PaychexException(ApiError e) : base(e.description) =>
            PaychexApiError = e;

        /// <summary>
        /// Override of base constructor
        /// </summary>
        /// <param name="er"></param>
        public PaychexException(PaychexError er) : base(er.error_description) =>
            PaychexApiError = new ApiError(er);

        /// <summary>
        /// Override of base constructor
        /// </summary>
        public PaychexException() { }

        /// <summary>
        /// Override of base constructor
        /// </summary>
        /// <param name="message"></param>
        public PaychexException(string message) : base(message) { }

        /// <summary>
        /// n/a
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected PaychexException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) :
            base(info, context)
        { }

        /// <summary>
        /// Cast the error data to the ApiError POCO
        /// </summary>
        public ApiError PaychexApiError { get; }

        /// <summary>
        /// Debug info that shows the api error response and the transaction id
        /// </summary>
        /// <returns></returns>
        public override string ToString() => PaychexApiError.ToString();
    }
}
