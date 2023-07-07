namespace Paychex.Api.Api.Interfaces
{
    /// <summary>
    /// Naive interface for caching api calls.
    /// </summary>
    public interface IPaychexDataCache
    {
        /// <summary>
        /// Write a response to the cache
        /// </summary>
        /// <param name="r"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        void Set<T>(string r, T value);

        /// <summary>
        /// Check for and retrieve a cached response
        /// </summary>
        /// <param name="r"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Get<T>(string r);

        /// <summary>
        /// Clear cache.
        /// </summary>
        void Clear();

        /// <summary>
        /// Set this to false if you want to bypass cache reads and force the API to hit the server. Still saves responses even if this is set.
        /// </summary>
        bool IgnoreCacheReads {get;set;}
    }
}
