using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Paychex.Api.Api.Interfaces;
using Paychex.Api.Models.Authentication;

namespace Paychex.Api
{
    public class PaychexWebCache : IPaychexTokenCache, IPaychexDataCache
    {
        private const string TokenCacheKey = "PaychexToken";
        private const string DataCachePrefix = "paychex:";
        private readonly ICacheProvider _cache;

        public PaychexWebCache(ICacheProvider cache)
        {
            _cache = cache;
        }

        public void Set<T>(string r, T value)
        {
            SaveItemToWebCache(DataCacheKey(r), value, DateTime.Now.AddMinutes(5));
        }

        public T Get<T>(string r) => IgnoreCacheReads ? default(T) : InternalGet<T>(r);

        public void Clear()
        {
            foreach (var item in
                from ce in _cache
                where ce.Key.ToString().StartsWith(DataCachePrefix)
                select ce)
            {
                _cache.Remove(item.Key.ToString());
            }
        }

        public bool IgnoreCacheReads { get; set; }

        public void Invalidate() => _cache.Remove(TokenCacheKey);

        public PaychexAuthToken Load() => InternalGet<PaychexAuthToken>(TokenCacheKey);

        public void Save(PaychexAuthToken token) => SaveItemToWebCache(
            TokenCacheKey,
            token,
            DateTime.Now.AddSeconds(token.expires_in)
        );

        private T InternalGet<T>(string r) => (T) _cache[DataCacheKey(r)];

        private void SaveItemToWebCache(string key, object token, DateTime expires)
        {
            _cache.Add(
                key,
                token,
                new CacheExpiration(expires)
            );
        }

        private static string DataCacheKey(string k) => $"{DataCachePrefix}{k}";
    }

    public struct CacheExpiration
    {
        public CacheExpiration(TimeSpan slidingExpiration)
        {
            SlidingExpiration = slidingExpiration;
            AbsoluteExpiration = null;
        }

        public CacheExpiration(DateTime absolute)
        {
            AbsoluteExpiration = absolute;
            SlidingExpiration = Cache.NoSlidingExpiration;
        }

        public TimeSpan SlidingExpiration {get;}
        public DateTime? AbsoluteExpiration {get;}
    }

    public interface ICacheProvider : IEnumerable<DictionaryEntry>
    {
        object this[string item] {get;set;}
        object Get(string item);
        object Add(string key, object item, CacheExpiration cacheExpiration);
        object Remove(string key);
    }

    internal class DictAdapter : IEnumerator<DictionaryEntry>
    {
        private readonly IDictionaryEnumerator _dictionaryEnumerator;

        public DictAdapter(IDictionaryEnumerator dictionaryEnumerator)
        {
            _dictionaryEnumerator = dictionaryEnumerator;
        }
        public void Dispose() { }

        public bool MoveNext() => _dictionaryEnumerator.MoveNext();

        public void Reset() => _dictionaryEnumerator.Reset();

        public DictionaryEntry Current => (DictionaryEntry)_dictionaryEnumerator.Current;

        object IEnumerator.Current => Current;
    }

    public class HttpWebCache : ICacheProvider
    {
        private readonly Cache _cache;

        public HttpWebCache(Cache cache)
        {
            _cache = cache;
        }

        public IEnumerator<DictionaryEntry> GetEnumerator()
        {
            var enumerator = _cache.GetEnumerator();
            return new DictAdapter(enumerator);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public object this[string key]
        {
            get => _cache[key];
            set => _cache[key] = value;
        }

        public object Get(string key) => _cache.Get(key);

        public object Add(string key, object item, CacheExpiration cacheExpiration) =>
            _cache.Add(
                key,
                item,
                null,
                cacheExpiration.AbsoluteExpiration ?? Cache.NoAbsoluteExpiration,
                cacheExpiration.SlidingExpiration,
                CacheItemPriority.Default,
                null
            );

        public object Remove(string key) => _cache.Remove(key);
    }

    public class MemoryCache : ICacheProvider
    {
        private readonly Dictionary<object, object> _mockCache = new Dictionary<object, object>();

        public IEnumerator<DictionaryEntry> GetEnumerator() =>
            new DictAdapter(_mockCache.GetEnumerator());

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public object this[string key]
        {
            get => _mockCache[key];
            set => _mockCache[key] = value;
        }

        public object Get(string key)
        {
            _mockCache.TryGetValue(key, out var value);
            return value;
        }

        public object Add(string key, object item, CacheExpiration cacheExpiration) =>
            this[key] = item;


        public object Remove(string key) => _mockCache.Remove(key);
    }
}
