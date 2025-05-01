using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ProjetoLaboratorio25.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
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