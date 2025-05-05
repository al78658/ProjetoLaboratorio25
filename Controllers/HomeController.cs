using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Models;

namespace ProjetoLaboratorio25.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
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
        public async Task<IActionResult> Logout()
        {
            // Limpar todos os cookies de autenticação
            await HttpContext.SignOutAsync("CookieAuth");

            // Limpar a sessão
            HttpContext.Session.Clear();

            // Redirecionar para a página Home
            return RedirectToAction("Index", "Home");
        }
    }
}
