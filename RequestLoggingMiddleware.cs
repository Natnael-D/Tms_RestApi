using System.Diagnostics;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];//create a unique id
           //logs the start of the request
        _logger.LogInformation(
            "Request started {Method} {Path} | CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId
        );
          // Add correlation ID to response headers BEFORE calling next
        context.Response.Headers["X-Correlation-Id"] = correlationId;

        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation(
            "Request completed {Method} {Path} | Status: {StatusCode} | Elapsed: {ElapsedMs}ms | CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId
        );
    }
}