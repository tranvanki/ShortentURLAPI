using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace ShortenURLService.Services
{
    //This defines a contract (interface) for Redis
    public interface IRedisCacheService
    {
        Task CacheUrlAsync(string ShortenedUrl, string OriginalUrl);
        Task<string> GetCachedUrlAsync(string ShortenedUrl);
    }

    public class RedisCacheService : IRedisCacheService
    {
        //Constructor and Redis Connection
        private readonly IDatabase _cache;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _cache = redis.GetDatabase();
        }

        //Cache a Shortened URL
        public async Task CacheUrlAsync(string ShortenedUrl, string OriginalUrl)
        {
            await _cache.StringSetAsync(ShortenedUrl, OriginalUrl, TimeSpan.FromHours(1)); // Cache for 1 hour
        }

        //Retrieve Cached URL
        public async Task<string> GetCachedUrlAsync(string ShortenedUrl)
        {
            return await _cache.StringGetAsync(ShortenedUrl);
        }
    }
}