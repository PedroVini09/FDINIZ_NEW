using System.Net;

namespace FDINIZ_Lab.Tests;

public class SegurancaTests : IClassFixture<SiteFactory>
{
    private readonly SiteFactory _site;

    public SegurancaTests(SiteFactory site) => _site = site;

    [Theory]
    [InlineData("/")]
    [InlineData("/exames")]
    [InlineData("/exames/hemograma-completo")]
    [InlineData("/unidades")]
    [InlineData("/contato")]
    [InlineData("/privacidade")]
    public async Task Paginas_AbremComCabecalhosDeSeguranca(string url)
    {
        var resposta = await _site.Cliente().GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var csp = Assert.Single(resposta.Headers.GetValues("Content-Security-Policy"));
        Assert.Contains("script-src 'self'", csp);
        Assert.Contains("frame-ancestors 'none'", csp);
        Assert.Equal("nosniff", Assert.Single(resposta.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal("DENY", Assert.Single(resposta.Headers.GetValues("X-Frame-Options")));
        Assert.False(resposta.Headers.Contains("Server"));
    }

    [Fact]
    public async Task ExameInexistente_Devolve404()
    {
        var resposta = await _site.Cliente().GetAsync("/exames/exame-que-nao-existe");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task BuscaComScript_ApareceEscapadaNaPagina()
    {
        var html = await _site.Cliente().GetStringAsync("/exames?q=%3Cscript%3Ealert(1)%3C%2Fscript%3E");

        Assert.DoesNotContain("<script>alert(1)</script>", html);
        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", html);
    }

    [Fact]
    public async Task FiltroComTentativaDeInjecao_EIgnorado()
    {
        var resposta = await _site.Cliente().GetAsync("/exames?categoria=%27%20OR%201%3D1%20--&preparo=%3Cx%3E&pagina=-1");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task PostSemTokenAntiCsrf_ERecusado()
    {
        var cliente = _site.Cliente();
        var dados = Formulario.ContatoValido(token: "", carimbo: "");
        dados.Remove("__RequestVerificationToken");

        var resposta = await cliente.PostAsync("/contato", new FormUrlEncodedContent(dados));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}
