using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FDINIZ_Lab.Controllers;

public class HomeController : Controller
{
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

    public IActionResult Privacy()
    {
        return View();
    }

    // [ResponseCache(
    //     Duration = 0,
    //     Location = ResponseCacheLocation.None,
    //     NoStore = true)]
    // public IActionResult Error()
    // {
    //     return View(new ErrorViewModel
    //     {
    //         RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    //     });
    // }
}