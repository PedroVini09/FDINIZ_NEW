using System.ComponentModel.DataAnnotations;
using FDINIZ_Lab.Models;
using FDINIZ_Lab.Services;

namespace FDINIZ_Lab.Tests;

public class ContatoTests
{
    private static ContatoViewModel Valido() => new()
    {
        Nome = "Maria Souza",
        Email = "maria@example.com",
        Telefone = "(83) 99999-0000",
        Assunto = AssuntoContato.Exames,
        Mensagem = "Gostaria de saber o preparo do hemograma.",
        AceitoPrivacidade = true
    };

    private static List<ValidationResult> Validar(ContatoViewModel modelo)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(modelo, new ValidationContext(modelo), resultados, validateAllProperties: true);
        return resultados;
    }

    [Fact]
    public void ContatoValido_PassaNaValidacao()
    {
        Assert.Empty(Validar(Valido()));
    }

    [Theory]
    [InlineData("Maria\r\nBcc: vitima@example.com")]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("A")]
    public void NomeInvalido_ERecusado(string nome)
    {
        var modelo = Valido();
        modelo.Nome = nome;

        Assert.Contains(Validar(modelo), r => r.MemberNames.Contains(nameof(ContatoViewModel.Nome)));
    }

    [Fact]
    public void EmailComQuebraDeLinha_ERecusado()
    {
        var modelo = Valido();
        modelo.Email = "maria@example.com\r\nBcc: vitima@example.com";

        Assert.Contains(Validar(modelo), r => r.MemberNames.Contains(nameof(ContatoViewModel.Email)));
    }

    [Fact]
    public void SemAceitarAPrivacidade_ERecusado()
    {
        var modelo = Valido();
        modelo.AceitoPrivacidade = false;

        Assert.Contains(Validar(modelo), r => r.MemberNames.Contains(nameof(ContatoViewModel.AceitoPrivacidade)));
    }

    [Fact]
    public void MensagemGigante_ERecusada()
    {
        var modelo = Valido();
        modelo.Mensagem = new string('x', 2001);

        Assert.Contains(Validar(modelo), r => r.MemberNames.Contains(nameof(ContatoViewModel.Mensagem)));
    }

    [Fact]
    public void Email_EscapaHtmlDigitadoPeloUsuario()
    {
        var modelo = Valido();
        modelo.Mensagem = "<script>alert('xss')</script><img src=x onerror=alert(1)>";

        var email = ContatoEmailFormatter.Formatar(modelo, AssuntoContato.Exames, "Dúvida sobre exames");

        Assert.DoesNotContain("<script>", email.CorpoHtml);
        Assert.DoesNotContain("<img", email.CorpoHtml);
        Assert.Contains("&lt;script&gt;", email.CorpoHtml);
    }

    [Fact]
    public void Assunto_NaoAceitaQuebraDeLinha()
    {
        var modelo = Valido();
        modelo.Nome = "Maria\r\nBcc: vitima@example.com";

        var email = ContatoEmailFormatter.Formatar(modelo, AssuntoContato.Exames, "Dúvida sobre exames");

        Assert.DoesNotContain('\r', email.Assunto);
        Assert.DoesNotContain('\n', email.Assunto);
    }
}
