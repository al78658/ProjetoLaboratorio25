using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;

namespace ProjetoLaboratorio25.Controllers
{
    public class CriarCompeticaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CriarCompeticaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(string nome, string tipo)
        {
            // Validar dados do formulário
            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(tipo))
            {
                return View();
            }

            // Criar nova competição com dados básicos
            var competicao = new Competicao
            {
                Nome = nome,
                TipoCompeticao = tipo
            };

            try
            {
                // Adicionar competição ao contexto
                _context.Competicoes.Add(competicao);
                
                // Salvar mudanças no banco de dados
                _context.SaveChanges();

                // Armazenar dados temporários para próxima view
                TempData["NomeCompeticao"] = nome;
                TempData["TipoCompeticao"] = tipo;
                TempData["CompeticaoId"] = competicao.Id;

                // Redirecionar para configuração de formato
                return RedirectToAction("Index", "FormatodaCompeticao");
            }
            catch (Exception ex)
            {
                // Em caso de erro, mostrar mensagem e retornar à view
                ModelState.AddModelError("", "Erro ao criar competição: " + ex.Message);
                return View();
            }
        }
    }
}
