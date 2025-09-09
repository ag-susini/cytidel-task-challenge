using System.Diagnostics;
using Serilog.Context;

namespace Tasker.Api.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString("N");
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            var stopwatch = Stopwatch.StartNew();
            var method = context.Request.Method;
            var path = context.Request.Path.Value;
            var queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
            
            try
            {
                logger.LogInformation(
                    "HTTP {Method} {Path}{QueryString} started",
                    method,
                    path,
                    queryString);

                await next(context);

                stopwatch.Stop();
                
                logger.LogInformation(
                    "HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMs}ms",
                    method,
                    path,
                    queryString,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                logger.LogError(ex,
                    "HTTP {Method} {Path}{QueryString} failed after {ElapsedMs}ms",
                    method,
                    path,
                    queryString,
                    stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }
    }
}