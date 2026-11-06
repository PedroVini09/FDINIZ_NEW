using System.Net;
using System.Text.RegularExpressions;
using FDINIZ_Lab.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;

namespace FDINIZ_Lab.Tests;

/// <summary>Sobe o site em memória, com relógio controlado e e-mail falso.</summary>
public sealed class SiteFactory : WebApplicationFactory<Program>
{
    public FakeTimeProvider Relogio { get; } = new(DateTimeOffset.UtcNow);
    public EmailFalso Email { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Relogio);
            services.RemoveAll<IEmailService>();
            services.AddSingleton<IEmailService>(Email);
        });
    }

    public HttpClient Cliente() => CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
}

public sealed class EmailFalso : IEmailService
{
    public List<MensagemEmail> Enviados { get; } = [];
    public bool Falhar { get; set; }

    public Task<bool> EnviarAsync(MensagemEmail mensagem, CancellationToken cancellationToken = default)
    {
        if (Falhar) return Task.FromResult(false);
        Enviados.Add(mensagem);
        return Task.FromResult(true);
    }
}

public static partial class Formulario
{
    /// <summary>Abre /contato e devolve o token anti-CSRF e o carimbo anti-spam do formulário.</summary>
    public static async Task<(string Token, string Carimbo)> AbrirContatoAsync(HttpClient cliente)
    {
        var html = await cliente.GetStringAsync("/contato");
        return (Valor(html, "__RequestVerificationToken"), Valor(html, "Carimbo"));
    }

    public static Dictionary<string, string> ContatoValido(string token, string carimbo) => new()
    {
        ["__RequestVerificationToken"] = token,
        ["Carimbo"] = carimbo,
        ["Nome"] = "Maria Souza",
        ["Email"] = "maria@example.com",
        ["Telefone"] = "(83) 99999-0000",
        ["Assunto"] = "Exames",
        ["Mensagem"] = "Gostaria de saber o preparo do hemograma.",
        ["AceitoPrivacidade"] = "true",
        ["Website"] = ""
    };

    private static string Valor(string html, string nome)
    {
        var m = Regex.Match(html, $"name=\"{Regex.Escape(nome)}\"[^>]*value=\"([^\"]*)\"");
        if (!m.Success) m = Regex.Match(html, $"value=\"([^\"]*)\"[^>]*name=\"{Regex.Escape(nome)}\"");
        return WebUtility.HtmlDecode(m.Groups[1].Value);
    }
}
