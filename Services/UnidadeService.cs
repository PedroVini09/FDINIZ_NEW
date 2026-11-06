using System.Text.Json;
using FDINIZ_Lab.Models;

namespace FDINIZ_Lab.Services;

public interface IUnidadeService
{
    IReadOnlyList<Unidade> Todas { get; }
    IReadOnlyList<Unidade> PorRegiao(string? regiao);
    IReadOnlyList<(string Valor, string Nome)> Regioes { get; }
    IReadOnlyList<Convenio> Convenios { get; }
}

public sealed class JsonUnidadeService : IUnidadeService
{
    private static readonly Dictionary<string, string> NomesRegioes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["centro"] = "Centro",
        ["catole"] = "Catolé",
        ["malvinas"] = "Malvinas",
        ["liberdade"] = "Liberdade",
        ["prata"] = "Prata",
        ["inga"] = "Ingá",
        ["queimadas"] = "Queimadas",
        ["sao-jose"] = "São José da Mata",
        ["bezerra-carvalho"] = "Bezerra Carvalho"
    };

    public JsonUnidadeService(IReadOnlyList<Unidade> unidades, IReadOnlyList<Convenio> convenios)
    {
        Todas = unidades;
        Convenios = convenios;
        Regioes = unidades
            .Select(u => u.Regiao)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(r => (r, NomesRegioes.GetValueOrDefault(r, r)))
            .ToList();
    }

    public static JsonUnidadeService CarregarDeArquivos(string unidades, string convenios) => new(
        JsonSerializer.Deserialize<List<Unidade>>(File.ReadAllText(unidades), JsonOpcoes.Padrao) ?? [],
        JsonSerializer.Deserialize<List<Convenio>>(File.ReadAllText(convenios), JsonOpcoes.Padrao) ?? []);

    public IReadOnlyList<Unidade> Todas { get; }
    public IReadOnlyList<Convenio> Convenios { get; }
    public IReadOnlyList<(string Valor, string Nome)> Regioes { get; }

    /// <summary>Região desconhecida devolve todas as unidades, nunca um erro.</summary>
    public IReadOnlyList<Unidade> PorRegiao(string? regiao) =>
        string.IsNullOrWhiteSpace(regiao) || !Regioes.Any(r => r.Valor.Equals(regiao, StringComparison.OrdinalIgnoreCase))
            ? Todas
            : Todas.Where(u => u.Regiao.Equals(regiao, StringComparison.OrdinalIgnoreCase)).ToList();
}
