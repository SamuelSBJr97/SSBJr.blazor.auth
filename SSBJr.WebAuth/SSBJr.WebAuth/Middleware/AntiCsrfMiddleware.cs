namespace SSBJr.WebAuth.Middleware;

public class AntiCsrfMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AntiCsrfMiddleware> _logger;
    private const string CsrfTokenHeader = "X-CSRF-TOKEN";
    private const string CsrfTokenCookie = "__HostCSRFToken";

    public AntiCsrfMiddleware(RequestDelegate next, ILogger<AntiCsrfMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip CSRF validation for Blazor SignalR endpoints
        if (context.Request.Path.StartsWithSegments("/_blazor"))
        {
            await _next(context);
            return;
        }

        // Apenas validar em requisições que modificam dados
        if (context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase) ||
            context.Request.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase))
        {
            if (!ValidateCsrfToken(context))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                _logger.LogWarning($"CSRF token validation failed for {context.Request.Path}");
                await context.Response.WriteAsync("CSRF token validation failed");
                return;
            }
        }

        // Gerar novo token CSRF se não existir
        if (!context.Request.Cookies.ContainsKey(CsrfTokenCookie))
        {
            var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            context.Response.Cookies.Append(CsrfTokenCookie, token, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = false, // Necessário para JavaScript ler o token
                Secure = true,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                IsEssential = true
            });
        }

        await _next(context);
    }

    private bool ValidateCsrfToken(HttpContext context)
    {
        var tokenFromHeader = context.Request.Headers[CsrfTokenHeader].ToString();
        var tokenFromCookie = context.Request.Cookies[CsrfTokenCookie];

        if (string.IsNullOrEmpty(tokenFromHeader) || string.IsNullOrEmpty(tokenFromCookie))
        {
            return false;
        }

        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(tokenFromHeader),
            System.Text.Encoding.UTF8.GetBytes(tokenFromCookie));
    }
}
