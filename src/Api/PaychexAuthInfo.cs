using System;
using Paychex.Api.Api.Interfaces;

namespace Paychex.Api.Api
{
    /// <inheritdoc />
    public class PaychexAuthInfo : IPaychexConfiguration
    {
        /// <inheritdoc />
        public string UrlEndpoint { get; set; }
        /// <inheritdoc />
        public string ApiKey { get; set; }
        /// <inheritdoc />
        public string ClientSecret { get; set; }
        /// <inheritdoc />
        public TimeSpan Timeout { get; set; }
    }
}
