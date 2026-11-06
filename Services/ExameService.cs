using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FDINIZ_Lab.Models;

namespace FDINIZ_Lab.Services;

public interface IExameService
{
    IReadOnlyList<Exame> Todos { get; }
    ResultadoCatalogo Buscar(FiltroCatalogo filtro);
    Exame? ObterPorSlug(string? slug);
    IReadOnlyList<Exame> Relacionados(Exame exame, int quantidade = 4);
    int Contar(CategoriaExame categoria);
}

/// <summary>Catálogo em memória, lido uma vez de um arquivo JSON. Não há banco de dados.</summary>
public sealed class JsonExameService : IExameService
{
    private readonly IReadOnlyList<Exame> _exames;
    private readonly Dictionary<string, Exame> _porSlug;
    private readonly Dictionary<string, string> _textoBusca;

    public JsonExameService(IReadOnlyList<Exame> exames)
    {
        _exames = exames.OrderBy(e => e.Nome, StringComparer.Create(new CultureInfo("pt-BR"), true)).ToList();
        _porSlug = _exames.ToDictionary(e => e.Slug, StringComparer.OrdinalIgnoreCase);
        _textoBusca = _exames.ToDictionary(
            e => e.Slug,
            e => Normalizar(string.Join(' ', [e.Nome, e.Codigo, e.Material, .. e.Sinonimos])));
    }

    public static JsonExameService CarregarDeArquivo(string caminho) =>
        new(JsonSerializer.Deserialize<List<Exame>>(File.ReadAllText(caminho), JsonOpcoes.Padrao)
            ?? throw new InvalidOperationException($"Não foi possível ler o catálogo de exames em {caminho}."));

    public IReadOnlyList<Exame> Todos => _exames;

    public ResultadoCatalogo Buscar(FiltroCatalogo filtro)
    {
        IEnumerable<Exame> consulta = _exames;

        if (filtro.Busca is { } busca)
        {
            var termos = Normalizar(busca).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            consulta = consulta.Where(e => termos.All(t => _textoBusca[e.Slug].Contains(t, StringComparison.Ordinal)));
        }

        if (filtro.Categorias.Count > 0)
        {
            consulta = consulta.Where(e => filtro.Categorias.Contains(e.Categoria));
        }

        if (filtro.Preparo is { } preparo)
        {
            consulta = consulta.Where(e => e.TipoPreparo == preparo);
        }

        var filtrados = consulta.ToList();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(filtrados.Count / (double)FiltroCatalogo.ItensPorPagina));
        var pagina = Math.Min(filtro.Pagina, totalPaginas);

        var itens = filtrados
            .Skip((pagina - 1) * FiltroCatalogo.ItensPorPagina)
            .Take(FiltroCatalogo.ItensPorPagina)
            .ToList();

        return new ResultadoCatalogo(itens, filtrados.Count, pagina, totalPaginas);
    }

    public Exame? ObterPorSlug(string? slug) =>
        string.IsNullOrWhiteSpace(slug) ? null : _porSlug.GetValueOrDefault(slug.Trim());

    public IReadOnlyList<Exame> Relacionados(Exame exame, int quantidade = 4) =>
        _exames.Where(e => e.Categoria == exame.Categoria && e.Slug != exame.Slug).Take(quantidade).ToList();

    public int Contar(CategoriaExame categoria) => _exames.Count(e => e.Categoria == categoria);

    /// <summary>Remove acentos e caixa para a busca achar "hemograma" ao digitar "HEMOGRAMA" ou "hemográma".</summary>
    internal static string Normalizar(string texto)
    {
        var decomposto = texto.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposto.Length);
        foreach (var c in decomposto)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(c);
            if (categoria == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(char.IsLetterOrDigit(c) ? c : ' ');
        }
        return sb.ToString();
    }
}

internal static class JsonOpcoes
{
    public static readonly JsonSerializerOptions Padrao = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        ReadCommentHandling = JsonCommentHandling.Skip
    };
}
