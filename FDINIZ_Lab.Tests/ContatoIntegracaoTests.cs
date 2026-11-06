using System.Net;

namespace FDINIZ_Lab.Tests;

/// <summary>Cada teste usa um site novo, para o limite de envios de um não afetar o outro.</summary>
public class ContatoIntegracaoTests
{
    [Fact]
    public async Task ContatoValido_EnviaEmailComConteudoEscapado()
    {
        using var site = new SiteFactory();
        var cliente = site.Cliente();
        var (token, carimbo) = await Formulario.AbrirContatoAsync(cliente);
        site.Relogio.Advance(TimeSpan.FromSeconds(15));

        var dados = Formulario.ContatoValido(token, carimbo);
        dados["Mensagem"] = "Olá! <script>alert('xss')</script>";
        var resposta = await cliente.PostAsync("/contato", new FormUrlEncodedContent(dados));

        Assert.Equal(HttpStatusCode.Redirect, resposta.StatusCode);
        Assert.EndsWith("/contato/enviado", resposta.Headers.Location!.OriginalString);
        var email = Assert.Single(site.Email.Enviados);
        Assert.DoesNotContain("<script>", email.CorpoHtml);
        Assert.Equal("maria@example.com", email.ResponderPara);
    }

    [Fact]
    public async Task CampoArmadilhaPreenchido_FingeSucessoMasNaoEnvia()
    {
        using var site = new SiteFactory();
        var cliente = site.Cliente();
        var (token, carimbo) = await Formulario.AbrirContatoAsync(cliente);
        site.Relogio.Advance(TimeSpan.FromSeconds(15));

        var dados = Formulario.ContatoValido(token, carimbo);
        dados["Website"] = "https://spam.example";
        var resposta = await cliente.PostAsync("/contato", new FormUrlEncodedContent(dados));

        Assert.Equal(HttpStatusCode.Redirect, resposta.StatusCode);
        Assert.Empty(site.Email.Enviados);
    }

    [Fact]
    public async Task EnvioImediato_ERecusadoComoRobo()
    {
        using var site = new SiteFactory();
        var cliente = site.Cliente();
        var (token, carimbo) = await Formulario.AbrirContatoAsync(cliente);

        var resposta = await cliente.PostAsync("/contato", new FormUrlEncodedContent(Formulario.ContatoValido(token, carimbo)));

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Empty(site.Email.Enviados);
    }

    [Fact]
    public async Task QuartoEnvioSeguido_EBloqueadoPeloLimite()
    {
        using var site = new SiteFactory();
        var cliente = site.Cliente();
        var (token, carimbo) = await Formulario.AbrirContatoAsync(cliente);
        site.Relogio.Advance(TimeSpan.FromSeconds(15));
        var dados = Formulario.ContatoValido(token, carimbo);

        for (var i = 0; i < 3; i++)
        {
            var permitido = await cliente.PostAsync("/contato", new FormUrlEncodedContent(dados));
            Assert.NotEqual(HttpStatusCode.TooManyRequests, permitido.StatusCode);
        }

        var bloqueado = await cliente.PostAsync("/contato", new FormUrlEncodedContent(dados));

        Assert.Equal(HttpStatusCode.TooManyRequests, bloqueado.StatusCode);
        Assert.Equal(3, site.Email.Enviados.Count);
    }

    [Fact]
    public async Task FalhaNoEmail_MostraMensagemSemQuebrarAPagina()
    {
        using var site = new SiteFactory();
        site.Email.Falhar = true;
        var cliente = site.Cliente();
        var (token, carimbo) = await Formulario.AbrirContatoAsync(cliente);
        site.Relogio.Advance(TimeSpan.FromSeconds(15));

        var resposta = await cliente.PostAsync("/contato", new FormUrlEncodedContent(Formulario.ContatoValido(token, carimbo)));
        var html = await resposta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Contains("Não conseguimos enviar sua mensagem agora", html);
    }
}
