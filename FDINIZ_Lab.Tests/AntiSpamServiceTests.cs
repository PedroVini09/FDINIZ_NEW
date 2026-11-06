using FDINIZ_Lab.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Time.Testing;

namespace FDINIZ_Lab.Tests;

public class AntiSpamServiceTests
{
    private readonly FakeTimeProvider _relogio = new(new DateTimeOffset(2026, 11, 6, 12, 0, 0, TimeSpan.Zero));
    private readonly AntiSpamService _servico;

    public AntiSpamServiceTests()
    {
        _servico = new AntiSpamService(new EphemeralDataProtectionProvider(), _relogio);
    }

    [Fact]
    public void EnvioNormal_EAprovado()
    {
        var carimbo = _servico.GerarCarimbo();
        _relogio.Advance(TimeSpan.FromSeconds(20));

        Assert.Equal(ResultadoAntiSpam.Aprovado, _servico.Validar(carimbo, campoArmadilha: null));
    }

    [Fact]
    public void CampoArmadilhaPreenchido_ERecusado()
    {
        var carimbo = _servico.GerarCarimbo();
        _relogio.Advance(TimeSpan.FromSeconds(20));

        Assert.Equal(ResultadoAntiSpam.CampoArmadilhaPreenchido, _servico.Validar(carimbo, "https://spam.example"));
    }

    [Fact]
    public void EnvioEmMenosDeTresSegundos_ERecusado()
    {
        var carimbo = _servico.GerarCarimbo();
        _relogio.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal(ResultadoAntiSpam.EnviadoRapidoDemais, _servico.Validar(carimbo, null));
    }

    [Fact]
    public void FormularioAbertoPorMaisDeDuasHoras_ERecusado()
    {
        var carimbo = _servico.GerarCarimbo();
        _relogio.Advance(TimeSpan.FromHours(3));

        Assert.Equal(ResultadoAntiSpam.FormularioExpirado, _servico.Validar(carimbo, null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1730894400")]
    [InlineData("carimbo-forjado")]
    public void CarimboAusenteOuForjado_ERecusado(string? carimbo)
    {
        Assert.Equal(ResultadoAntiSpam.CarimboInvalido, _servico.Validar(carimbo, null));
    }

    [Fact]
    public void CarimboDeOutraChave_ERecusado()
    {
        var outroServico = new AntiSpamService(new EphemeralDataProtectionProvider(), _relogio);
        var carimbo = outroServico.GerarCarimbo();
        _relogio.Advance(TimeSpan.FromSeconds(20));

        Assert.Equal(ResultadoAntiSpam.CarimboInvalido, _servico.Validar(carimbo, null));
    }
}
