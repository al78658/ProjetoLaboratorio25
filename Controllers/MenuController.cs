using Microsoft.AspNetCore.Mvc;

namespace ProjetoLaboratorio25.Controllers
{
    public class MenuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
