namespace FDINIZ_Lab.Infrastructure;

/// <summary>
/// Cabeçalhos de segurança enviados em toda resposta.
/// A Content-Security-Policy só permite scripts, estilos e fontes do próprio site, o que bloqueia
/// scripts injetados (XSS) mesmo que algum texto escape sem querer.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment ambiente)
{
    public const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self'; " +
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "connect-src 'self'; " +
        "media-src 'self'; " +
        "object-src 'none'; " +
        "frame-src 'none'; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'";

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            // Em produção, pede ao navegador que troque qualquer recurso http por https.
            headers.ContentSecurityPolicy = ambiente.IsDevelopment()
                ? ContentSecurityPolicy
                : ContentSecurityPolicy + "; upgrade-insecure-requests";
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";
            headers["Cross-Origin-Opener-Policy"] = "same-origin";
            headers.Remove("Server");
            headers.Remove("X-Powered-By");
            return Task.CompletedTask;
        });

        return next(context);
    }
}
