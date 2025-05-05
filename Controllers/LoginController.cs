using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ProjetoLaboratorio25.Controllers
{
    public class LoginController : Controller
    {
        private readonly string usersFile = Path.Combine(Directory.GetCurrentDirectory(), "users.json");

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string senha)
        {
            // Ler utilizadores existentes
            List<Utilizador> utilizadores = new List<Utilizador>();
            if (System.IO.File.Exists(usersFile))
            {
                var json = System.IO.File.ReadAllText(usersFile);
                if (!string.IsNullOrWhiteSpace(json))
                    utilizadores = JsonSerializer.Deserialize<List<Utilizador>>(json);
            }

            // Validar credenciais
            var user = utilizadores.FirstOrDefault(u => 
                u.Email.Trim().ToLower() == email.Trim().ToLower() && 
                u.Senha.Trim() == senha.Trim());
            if (user == null)
            {
                ViewBag.Erro = "Email ou senha incorretos.";
                return View();
            }

            // Criar claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UtilizadorNome),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var authProperties = new AuthenticationProperties();

            await HttpContext.SignInAsync(
                "CookieAuth",
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirecionar para Dashboard
            return RedirectToAction("Index", "Home");
        }
    }
}
