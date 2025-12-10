
// namespace Shared.Middleware;



// public class DistributedTracingMiddleware
// {
    
//     private readonly RequestDelegate _next;
//     private readonly ILogger<DistributedTracingMiddleware> _logger;

//     public DistributedTracingMiddleware(
//         RequestDelegate next,
//         ILogger<DistributedTracingMiddleware> logger
//     )
//     {
//         _next = next;
//         _logger = logger;
//     }


//     public async Task InvokeAsync(HttpContext context)
//     {
//         var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
//         var requestId = Guid.NewGuid().ToString();


//         // Add Trace Headers
//         context.Response.Headers["X-Trace-Id"] = traceId;
//         context.Response.Headers["X-Request-Id"] = requestId;

//         var stopwatch = Stopwatch.Startnew();

//         try
//         {

//             _logger.LogInformation(
//                 "Request failed: {Method} {Path} | TraceId: {TraceId} | RequestId: {RequestId}",
//                 context.Request.Method,
//                 context.Request.Path,
//                 traceId,
//                 requestId
//             );

//             await _next(context);

//             stopwatch.Stop();

//             _logger.LogInformation(
//                 "Request completed: {Method} {Path} | StatusCode: {StatusCode} | Duration: {Duration}ms | TraceId: {TraceId}",
//                 context.Request.Method,
//                 context.Request.Path,
//                 context.Response.StatusCode,
//                 stopwatch.ElapsedMilliseconds,
//                 traceId
//             );

            
//         } catch (Exception ex)
//         {
//             stopwatch.Stop();

//             _logger.LogError(
//                 ex,
//                 "Request failed: {Method} {Path} | Duration: {Duration}ms | TraceId: {TraceId} | Error: {Error}",
//                 context.Request.Method,
//                 context.Request.Path,
//                 stopwatch.ElapsedMilliseconds,
//                 traceId,
//                 ex.Message
//             );


//             throw;
//         }
//     }

// }