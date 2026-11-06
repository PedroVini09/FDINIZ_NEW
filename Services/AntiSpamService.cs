using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace FDINIZ_Lab.Services;

public enum ResultadoAntiSpam
{
    Aprovado,
    CampoArmadilhaPreenchido,
    EnviadoRapidoDemais,
    FormularioExpirado,
    CarimboInvalido
}

public interface IAntiSpamService
{
    /// <summary>Gera uma marca de tempo assinada para colocar no formulário.</summary>
    string GerarCarimbo();

    ResultadoAntiSpam Validar(string? carimbo, string? campoArmadilha);
}

/// <summary>
/// Barra robôs sem CAPTCHA: um campo escondido que pessoas não preenchem e um tempo mínimo
/// entre abrir e enviar o formulário. A marca de tempo é assinada, então não pode ser forjada.
/// </summary>
public sealed class AntiSpamService(IDataProtectionProvider protecao, TimeProvider relogio) : IAntiSpamService
{
    public static readonly TimeSpan TempoMinimo = TimeSpan.FromSeconds(3);
    public static readonly TimeSpan TempoMaximo = TimeSpan.FromHours(2);

    private readonly IDataProtector _protetor = protecao.CreateProtector("FDINIZ.Formularios.Carimbo.v1");

    public string GerarCarimbo() =>
        _protetor.Protect(relogio.GetUtcNow().UtcTicks.ToString(CultureInfo.InvariantCulture));

    public ResultadoAntiSpam Validar(string? carimbo, string? campoArmadilha)
    {
        if (!string.IsNullOrEmpty(campoArmadilha))
        {
            return ResultadoAntiSpam.CampoArmadilhaPreenchido;
        }

        if (string.IsNullOrWhiteSpace(carimbo) || carimbo.Length > 1024)
        {
            return ResultadoAntiSpam.CarimboInvalido;
        }

        long ticks;
        try
        {
            ticks = long.Parse(_protetor.Unprotect(carimbo), CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException or OverflowException)
        {
            return ResultadoAntiSpam.CarimboInvalido;
        }

        var decorrido = relogio.GetUtcNow() - new DateTimeOffset(ticks, TimeSpan.Zero);

        if (decorrido < TempoMinimo) return ResultadoAntiSpam.EnviadoRapidoDemais;
        if (decorrido > TempoMaximo) return ResultadoAntiSpam.FormularioExpirado;
        return ResultadoAntiSpam.Aprovado;
    }
}
