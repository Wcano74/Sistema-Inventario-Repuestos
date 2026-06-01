using System.Diagnostics;
using System.Text;

namespace SistemaInventario.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    private static readonly HashSet<string> _skipExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".css", ".js", ".map", ".ico", ".png", ".jpg", ".jpeg",
        ".webp", ".gif", ".svg", ".woff", ".woff2", ".ttf", ".eot"
    };

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        var ext  = Path.GetExtension(path);
        if (_skipExtensions.Contains(ext)) { await _next(context); return; }

        var method = context.Request.Method;
        var sw     = Stopwatch.StartNew();
        var user   = context.User?.Identity?.Name ?? "anónimo";

        // ── Capturar body del REQUEST ──────────────────────────
        string requestBody = "";
        if (context.Request.ContentLength > 0 &&
            method is "POST" or "PUT" or "PATCH")
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            requestBody = Truncate(await reader.ReadToEndAsync(), 400);
            context.Request.Body.Position = 0;
        }

        // ── Log REQUEST ────────────────────────────────────────
        if (string.IsNullOrEmpty(requestBody))
        {
            _logger.LogInformation(
                "➡️  {EventType,-8} │ {Method,-6} │ {Path,-45} │ usuario: {User}",
                "REQUEST", method, path, user);
        }
        else
        {
            _logger.LogInformation(
                "➡️  {EventType,-8} │ {Method,-6} │ {Path,-45} │ usuario: {User}\n            body: {Body}",
                "REQUEST", method, path, user, requestBody);
        }

        // ── Capturar RESPONSE ──────────────────────────────────
        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        Exception? caughtEx = null;
        try   { await _next(context); }
        catch (Exception ex) { caughtEx = ex; context.Response.StatusCode = 500; }
        finally
        {
            sw.Stop();
            var status = context.Response.StatusCode;
            var ms     = sw.ElapsedMilliseconds;

            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            // ── Log RESPONSE ───────────────────────────────────
            if (caughtEx is not null)
            {
                _logger.LogError(caughtEx,
                    "💥  {EventType,-8} │ {Method,-6} │ {Path,-45} │ {Status} │ {Ms}ms │ usuario: {User}\n            error: {Error}",
                    "ERROR", method, path, status, ms, user, caughtEx.Message);
            }
            else if (status >= 500)
            {
                buffer.Position = 0;
                var errBody = Truncate(await new StreamReader(buffer).ReadToEndAsync(), 300);
                _logger.LogError(
                    "🔴  {EventType,-8} │ {Method,-6} │ {Path,-45} │ {Status} │ {Ms}ms │ usuario: {User}\n            response: {Body}",
                    "RESPONSE", method, path, status, ms, user, errBody);
            }
            else if (status >= 400)
            {
                _logger.LogWarning(
                    "🟡  {EventType,-8} │ {Method,-6} │ {Path,-45} │ {Status} │ {Ms}ms │ usuario: {User}",
                    "RESPONSE", method, path, status, ms, user);
            }
            else
            {
                _logger.LogInformation(
                    "✅  {EventType,-8} │ {Method,-6} │ {Path,-45} │ {Status} │ {Ms}ms │ usuario: {User}",
                    "RESPONSE", method, path, status, ms, user);
            }

            if (caughtEx is not null) throw caughtEx;
        }
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
