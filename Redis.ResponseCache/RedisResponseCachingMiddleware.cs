using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Redis.ResponseCache
{
    public class RedisResponseCachingMiddleware : ResponseCachingMiddleware
    {
        private RedisResponseCache Cache
        {
            set
            {
                var cacheFieldInfo = typeof(ResponseCachingMiddleware)
                    .GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance);

                cacheFieldInfo.SetValue(this, value);
            }
        }

        public RedisResponseCachingMiddleware(RequestDelegate next, 
            IOptions<ResponseCachingOptions> options,
            ILoggerFactory loggerFactory,
            IResponseCachingPolicyProvider policyProvider,
            IResponseCachingKeyProvider keyProvider,
            IOptions<RedisResponseCachingOptions> redisOptions)
            : base(next, options, loggerFactory, policyProvider, keyProvider)
        {
            Cache = new RedisResponseCache(redisOptions.Value.RedisConnectionMultiplexerConfiguration);
        }
    }
}