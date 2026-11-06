using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FDINIZ_Lab.Models;
using FDINIZ_Lab.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FDINIZ_Lab.Controllers;

[Route("contato")]
public class ContatoController(
    IAntiSpamService antiSpam,
    IEmailService email,
    ILogger<ContatoController> logger) : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View(new ContatoViewModel { Carimbo = antiSpam.GerarCarimbo() });

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(PoliticasLimite.Formularios)]
    public async Task<IActionResult> Index(ContatoViewModel modelo, CancellationToken cancellationToken)
    {
        var verificacao = antiSpam.Validar(modelo.Carimbo, modelo.Website);

        if (verificacao is ResultadoAntiSpam.CampoArmadilhaPreenchido)
        {
            // Robô: responde como se tivesse dado certo, para ele não aprender a contornar.
            logger.LogInformation("Formulário de contato descartado pelo campo-armadilha.");
            return RedirectToAction(nameof(Enviado));
        }

        if (verificacao is not ResultadoAntiSpam.Aprovado)
        {
            logger.LogInformation("Formulário de contato recusado pelo anti-spam: {Motivo}.", verificacao);
            ModelState.AddModelError(string.Empty, verificacao == ResultadoAntiSpam.FormularioExpirado
                ? "O formulário ficou aberto por muito tempo. Confira os dados e envie de novo."
                : "Não foi possível validar o envio. Aguarde alguns segundos e tente de novo.");
        }

        if (!ModelState.IsValid)
        {
            modelo.Carimbo = antiSpam.GerarCarimbo();
            return View(modelo);
        }

        var assunto = modelo.Assunto!.Value;
        var mensagem = ContatoEmailFormatter.Formatar(modelo, assunto, NomeDoAssunto(assunto));

        if (!await email.EnviarAsync(mensagem, cancellationToken))
        {
            ModelState.AddModelError(string.Empty,
                "Não conseguimos enviar sua mensagem agora. Tente mais tarde ou ligue para (83) 3315-7373.");
            modelo.Carimbo = antiSpam.GerarCarimbo();
            return View(modelo);
        }

        return RedirectToAction(nameof(Enviado));
    }

    [HttpGet("enviado")]
    public IActionResult Enviado() => View();

    internal static string NomeDoAssunto(AssuntoContato assunto) =>
        typeof(AssuntoContato).GetField(assunto.ToString())?.GetCustomAttribute<DisplayAttribute>()?.Name
        ?? assunto.ToString();
}
