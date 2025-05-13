using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Models;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;

namespace ProjetoLaboratorio25.Controllers
{
    public class RegistarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegistarController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string utilizador, string email, string senha)
        {
            // Verificar se já existe email
            if (await _context.Utilizadores.AnyAsync(u => u.Email.Trim().ToLower() == email.Trim().ToLower()))
            {
                ViewBag.Erro = "Já existe uma conta com este email.";
                return View();
            }

            // Adicionar novo utilizador
            var novoUtilizador = new Utilizador 
            { 
                UtilizadorNome = utilizador, 
                Email = email, 
                Senha = senha 
            };
            
            _context.Utilizadores.Add(novoUtilizador);
            await _context.SaveChangesAsync();

            // Redirecionar para login após registo
            return RedirectToAction("Index", "Login");
        }
    }
}
