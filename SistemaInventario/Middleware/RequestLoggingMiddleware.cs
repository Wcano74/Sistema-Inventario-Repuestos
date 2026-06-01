using System.Diagnostics;

namespace SistemaInventario.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    private static readonly HashSet<string> _skipExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".css", ".js", ".map", ".ico", ".png", ".jpg", ".jpeg", ".webp",
        ".gif", ".svg", ".woff", ".woff2", ".ttf", ".eot"
    };

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Ignorar archivos estáticos
        var ext = Path.GetExtension(path);
        if (_skipExtensions.Contains(ext))
        {
            await _next(context);
            return;
        }

        var method  = context.Request.Method;
        var sw      = Stopwatch.StartNew();
        var user    = context.User?.Identity?.Name ?? "anónimo";

        // Capturar body del request (solo POST/PUT/PATCH)
        string requestBody = "";
        if (context.Request.ContentLength > 0 &&
            (method == "POST" || method == "PUT" || method == "PATCH"))
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        _logger.LogInformation(
            "➡️  {Method} {Path} | Usuario: {User}{Body}",
            method, path, user,
            string.IsNullOrEmpty(requestBody) ? "" : $" | Body: {Truncate(requestBody, 300)}");

        // Capturar el response
        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        Exception? caughtEx = null;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            caughtEx = ex;
            context.Response.StatusCode = 500;
        }
        finally
        {
            sw.Stop();
            var status = context.Response.StatusCode;
            var ms     = sw.ElapsedMilliseconds;

            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            if (caughtEx is not null)
            {
                _logger.LogError(caughtEx,
                    "💥 {Method} {Path} → {Status} ({Ms}ms) | {Error}",
                    method, path, status, ms, caughtEx.Message);
            }
            else if (status >= 500)
            {
                buffer.Position = 0;
                var body = await new StreamReader(buffer).ReadToEndAsync();
                _logger.LogError(
                    "🔴 {Method} {Path} → {Status} ({Ms}ms) | Usuario: {User} | Response: {Body}",
                    method, path, status, ms, user, Truncate(body, 500));
            }
            else if (status >= 400)
            {
                _logger.LogWarning(
                    "🟡 {Method} {Path} → {Status} ({Ms}ms) | Usuario: {User}",
                    method, path, status, ms, user);
            }
            else
            {
                _logger.LogInformation(
                    "✅ {Method} {Path} → {Status} ({Ms}ms) | Usuario: {User}",
                    method, path, status, ms, user);
            }

            if (caughtEx is not null) throw caughtEx;
        }
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
