using System;

namespace Paychex.Api.Models.Authentication
{
    /// <summary>
    /// Token received upon successful authentication with the Api server.
    /// </summary>
    public class PaychexAuthToken
    {
        /// <summary>
        /// Shortcut for determining if the token is valid.
        /// </summary>
        [JsonIgnore]
        public bool isValid => TimeAuthenticated != default(DateTime)
                               && DateTime.Now < TimeAuthenticated.AddSeconds(expires_in);

        /// <summary>
        /// The token which will be used for making future calls.
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// This will always return "Bearer" based on OAuth configuration.
        /// </summary>
        public string token_type { get; set; }
        /// <summary>
        /// Number of seconds remaining before the token expires.
        /// </summary>
        public int expires_in { get; set; }
        /// <summary>
        /// This will always return "oob" (out of band) based on OAuth configuration.
        /// </summary>
        public string scope { get; set; }
        /// <summary>
        /// Time we authenticated - used by isValid.
        /// </summary>
        public DateTime TimeAuthenticated { get; set; }
    }
}
