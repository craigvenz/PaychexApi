using System;
using Paychex.Api.Models;

namespace Paychex.Api.Api
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class PaychexAuthenticationException : PaychexException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pae"></param>
        public PaychexAuthenticationException(PaychexError pae) : base(new ApiError(pae))
        {
            Data.Add("PaychexAuthError", pae);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        public PaychexAuthenticationException(ApiError error) : base(error) { }

        /// <inheritdoc />
        public PaychexAuthenticationException(string message, Exception inner) : base(message, inner) { }

        /// <inheritdoc />
        public PaychexAuthenticationException() { }

        /// <inheritdoc />
        public PaychexAuthenticationException(string message) : base(message) { }
    }
}
