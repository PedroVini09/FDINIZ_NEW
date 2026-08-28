using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FDINIZ_Lab.Models;
using FDINIZ_Lab.Models.ViewModels;
using FDINIZ_Lab.Services;

namespace FDINIZ_Lab.Controllers;

public class HomeController : Controller
{
    private readonly IEmailService _emailService;
    public HomeController(IEmailService emailService)
    {
        _emailService = emailService;
    }
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Institucional()
    {
        return View();
    }
    public IActionResult Especialidades()
    {
        return View();
    }

    public IActionResult Exames()
    {
        return View();
    }

    public IActionResult Contato()
    {
        return View();
    }

    public IActionResult Kids()
    {
        return View();
    }

    public IActionResult Unidades()
    {
        return View();
    }

   


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarContato(ContatoMensagemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ContatoErro"] = "Preencha todos os campos corretamente.";
            return RedirectToAction("Contato");
        }

        try
        {
            await _emailService.EnviarMensagemContatoAsync(model);

            TempData["ContatoSucesso"] = "Mensagem enviada com sucesso!";
            return RedirectToAction("Contato");
        }
        catch (Exception ex)
        {
            TempData["ContatoErro"] = "Erro ao enviar e-mail: " + ex.Message;
            return RedirectToAction("Contato");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}