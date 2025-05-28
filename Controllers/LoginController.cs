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
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;

namespace ProjetoLaboratorio25.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string senha)
        {
            // Buscar utilizador no banco de dados
            var user = await _context.Utilizadores
                .FirstOrDefaultAsync(u => 
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

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
