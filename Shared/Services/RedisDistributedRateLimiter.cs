// using System.Threading.RateLimiting;
// using Microsoft.AspNetCore.RateLimiting;


// namespace Shared.Services;

// public class RateLimitInfo
// {
//     public int CurrentCount { get; set; }
//     public bool IsLimited { get; set; }
//     public DateTime ResetTime { get; set; }
// }

// [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
// public class RateLimitAttribute: Attribute
// {
//     public int MaxRequests { get; set; }
//     public TimeSpan Window { get; set; }

//     public RateLimitAttribute(
//         int maxRequests, int windowSeconds
//     )
//     {
//         MaxRequests = maxRequests;
//         Window = TimeSpan.FromSeconds(windowSeconds);
//     }
// }

// public interface IDistributedRateLimiter
// {
//     Task<bool> IsAllowedAsync(string key, int maxRequests, TimeSpan window);
//     Task<RateLimitInfo> GetRateLimitInfoAsync(string key);
// }


// public class RedisDistributedRateLimiter: IDistributedRateLimiter
// {
//     private readonly IRedisCacheService _cache;

//     public RedisDistributedRateLimiter(IRedisCacheService cache)
//     {
//         _cache = cache;
//     }

//     public async Task<bool> IsAllowedAsync(
//         string key, int maxRequests, TimeSpan window
//     )
//     {
//         var rateLimitKey = $"rateLimit:{key}";
//         var currentCount = await _cache.IncrementAsync(rateLimitKey);

//         if(currentCount == 1)
//         {
//             await _cache.SetAsync(rateLimitKey, currentCount, window);
//         }

//         return currentCount <= maxRequests;
//     }


//     public async Task<RateLimitInfo> GetRateLimitInfoAsync(
//         string key
//     )
//     {
//         var rateLimitKey = $"rateLimit:{key}";
//         var count = await _cache.GetAsync<long>(rateLimitKey) ?? 0L;

//         return new RateLimitInfo
//         {
//             CurrentCount = (int)count,
//             IsLimited = count > 100,
//             ResetTime = DateTime.UtcNow.AddMinutes(1)
//         };
//     }
// }



// public class CustomRateLimitingMiddleware
// {
//     private readonly RequestDelegate _next;
//     private readonly IDistributedRateLimiter _rateLimiter;
//     private readonly ILogger<CustomRateLimitingMiddleware> _logger;

//     public CustomRateLimitingMiddleware(
//         RequestDelegate next,
//         IDistributedRateLimiter rateLimiter,
//         ILogger<CustomRateLimitingMiddleware> logger
//     )
//     {
//         _next = next;
//         _rateLimiter = rateLimiter;
//         _logger = logger;
//     }

//     public async Task InvokeAsync(HttpContext context)
//     {
//         var endpoint = context.GetEndpoint();
//         var rateLimitAtribute = endpoint?.Metadata.GetMetadata<RateLimitAttribute>();

//         if (rateLimitAtribute != null)
//         {
//             var clientId = context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

//             var key = $"{clientId}:{context.Request.Path}";

//             var isAllowed = await _rateLimiter.IsAllowedAsync(
//                 key,
//                 rateLimitAtribute.MaxRequests,
//                 rateLimitAtribute.Window
//             );

//             if (!isAllowed)
//             {
//                 _logger.LogWarning(
//                     "Rate limit exceeded for {ClientId} on {Path}",
//                     clientId,
//                     context.Request.Path
//                 );

//                 context.Response.StatusCode = StatusCodes.Status429TooManyRequests;

//                 await context.Response.WriteAsJsonAsync(new
//                 {
//                     error = "Rate Limit Exceeded",
//                     message = $"Maximum {rateLimitAtribute.MaxRequests} requests per {rateLimitAtribute.Window}" 
//                 });

//                 return;
//             }


//             // Add Rate Limit in Headers
//             var info = await _rateLimiter.GetRateLimitInfoAsync(key);
//             context.Response.Headers["X-RateLimit-Limit"] = rateLimitAtribute.MaxRequests.ToString();
//             context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, rateLimitAtribute.MaxRequests - info.CurrentCount).ToString();
//             context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(info.ResetTime).ToUnixTimeSeconds().ToString();
//         }


//         await _next(context);
//     }
// }