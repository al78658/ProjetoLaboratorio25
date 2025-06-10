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
using Microsoft.Extensions.Logging;

namespace ProjetoLaboratorio25.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoginController> _logger;

        public LoginController(ApplicationDbContext context, ILogger<LoginController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string senha)
        {
            _logger.LogInformation($"[LOGIN] Tentativa de login para o email: {email}");

            // Buscar utilizador no banco de dados
            var user = await _context.Utilizadores
                .FirstOrDefaultAsync(u => 
                    u.Email.Trim().ToLower() == email.Trim().ToLower() && 
                    u.Senha.Trim() == senha.Trim());

            if (user == null)
            {
                _logger.LogWarning($"[LOGIN] Login falhou para o email: {email} - Usuário não encontrado");
                ViewBag.Erro = "Email ou senha incorretos.";
                return View();
            }

            _logger.LogInformation($"[LOGIN] Usuário encontrado: ID={user.Id}, Nome={user.UtilizadorNome}");

            // Armazenar informações na sessão
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetInt32("UserId", user.Id);

            _logger.LogInformation($"[LOGIN] Dados armazenados na sessão: Email={user.Email}, ID={user.Id}");

            // Criar claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("FullName", user.UtilizadorNome),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.Id.ToString())
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

            _logger.LogInformation($"[LOGIN] Login bem-sucedido para o usuário: {user.Email}");
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            _logger.LogInformation($"[LOGOUT] Iniciando logout para o usuário: {userEmail}");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            _logger.LogInformation("[LOGOUT] Sessão limpa e usuário deslogado com sucesso");
            return RedirectToAction("Index", "Home");
        }
    }
}
