using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.RateLimiting;
using FDINIZ_Lab.Infrastructure;
using FDINIZ_Lab.Models;
using FDINIZ_Lab.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;

// Em desenvolvimento, lê variáveis de um arquivo .env local (que nunca vai para o Git).
if (File.Exists(".env"))
{
    DotNetEnv.Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

// Não revela qual servidor web está em uso.
builder.WebHost.ConfigureKestrel(o =>
{
    o.AddServerHeader = false;
    o.Limits.MaxRequestBodySize = 1 * 1024 * 1024; // 1 MB é suficiente para os formulários atuais.
});

builder.Services.AddControllersWithViews(o =>
{
    // Todo POST exige o token anti-CSRF, mesmo que alguém esqueça o atributo em uma action.
    o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

// Mantém acentos como texto normal no HTML. Os caracteres perigosos (< > & " ') continuam escapados.
builder.Services.Configure<Microsoft.Extensions.WebEncoders.WebEncoderOptions>(o =>
    o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

builder.Services.AddAntiforgery(o =>
{
    o.Cookie.Name = "fdiniz.af";
    o.Cookie.HttpOnly = true;
    // Em produção o cookie só trafega por HTTPS. Em desenvolvimento local (http) segue a requisição.
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.Configure<SiteSettings>(builder.Configuration.GetSection(SiteSettings.Secao));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.Secao));

var dados = Path.Combine(builder.Environment.ContentRootPath, "Data");
builder.Services.AddSingleton<IExameService>(_ =>
    JsonExameService.CarregarDeArquivo(Path.Combine(dados, "exames.json")));
builder.Services.AddSingleton<IUnidadeService>(_ =>
    JsonUnidadeService.CarregarDeArquivos(Path.Combine(dados, "unidades.json"), Path.Combine(dados, "convenios.json")));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDataProtection();
builder.Services.AddSingleton<IAntiSpamService, AntiSpamService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

// Limites de requisições por IP: protege contra envio em massa e abuso do site.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            IpDoCliente(ctx),
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1) }));

    o.AddPolicy(PoliticasLimite.Formularios, ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            IpDoCliente(ctx),
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 3, Window = TimeSpan.FromMinutes(10) }));

    o.OnRejected = async (ctx, token) =>
    {
        ctx.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await ctx.HttpContext.Response.WriteAsync(
            "Muitas tentativas em pouco tempo. Aguarde alguns minutos e tente novamente.", token);
    };
});

builder.Services.AddHsts(o =>
{
    o.MaxAge = TimeSpan.FromDays(365);
    o.IncludeSubDomains = true;
});

var app = builder.Build();

// Atrás de um proxy reverso (Nginx, IIS, nuvem), usa o IP real do visitante no limite de requisições.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    // Páginas de erro amigáveis, sem pilha de chamadas nem detalhes internos.
    app.UseExceptionHandler("/erro");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/erro/{0}");
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static string IpDoCliente(HttpContext ctx) => ctx.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

public static class PoliticasLimite
{
    public const string Formularios = "formularios";
}

public partial class Program;
