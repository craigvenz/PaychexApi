using System;

namespace Paychex.Api.Api.Interfaces
{
    /// <summary>
    ///     Configuration options for calling Paychex API.
    /// </summary>
    public interface IPaychexConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        string UrlEndpoint { get; }
        /// <summary>
        /// 
        /// </summary>
        string ApiKey { get; }
        /// <summary>
        /// 
        /// </summary>
        string ClientSecret { get; }
        /// <summary>
        /// 
        /// </summary>
        TimeSpan Timeout { get; }
    }
}
