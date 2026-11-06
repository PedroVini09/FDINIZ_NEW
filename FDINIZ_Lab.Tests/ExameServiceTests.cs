using FDINIZ_Lab.Models;
using FDINIZ_Lab.Services;

namespace FDINIZ_Lab.Tests;

public class ExameServiceTests
{
    private static readonly JsonExameService Servico = JsonExameService.CarregarDeArquivo(
        Path.Combine(AppContext.BaseDirectory, "Data", "exames.json"));

    [Fact]
    public void Catalogo_CarregaOsExamesDoArquivo()
    {
        Assert.NotEmpty(Servico.Todos);
        Assert.All(Servico.Todos, e => Assert.Matches("^[a-z0-9-]+$", e.Slug));
    }

    [Fact]
    public void Slugs_SaoUnicos()
    {
        Assert.Equal(Servico.Todos.Count, Servico.Todos.Select(e => e.Slug).Distinct().Count());
    }

    [Theory]
    [InlineData("hemograma")]
    [InlineData("HEMOGRAMA")]
    [InlineData("hemográma")]
    [InlineData("  hemograma  ")]
    public void Busca_IgnoraAcentosMaiusculasEEspacos(string termo)
    {
        var resultado = Servico.Buscar(FiltroCatalogo.Criar(termo, null, null, 1));

        Assert.Contains(resultado.Itens, e => e.Slug == "hemograma-completo");
    }

    [Fact]
    public void Busca_EncontraPorSinonimo()
    {
        var resultado = Servico.Buscar(FiltroCatalogo.Criar("glicose", null, null, 1));

        Assert.Contains(resultado.Itens, e => e.Slug == "glicemia-em-jejum");
    }

    [Fact]
    public void Busca_SemResultado_DevolveListaVazia()
    {
        var resultado = Servico.Buscar(FiltroCatalogo.Criar("exame-que-nao-existe", null, null, 1));

        Assert.Empty(resultado.Itens);
        Assert.Equal(0, resultado.Total);
    }

    [Fact]
    public void Filtro_PorCategoriaEPreparo()
    {
        var resultado = Servico.Buscar(FiltroCatalogo.Criar(null, ["Sangue"], "Jejum", 1));

        Assert.NotEmpty(resultado.Itens);
        Assert.All(resultado.Itens, e =>
        {
            Assert.Equal(CategoriaExame.Sangue, e.Categoria);
            Assert.Equal(TipoPreparo.Jejum, e.TipoPreparo);
        });
    }

    [Fact]
    public void PaginaAlemDoFim_VoltaParaAUltimaPagina()
    {
        var resultado = Servico.Buscar(FiltroCatalogo.Criar(null, null, null, 999));

        Assert.Equal(resultado.TotalPaginas, resultado.Pagina);
        Assert.NotEmpty(resultado.Itens);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("nao-existe")]
    [InlineData("../../appsettings")]
    public void ObterPorSlug_Inexistente_DevolveNulo(string? slug)
    {
        Assert.Null(Servico.ObterPorSlug(slug));
    }

    [Fact]
    public void Relacionados_SaoDaMesmaCategoriaESemOProprioExame()
    {
        var exame = Servico.ObterPorSlug("hemograma-completo")!;

        var relacionados = Servico.Relacionados(exame);

        Assert.All(relacionados, r =>
        {
            Assert.Equal(exame.Categoria, r.Categoria);
            Assert.NotEqual(exame.Slug, r.Slug);
        });
    }
}
