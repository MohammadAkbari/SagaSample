using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using StackExchange.Redis;

namespace Redis.ResponseCache
{
    internal class RedisResponseCache : IResponseCache
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisResponseCache(string redisConnectionMultiplexerConfiguration)
        {
            if (string.IsNullOrWhiteSpace(redisConnectionMultiplexerConfiguration))
            {
                throw new ArgumentNullException(nameof(redisConnectionMultiplexerConfiguration));
            }

            _redis = ConnectionMultiplexer.Connect(redisConnectionMultiplexerConfiguration);
        } 

        public IResponseCacheEntry Get(string key)
        {
            return GetAsync(key).GetAwaiter().GetResult();
        }

        public async Task<IResponseCacheEntry> GetAsync(string key)
        {
            IResponseCacheEntry responseCacheEntry = null;

            var redisDatabase = _redis.GetDatabase();

            var hashEntries = await redisDatabase.HashGetAllAsync(key);

            var entry = hashEntries.FirstOrDefault(e => e.Name == "Type");

            if (!entry.Value.HasValue)
            {
                return null;
            }

            string type = entry.Value;

            if (type == nameof(CachedResponse))
            {
                HashEntry[] headersHashEntries = await redisDatabase.HashGetAllAsync(key + "_Headers");

                if ((headersHashEntries != null) && headersHashEntries.Length > 0 && (hashEntries.Length > 0))
                {
                    CachedResponse cachedResponse = CachedResponseFromHashEntryArray(hashEntries);
                    cachedResponse.Headers = HeaderDictionaryFromHashEntryArray(headersHashEntries);

                    responseCacheEntry = cachedResponse;
                }
            }
            else if (type == nameof(CachedVaryByRules))
            {
            }

            return responseCacheEntry;
        }

        public void Set(string key, IResponseCacheEntry entry, TimeSpan validFor)
        {
            SetAsync(key, entry, validFor).GetAwaiter().GetResult();
        }

        public async Task SetAsync(string key, IResponseCacheEntry entry, TimeSpan validFor)
        {
            if (entry is CachedResponse cachedResponse)
            {
                string headersKey = key + "_Headers";

                IDatabase redisDatabase = _redis.GetDatabase();

                await redisDatabase.HashSetAsync(key, CachedResponseToHashEntryArray(cachedResponse));
                await redisDatabase.HashSetAsync(headersKey, HeaderDictionaryToHashEntryArray(cachedResponse.Headers));

                await redisDatabase.KeyExpireAsync(headersKey, validFor);
                await redisDatabase.KeyExpireAsync(key, validFor);
            }
            else if (entry is CachedVaryByRules cachedVaryByRules)
            {
            }
        }

        private CachedResponse CachedResponseFromHashEntryArray(HashEntry[] hashEntries)
        {
            var cachedResponse = new CachedResponse();

            foreach (var hashEntry in hashEntries)
            {
                switch (hashEntry.Name)
                {
                    case nameof(cachedResponse.Created):
                        cachedResponse.Created = DateTimeOffset.FromUnixTimeMilliseconds((long)hashEntry.Value);
                        break;
                    case nameof(cachedResponse.StatusCode):
                        cachedResponse.StatusCode = (int)hashEntry.Value;
                        break;
                    case nameof(cachedResponse.Body):
                        cachedResponse.Body = new MemoryStream(hashEntry.Value);
                        break;
                }
            }

            return cachedResponse;
        }

        private static IHeaderDictionary HeaderDictionaryFromHashEntryArray(IEnumerable<HashEntry> headersHashEntries)
        {
            IHeaderDictionary headerDictionary = new HeaderDictionary();

            foreach (var headersHashEntry in headersHashEntries)
            {
                headerDictionary.Add(headersHashEntry.Name, (string)headersHashEntry.Value);
            }

            return headerDictionary;
        }

        private HashEntry[] CachedResponseToHashEntryArray(CachedResponse cachedResponse)
        {
            var bodyStream = new MemoryStream();
            cachedResponse.Body.CopyTo(bodyStream);

            return new[]
            {
                new HashEntry("Type", nameof(CachedResponse)),
                new HashEntry(nameof(cachedResponse.Created), cachedResponse.Created.ToUnixTimeMilliseconds()),
                new HashEntry(nameof(cachedResponse.StatusCode), cachedResponse.StatusCode),
                new HashEntry(nameof(cachedResponse.Body), bodyStream.ToArray())
            };
        }

        private static HashEntry[] HeaderDictionaryToHashEntryArray(IHeaderDictionary headerDictionary)
        {
            var headersHashEntries = new HashEntry[headerDictionary.Count];

            int headersHashEntriesIndex = 0;
            foreach (var header in headerDictionary)
            {
                headersHashEntries[headersHashEntriesIndex++] = new HashEntry(header.Key, (string)header.Value);
            }

            return headersHashEntries;
        }
    }
}