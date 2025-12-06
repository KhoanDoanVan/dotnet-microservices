using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;


namespace Shared.Services;

public class RateLimitInfo
{
    public int CurrentCount { get; set; }
    public bool IsLimited { get; set; }
    public DateTime ResetTime { get; set; }
}


public interface IDistributedRateLimiter
{
    Task<bool> IsAllowedAsync(string key, int maxRequests, TimeSpan window);
    Task<RateLimitInfo> GetRateLimitInfoAsync(string key);
}


public class RedisDistributedRateLimiter: IDistributedRateLimiter
{
    private readonly IRedisCacheService _cache;

    public RedisDistributedRateLimiter(IRedisCacheService cache)
    {
        _cache = cache;
    }

    public async Task<bool> IsAllowedAsync(
        string key, int maxRequests, TimeSpan window
    )
    {
        var rateLimitKey = $"rateLimit:{key}";
        var currentCount = await _cache.IncrementAsync(rateLimitKey);

        if(currentCount == 1)
        {
            await _cache.SetAsync(rateLimitKey, currentCount, window);
        }

        return currentCount <= maxRequests;
    }


    public async Task<RateLimitInfo> GetRateLimitInfoAsync(
        string key
    )
    {
        var rateLimitKey = $"rateLimit:{key}";
        var count = await _cache.GetAsync<long>(rateLimitKey) ?? 0L;

        return new RateLimitInfo
        {
            CurrentCount = (int)count,
            IsLimited = count > 100,
            ResetTime = DateTime.UtcNow.AddMinutes(1)
        };
    }
}