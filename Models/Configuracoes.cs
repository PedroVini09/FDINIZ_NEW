using System.ComponentModel.DataAnnotations;

namespace FDINIZ_Lab.Models;

/// <summary>Dados públicos do laboratório exibidos no site (seção "Site" do appsettings).</summary>
public sealed class SiteSettings
{
    public const string Secao = "Site";

    public string Telefone { get; init; } = "(83) 3315-7373";
    public string WhatsAppNumero { get; init; } = "558333157373";
    public string EmailPublico { get; init; } = "contato@laboratoriodiniz.com.br";
    public string ResultadosUrl { get; init; } = "";
    public string HelpdeskUrl { get; init; } = "";

    /// <summary>Caminho do folder em wwwroot. O botão só aparece se o arquivo existir.</summary>
    public string FolderInstitucional { get; init; } = "/docs/folder-institucional.pdf";

    public string WhatsAppLink(string? mensagem = null) =>
        $"https://wa.me/{WhatsAppNumero}" + (mensagem is null ? "" : "?text=" + Uri.EscapeDataString(mensagem));
}

/// <summary>Configuração de envio de e-mail (seção "Email"). A senha vem só de variável de ambiente.</summary>
public sealed class EmailSettings
{
    public const string Secao = "Email";

    [Required] public string SmtpServidor { get; init; } = "";
    [Range(1, 65535)] public int SmtpPorta { get; init; } = 587;
    [Required] public string Usuario { get; init; } = "";
    public string Senha { get; init; } = "";
    [Required, EmailAddress] public string Remetente { get; init; } = "";
    public string RemetenteNome { get; init; } = "Site Laboratório F. Diniz";
    [Required, EmailAddress] public string Destinatario { get; init; } = "";

    public bool Configurado =>
        !string.IsNullOrWhiteSpace(SmtpServidor) &&
        !string.IsNullOrWhiteSpace(Usuario) &&
        !string.IsNullOrWhiteSpace(Senha) &&
        !string.IsNullOrWhiteSpace(Remetente) &&
        !string.IsNullOrWhiteSpace(Destinatario);
}
