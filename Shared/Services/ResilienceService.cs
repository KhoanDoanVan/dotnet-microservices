using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;


namespace Shared.Services;


public interface IResilienceService
{
    AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy();
    AsyncCircuitBreakerPolicy<HttpResponseMessage> GetCircuitBreakerPolicy();
    AsyncTimeoutPolicy<HttpResponseMessage> GetTimeoutPolicy();
    IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();
}


public class ResilienceService: IResilienceService
{
    private readonly ILogger<ResilienceService> _logger;

    public ResilienceService(ILogger<ResilienceService> logger)
    {
        _logger = logger;
    }


    // Retry Policy: Retry 3 times with exponential backoff
    public AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Retry {RetryCount} after {Delay}s due to {Reason}",
                        retryCount,
                        timespan.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()
                    );
                }
            );
    }


    // Circuit Breaker: Open circuit after 5 consecutive failures
    public AsyncCircuitBreakerPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    _logger.LogError(
                        "Circuit breaker opened for {Duration}s due to {Reason}",
                        duration.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()
                    );
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset - service is healthy again");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("Circuit breaker half-open - testing service");
                }
            );
    }


    // Timeout Policy: 10 seconds timeout
    public AsyncTimeoutPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(10),
            onTimeoutAsync: (context, timespan, task) =>
            {
                _logger.LogError("Request timed out after {Timeout}s", timespan.TotalSeconds);
                return Task.CompletedTask;      
            }
        );
    }


    // Combined Policy: Timeout -> Retry -> Circuit Breaker
    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        return Policy.WrapAsync(
            GetCircuitBreakerPolicy(),
            GetRetryPolicy(),
            GetTimeoutPolicy()
        );
    }
}