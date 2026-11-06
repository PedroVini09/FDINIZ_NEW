using FDINIZ_Lab.Models;
using FDINIZ_Lab.Services;
using Microsoft.AspNetCore.Mvc;

namespace FDINIZ_Lab.Controllers;

[Route("exames")]
public class ExamesController(IExameService exames) : Controller
{
    [HttpGet("")]
    public IActionResult Index(
        [FromQuery(Name = "q")] string? busca,
        [FromQuery(Name = "categoria")] string[]? categorias,
        [FromQuery(Name = "preparo")] string? preparo,
        [FromQuery(Name = "pagina")] int pagina = 1)
    {
        var filtro = FiltroCatalogo.Criar(busca, categorias, preparo, pagina);
        ViewBag.Contagens = Enum.GetValues<CategoriaExame>().ToDictionary(c => c, exames.Contar);
        return View(new CatalogoViewModel(filtro, exames.Buscar(filtro)));
    }

    [HttpGet("{slug:regex(^[[a-z0-9-]]{{1,80}}$)}")]
    public IActionResult Detalhe(string slug)
    {
        var exame = exames.ObterPorSlug(slug);
        return exame is null
            ? NotFound()
            : View(new DetalheExameViewModel(exame, exames.Relacionados(exame)));
    }
}
