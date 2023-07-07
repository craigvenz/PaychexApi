using Paychex.Api.Models.Authentication;

namespace Paychex.Api.Api.Interfaces
{
    /// <summary>
    ///     Interface to wrap possible caching methods for the token returned by paychex.
    ///     Maintains the token's time-to-live value so we don't keep unnecessarily calling authorize for each call.
    /// </summary>
    public interface IPaychexTokenCache
    {
        /// <summary>
        /// Get the cached token
        /// </summary>
        /// <returns>Cached token instance, or null if expired or not found.</returns>
        PaychexAuthToken Load();
        /// <summary>
        /// Set the cached token.
        /// </summary>
        /// <param name="token">Value to cache.</param>
        void Save(PaychexAuthToken token);
        /// <summary>
        /// Manually clear the cached token.
        /// </summary>
        void Invalidate();
    }
}
