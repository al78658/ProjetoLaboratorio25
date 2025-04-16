using Microsoft.AspNetCore.Mvc;

namespace ProjetoLaboratorio25.Controllers
{
    public class ListadeJogadoresController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
