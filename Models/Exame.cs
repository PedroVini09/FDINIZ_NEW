namespace FDINIZ_Lab.Models;

/// <summary>Exame oferecido pelo laboratório, carregado de Data/exames.json.</summary>
public sealed record Exame
{
    public required string Slug { get; init; }
    public required string Nome { get; init; }
    public required string Codigo { get; init; }
    public required CategoriaExame Categoria { get; init; }
    public required string Material { get; init; }
    public required string Metodo { get; init; }
    public required string Preparo { get; init; }
    public required TipoPreparo TipoPreparo { get; init; }
    public required string Prazo { get; init; }

    /// <summary>Outros nomes pelos quais o exame é conhecido, usados na busca.</summary>
    public IReadOnlyList<string> Sinonimos { get; init; } = [];

    public bool DisponivelEmTodasUnidades { get; init; }
}

public enum CategoriaExame
{
    Sangue,
    Urina,
    Fezes,
    Imagem
}

public enum TipoPreparo
{
    SemPreparo,
    Jejum,
    Especial
}

public static class ExameRotulos
{
    public static string Nome(this CategoriaExame categoria) => categoria switch
    {
        CategoriaExame.Sangue => "Sangue",
        CategoriaExame.Urina => "Urina",
        CategoriaExame.Fezes => "Fezes",
        CategoriaExame.Imagem => "Imagem",
        _ => categoria.ToString()
    };

    public static string Sigla(this CategoriaExame categoria) => categoria switch
    {
        CategoriaExame.Sangue => "SANGUE",
        CategoriaExame.Urina => "URINA",
        CategoriaExame.Fezes => "FEZES",
        CategoriaExame.Imagem => "IMAGEM",
        _ => categoria.ToString().ToUpperInvariant()
    };

    public static string Icone(this CategoriaExame categoria) => categoria switch
    {
        CategoriaExame.Sangue => "fa-droplet",
        CategoriaExame.Urina => "fa-flask",
        CategoriaExame.Fezes => "fa-vial",
        CategoriaExame.Imagem => "fa-x-ray",
        _ => "fa-vials"
    };

    public static string Nome(this TipoPreparo tipo) => tipo switch
    {
        TipoPreparo.SemPreparo => "Sem preparo",
        TipoPreparo.Jejum => "Jejum obrigatório",
        TipoPreparo.Especial => "Preparo especial",
        _ => tipo.ToString()
    };
}
