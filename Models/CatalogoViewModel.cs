namespace FDINIZ_Lab.Models;

/// <summary>Filtros do catálogo, recebidos pela query string e sempre normalizados no servidor.</summary>
public sealed class FiltroCatalogo
{
    public const int TamanhoMaximoBusca = 60;
    public const int ItensPorPagina = 12;

    public string? Busca { get; init; }
    public IReadOnlyList<CategoriaExame> Categorias { get; init; } = [];
    public TipoPreparo? Preparo { get; init; }
    public int Pagina { get; init; } = 1;

    /// <summary>Cria um filtro seguro a partir dos valores brutos da URL.</summary>
    public static FiltroCatalogo Criar(string? busca, IEnumerable<string>? categorias, string? preparo, int pagina)
    {
        var buscaLimpa = string.IsNullOrWhiteSpace(busca) ? null : busca.Trim();
        if (buscaLimpa is { Length: > TamanhoMaximoBusca })
        {
            buscaLimpa = buscaLimpa[..TamanhoMaximoBusca];
        }

        var categoriasValidas = (categorias ?? [])
            .Select(c => Enum.TryParse<CategoriaExame>(c, ignoreCase: true, out var valor) ? valor : (CategoriaExame?)null)
            .Where(c => c is not null && Enum.IsDefined(c.Value))
            .Select(c => c!.Value)
            .Distinct()
            .ToList();

        TipoPreparo? preparoValido = Enum.TryParse<TipoPreparo>(preparo, ignoreCase: true, out var p) && Enum.IsDefined(p)
            ? p
            : null;

        return new FiltroCatalogo
        {
            Busca = buscaLimpa,
            Categorias = categoriasValidas,
            Preparo = preparoValido,
            Pagina = Math.Clamp(pagina, 1, 10_000)
        };
    }
}

public sealed record ResultadoCatalogo(
    IReadOnlyList<Exame> Itens,
    int Total,
    int Pagina,
    int TotalPaginas)
{
    public int Inicio => Total == 0 ? 0 : (Pagina - 1) * FiltroCatalogo.ItensPorPagina + 1;
    public int Fim => Math.Min(Pagina * FiltroCatalogo.ItensPorPagina, Total);
}

public sealed record CatalogoViewModel(FiltroCatalogo Filtro, ResultadoCatalogo Resultado);

public sealed record DetalheExameViewModel(Exame Exame, IReadOnlyList<Exame> Relacionados);
