using System.ComponentModel.DataAnnotations;

namespace FDINIZ_Lab.Models;

public sealed class ContatoViewModel
{
    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 80 caracteres.")]
    [RegularExpression(@"^[^\r\n<>]*$", ErrorMessage = "O nome contém caracteres inválidos.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(120, ErrorMessage = "O e-mail deve ter no máximo 120 caracteres.")]
    [RegularExpression(@"^[^\r\n]*$", ErrorMessage = "O e-mail contém caracteres inválidos.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
    [RegularExpression(@"^[0-9()+\-\s]*$", ErrorMessage = "Use apenas números, espaços, parênteses e hífen no telefone.")]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [Required(ErrorMessage = "Escolha o assunto.")]
    [Display(Name = "Assunto")]
    public AssuntoContato? Assunto { get; set; }

    [Required(ErrorMessage = "Escreva sua mensagem.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "A mensagem deve ter entre 10 e 2000 caracteres.")]
    [Display(Name = "Mensagem")]
    public string Mensagem { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "É preciso aceitar a Política de Privacidade.")]
    [Display(Name = "Aceito a Política de Privacidade")]
    public bool AceitoPrivacidade { get; set; }

    /// <summary>Campo-armadilha: fica escondido para pessoas. Se vier preenchido, é robô.</summary>
    public string? Website { get; set; }

    /// <summary>Marca de tempo protegida, gerada quando o formulário é exibido.</summary>
    public string? Carimbo { get; set; }
}

public enum AssuntoContato
{
    [Display(Name = "Dúvida sobre exames")]
    Exames,

    [Display(Name = "Coleta domiciliar")]
    ColetaDomiciliar,

    [Display(Name = "Resultados")]
    Resultados,

    [Display(Name = "Convênios")]
    Convenios,

    [Display(Name = "Elogio ou sugestão")]
    ElogioSugestao,

    [Display(Name = "Reclamação")]
    Reclamacao,

    [Display(Name = "Outro assunto")]
    Outro
}
