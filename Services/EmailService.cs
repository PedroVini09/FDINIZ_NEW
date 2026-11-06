using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using FDINIZ_Lab.Models;
using Microsoft.Extensions.Options;

namespace FDINIZ_Lab.Services;

public sealed record MensagemEmail(string Assunto, string CorpoHtml, string CorpoTexto, string? ResponderPara);

public interface IEmailService
{
    /// <returns>true quando a mensagem foi entregue ao servidor SMTP.</returns>
    Task<bool> EnviarAsync(MensagemEmail mensagem, CancellationToken cancellationToken = default);
}

/// <summary>Monta o e-mail do formulário de contato. Todo texto do usuário é escapado.</summary>
public static class ContatoEmailFormatter
{
    public static MensagemEmail Formatar(ContatoViewModel contato, AssuntoContato assunto, string assuntoNome)
    {
        var html = HtmlEncoder.Create(System.Text.Unicode.UnicodeRanges.All);
        var nome = UmaLinha(contato.Nome);
        var email = UmaLinha(contato.Email);
        var telefone = UmaLinha(contato.Telefone ?? "Não informado");

        var corpoHtml = new StringBuilder()
            .Append("<h2>Nova mensagem pelo site</h2>")
            .Append("<table cellpadding=\"6\">")
            .Append($"<tr><td><b>Assunto</b></td><td>{html.Encode(assuntoNome)}</td></tr>")
            .Append($"<tr><td><b>Nome</b></td><td>{html.Encode(nome)}</td></tr>")
            .Append($"<tr><td><b>E-mail</b></td><td>{html.Encode(email)}</td></tr>")
            .Append($"<tr><td><b>Telefone</b></td><td>{html.Encode(telefone)}</td></tr>")
            .Append("</table>")
            .Append("<h3>Mensagem</h3>")
            .Append($"<p style=\"white-space:pre-wrap\">{html.Encode(contato.Mensagem)}</p>")
            .ToString();

        var corpoTexto =
            $"Assunto: {assuntoNome}\nNome: {nome}\nE-mail: {email}\nTelefone: {telefone}\n\n{contato.Mensagem}";

        return new MensagemEmail(
            Assunto: $"[Site] {assuntoNome} - {nome}",
            CorpoHtml: corpoHtml,
            CorpoTexto: corpoTexto,
            ResponderPara: email);
    }

    /// <summary>Remove quebras de linha para impedir injeção de cabeçalhos no e-mail.</summary>
    internal static string UmaLinha(string valor) =>
        valor.Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal).Trim();
}

public sealed class SmtpEmailService(IOptions<EmailSettings> opcoes, ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly EmailSettings _config = opcoes.Value;

    public async Task<bool> EnviarAsync(MensagemEmail mensagem, CancellationToken cancellationToken = default)
    {
        if (!_config.Configurado)
        {
            logger.LogWarning("Envio de e-mail ignorado: a configuração de SMTP está incompleta.");
            return false;
        }

        using var email = new MailMessage
        {
            From = new MailAddress(_config.Remetente, _config.RemetenteNome),
            Subject = mensagem.Assunto,
            Body = mensagem.CorpoHtml,
            IsBodyHtml = true,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
        };
        email.To.Add(_config.Destinatario);
        email.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(mensagem.CorpoTexto, Encoding.UTF8, "text/plain"));

        if (mensagem.ResponderPara is { } responder && MailAddress.TryCreate(responder, out var endereco))
        {
            email.ReplyToList.Add(endereco);
        }

        using var smtp = new SmtpClient(_config.SmtpServidor, _config.SmtpPorta)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_config.Usuario, _config.Senha),
            Timeout = 15_000
        };

        try
        {
            await smtp.SendMailAsync(email, cancellationToken);
            return true;
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException)
        {
            // Não registra o conteúdo nem os dados pessoais da mensagem (LGPD).
            logger.LogError(ex, "Falha ao enviar e-mail pelo servidor SMTP.");
            return false;
        }
    }
}
