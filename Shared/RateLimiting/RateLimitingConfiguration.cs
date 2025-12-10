// using System.Threading.RateLimiting;
// using Microsoft.AspNetCore.RateLimiting;


// namespace Shared.RateLimiting;

// public static class RateLimitingConfiguration
// {
//     public static IServiceCollection AddAdvancedRateLimiting(
//         this IServiceCollection services
//     )
//     {
//         services.AddRateLimiter(options =>
//         {
//            // Global rate limit: 100 request per minute
//             options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
//             {
//                 return RateLimitPartition.GetFixedWindowLimiter(
//                     partitionKey: context.User?.Identity?.Name ?? context.Request.Headers.Host.ToString(),
//                     factory: partition => new FixedWindowRateLimiterOptions
//                     {
//                         AutoReplenishment = true,
//                         PermitLimit = 100,
//                         Window = TimeSpan.FromMinutes(1)
//                     }
//                 );
//             });

//             // Sliding window limiter for API endpoints
//             options.AddPolicy("api", context =>
//             {
//                 return RateLimitPartition.GetSlidingWindowLimiter(
//                     partitionKey: context.User?.Identity?.Name ?? GetClientIp(context),
//                     factory: partition => new SlidingWindowRateLimiterOptions
//                     {
//                         AutoReplenishment = true,
//                         PermitLimit = 200,
//                         Window = TimeSpan.FromMinutes(1),
//                         SegmentsPerWindow = 4,
//                     });
//             });

//             // Token bucket limiter for burst traffic
//             options.AddPolicy("burst", context =>
//             {
//                return RateLimitPartition.GetTokenBucketLimiter(
//                     partitionKey: GetClientIp(context),
//                     factory: partition => new TokenBucketRateLimiterOptions
//                     {
//                         TokenLimit = 50,
//                         ReplenishmentPeriod = TimeSpan.FromSeconds(10),
//                         TokensPerPeriod = 10,
//                         AutoReplenishment = true
//                     }
//                );
//             });

//             // Concurrency limiter for resource-intensive endpoints
//             options.AddPolicy("concurrent", context =>
//             {
//                 return RateLimitPartition.GetConcurrencyLimiter(
//                     partitionKey: GetClientIp(context),
//                     factory: partition => new ConcurrencyLimiterOptions
//                     {
//                         PermitLimit = 10,
//                         QueueLimit = 5,
//                         QueueProcessingOrder = QueueProcessingOrder.OldestFirst
//                     }
//                 ); 
//             });

//             // Custom rate limiter for authenticated users vs guests
//             options.AddPolicy("authenticated", context =>
//             {
//                 var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;
//                 var limit = isAuthenticated ? 500 : 100;

//                 return RateLimitPartition.GetFixedWindowLimiter(
//                     partitionKey: context.User?.Identity?.Name ?? GetClientIp(context),
//                     factory: partition => new FixedWindowRateLimiterOptions
//                     {
//                         AutoReplenishment = true,
//                         PermitLimit = limit,
//                         Window = TimeSpan.FromMinutes(1)
//                     });
//             });

//             // Rate Limit rejection handling
//             options.OnRejected = async (context, cancellationToken) =>
//             {
//                 context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

//                 if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
//                 {
//                     context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
//                 }

//                 await context.HttpContext.Response.WriteAsJsonAsync(
//                     new
//                     {
//                         error = "Too Many Requests",
//                         message = "Rate limit exceeded. PLease try again later.",
//                         retryAfter = retryAfter.TotalSeconds
//                     },
//                     cancellationToken
//                 );
//             };
//         });

//         return services;

//     }

//     private static string GetClientIp(HttpContext context)
//     {
//         return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
//     }
// }