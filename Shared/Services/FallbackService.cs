// using Polly;
// using Polly.CircuitBreaker;
// using Polly.Retry;
// using Polly.Timeout;


// namespace Shared.Services;



// public interface IFallbackService
// {
//     Task<T?> GetCachedOrDefaultAsync<T>(string key, Func<Task<T>> fetchFunc, T? defaultValue = default);
// }



// public class FallbackService: IFallbackService
// {
//     private readonly IRedisCacheService _cache;
//     private readonly ILogger<FallbackService> _logger;


//     public FallbackService( 
//         IRedisCacheService cache,
//         ILogger<FallbackService> logger
//     )
//     {
//         _cache = cache;
//         _logger = logger;
//     }


//     public async Task<T?> GetCachedOrDefaultAsync<T>(string key, Func<Task<T>> fetchFunc, T? defaultValue = default)
//     {
//         try
//         {
//             // Try to fetch fresh data
//             var result = await fetchFunc();

//             // Cache successful result
//             await _cache.SetAsync(key, result, TimeSpan.FromMinutes(10));

//             return result;

//         } catch (Exception ex)
//         {
//             _logger.LogWarning(ex, "Failed to fetch data, attempting cache fallback for key: {key}", key);

//             // Try cache as fallback
//             var cached = await _cache.GetAsync<T>(key);

//             if (cached != null)
//             {
//                 _logger.LogInformation("Returning cached data for key: {key}", key);
//                 return cached;
//             }


//             // Return default value as last resort
//             _logger.LogWarning("No cached data available, returning default value for key: {key}", key);
            
//             return defaultValue;
//         }
//     }
// }