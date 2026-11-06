using System.Diagnostics;
using FDINIZ_Lab.Services;
using Microsoft.AspNetCore.Mvc;

namespace FDINIZ_Lab.Controllers;

public class HomeController(IUnidadeService unidades, IExameService exames) : Controller
{
    public IActionResult Index()
    {
        ViewBag.Convenios = unidades.Convenios;
        ViewBag.TotalExames = exames.Todos.Count;
        ViewBag.TotalUnidades = unidades.Todas.Count;
        return View();
    }

    [Route("unidades")]
    public IActionResult Unidades(string? regiao)
    {
        ViewBag.Regioes = unidades.Regioes;
        ViewBag.RegiaoAtual = unidades.Regioes.Any(r => r.Valor == regiao) ? regiao : null;
        return View(unidades.PorRegiao(regiao));
    }

    [Route("privacidade")]
    public IActionResult Privacy() => View();

    [Route("erro/{codigo:int?}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? codigo)
    {
        ViewBag.Codigo = codigo ?? 500;
        ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View();
    }
}
