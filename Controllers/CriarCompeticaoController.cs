using Microsoft.AspNetCore.Mvc;

namespace ProjetoLaboratorio25.Controllers
{
    public class CriarCompeticaoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(string nome, string tipo)
        {
            // Store the competition data in TempData to pass to the next view
            TempData["NomeCompeticao"] = nome;
            TempData["TipoCompeticao"] = tipo;

            // Redirect to FormatodaCompeticao
            return RedirectToAction("Index", "FormatodaCompeticao");
        }
    }
}
