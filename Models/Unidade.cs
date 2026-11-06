namespace FDINIZ_Lab.Models;

/// <summary>Unidade de atendimento, carregada de Data/unidades.json.</summary>
public sealed record Unidade
{
    public required string Slug { get; init; }
    public required string Nome { get; init; }
    public required string Regiao { get; init; }
    public required string Tipo { get; init; }
    public required string Telefone { get; init; }
    public IReadOnlyList<string> EnderecoLinhas { get; init; } = [];
    public IReadOnlyList<string> Facilidades { get; init; } = [];
    public string? HorarioSemana { get; init; }
    public string? HorarioSabado { get; init; }
    public string? HorarioDomingo { get; init; }
    public string? Imagem { get; init; }
    public string? Mapa { get; init; }

    public string TipoNome => Tipo switch
    {
        "matriz" => "Matriz",
        "policlinica" => "Policlínica",
        _ => "Laboratório"
    };
}

public sealed record Convenio(string Nome, string Logo);
