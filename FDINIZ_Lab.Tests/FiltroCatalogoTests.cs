using FDINIZ_Lab.Models;

namespace FDINIZ_Lab.Tests;

public class FiltroCatalogoTests
{
    [Fact]
    public void BuscaMuitoLonga_ECortadaNoLimite()
    {
        var filtro = FiltroCatalogo.Criar(new string('a', 500), null, null, 1);

        Assert.Equal(FiltroCatalogo.TamanhoMaximoBusca, filtro.Busca!.Length);
    }

    [Fact]
    public void BuscaEmBranco_ViraNula()
    {
        Assert.Null(FiltroCatalogo.Criar("   ", null, null, 1).Busca);
    }

    [Fact]
    public void ValoresDesconhecidos_SaoIgnorados()
    {
        var filtro = FiltroCatalogo.Criar(null, ["Sangue", "<script>", "99", "sangue"], "' OR 1=1 --", 1);

        Assert.Equal([CategoriaExame.Sangue], filtro.Categorias);
        Assert.Null(filtro.Preparo);
    }

    [Theory]
    [InlineData(-5, 1)]
    [InlineData(0, 1)]
    [InlineData(3, 3)]
    [InlineData(int.MaxValue, 10_000)]
    public void Pagina_FicaDentroDoIntervalo(int recebida, int esperada)
    {
        Assert.Equal(esperada, FiltroCatalogo.Criar(null, null, null, recebida).Pagina);
    }
}
