using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ProjetoLaboratorio25.Controllers
{
    public class RegistarController : Controller
    {
        private readonly string usersFile = Path.Combine(Directory.GetCurrentDirectory(), "users.json");

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string utilizador, string email, string senha)
        {
            // Ler utilizadores existentes
            List<Utilizador> utilizadores = new List<Utilizador>();
            if (System.IO.File.Exists(usersFile))
            {
                var json = System.IO.File.ReadAllText(usersFile);
                if (!string.IsNullOrWhiteSpace(json))
                    utilizadores = JsonSerializer.Deserialize<List<Utilizador>>(json);
            }

            // Verificar se já existe email
            if (utilizadores.Any(u => u.Email.Trim().ToLower() == email.Trim().ToLower()))
            {
                ViewBag.Erro = "Já existe uma conta com este email.";
                return View();
            }

            // Adicionar novo utilizador
            utilizadores.Add(new Utilizador { UtilizadorNome = utilizador, Email = email, Senha = senha });
            
            // Garantir que o diretório existe
            Directory.CreateDirectory(Path.GetDirectoryName(usersFile));
            System.IO.File.WriteAllText(usersFile, JsonSerializer.Serialize(utilizadores));

            // Redirecionar para login após registo
            return RedirectToAction("Index", "Login");
        }
    }
}
