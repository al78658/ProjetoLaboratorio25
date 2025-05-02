using Microsoft.AspNetCore.Mvc;

namespace ProjetoLaboratorio25.Controllers
{
    public class FormatodaCompeticaoController : Controller
    {
        public IActionResult Index()
        {
            // Get the competition data from TempData
            ViewBag.NomeCompeticao = TempData["NomeCompeticao"];
            ViewBag.TipoCompeticao = TempData["TipoCompeticao"];

            return View();
        }
    }
}
