using Microsoft.Extensions.Options;

namespace Redis.ResponseCache
{
    public class RedisResponseCachingOptions : IOptions<RedisResponseCachingOptions>
    {
        public string RedisConnectionMultiplexerConfiguration { get; set; }

        RedisResponseCachingOptions IOptions<RedisResponseCachingOptions>.Value => this;
    }
}
