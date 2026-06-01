using System.Diagnostics;

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
        if (_skipExtensions.Contains(Path.GetExtension(path))) { await _next(context); return; }

        var method      = context.Request.Method;
        var queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : "";
        var sw          = Stopwatch.StartNew();
        var user        = context.User?.Identity?.Name ?? "anónimo";
        var contentType = context.Request.ContentType ?? "";

        // ── REQUEST body (solo para POST / PUT / PATCH con contenido) ──
        string requestBody = "";
        if (context.Request.ContentLength > 0 && method is "POST" or "PUT" or "PATCH")
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            requestBody = Truncate(await reader.ReadToEndAsync(), 500);
            context.Request.Body.Position = 0;
        }

        // ── Log REQUEST ────────────────────────────────────────────────
        var reqExtra = new System.Text.StringBuilder();
        if (!string.IsNullOrEmpty(queryString))      reqExtra.Append($"\n         query  : {queryString}");
        if (!string.IsNullOrEmpty(requestBody))      reqExtra.Append($"\n         body   : {requestBody}");
        if (!string.IsNullOrEmpty(contentType))      reqExtra.Append($"\n         content: {contentType.Split(';')[0]}");

        _logger.LogInformation(
            "➡️  {EventType,-8} │ {Method,-6} │ {Path,-40} │ usuario: {User}{Extra}",
            "REQUEST", method, path, user, reqExtra.ToString());

        // ── Capturar RESPONSE ──────────────────────────────────────────
        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        Exception? caughtEx = null;
        try   { await _next(context); }
        catch (Exception ex) { caughtEx = ex; context.Response.StatusCode = 500; }
        finally
        {
            sw.Stop();
            var status      = context.Response.StatusCode;
            var ms          = sw.ElapsedMilliseconds;
            var respCT      = context.Response.ContentType ?? "";
            var isJson      = respCT.Contains("application/json", StringComparison.OrdinalIgnoreCase);
            var isHtml      = respCT.Contains("text/html", StringComparison.OrdinalIgnoreCase);

            buffer.Position = 0;
            var rawResponse = await new StreamReader(buffer).ReadToEndAsync();
            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            // Mostrar body de respuesta: siempre en errores, solo JSON en éxitos
            string responseBody = "";
            if (status >= 400 || caughtEx is not null)
                responseBody = Truncate(rawResponse, 600);
            else if (isJson)
                responseBody = Truncate(rawResponse, 400);

            var resExtra = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(responseBody) && !isHtml)
                resExtra.Append($"\n         body   : {responseBody}");
            if (caughtEx is not null)
                resExtra.Append($"\n         error  : {caughtEx.GetType().Name}: {caughtEx.Message}");

            // ── Log RESPONSE ───────────────────────────────────────────
            if (caughtEx is not null)
            {
                _logger.LogError(caughtEx,
                    "💥  {EventType,-8} │ {Method,-6} │ {Path,-40} │ {Status} │ {Ms,5}ms │ usuario: {User}{Extra}",
                    "ERROR", method, path, status, ms, user, resExtra.ToString());
            }
            else if (status >= 500)
            {
                _logger.LogError(
                    "🔴  {EventType,-8} │ {Method,-6} │ {Path,-40} │ {Status} │ {Ms,5}ms │ usuario: {User}{Extra}",
                    "RESPONSE", method, path, status, ms, user, resExtra.ToString());
            }
            else if (status >= 400)
            {
                _logger.LogWarning(
                    "🟡  {EventType,-8} │ {Method,-6} │ {Path,-40} │ {Status} │ {Ms,5}ms │ usuario: {User}{Extra}",
                    "RESPONSE", method, path, status, ms, user, resExtra.ToString());
            }
            else
            {
                _logger.LogInformation(
                    "✅  {EventType,-8} │ {Method,-6} │ {Path,-40} │ {Status} │ {Ms,5}ms │ usuario: {User}{Extra}",
                    "RESPONSE", method, path, status, ms, user, resExtra.ToString());
            }

            if (caughtEx is not null) throw caughtEx;
        }
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "…";
}
