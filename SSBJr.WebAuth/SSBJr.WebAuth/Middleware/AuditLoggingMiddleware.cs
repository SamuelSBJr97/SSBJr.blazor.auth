namespace SSBJr.WebAuth.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var ipAddress = GetClientIpAddress(context);
        var userAgent = context.Request.Headers.UserAgent.ToString();

        _logger.LogInformation(
            $"Request: {context.Request.Method} {context.Request.Path} from {ipAddress}");

        await _next(context);

        var duration = DateTime.UtcNow - startTime;
        _logger.LogInformation(
            $"Response: {context.Response.StatusCode} - Duration: {duration.TotalMilliseconds}ms");
    }

    private string GetClientIpAddress(HttpContext context)
    {
        if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return context.Request.Headers["X-Forwarded-For"].ToString().Split(",")[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}
