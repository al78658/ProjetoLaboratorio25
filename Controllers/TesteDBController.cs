using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;

namespace ProjetoLaboratorio25.Controllers
{
    public class TesteDBController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TesteDBController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Buscar todos os dados
            var utilizadores = _context.Utilizadores.ToList();
            var competicoes = _context.Competicoes.ToList();
            var configuracoes = _context.ConfiguracoesFase.ToList();

            ViewBag.Utilizadores = utilizadores;
            ViewBag.Competicoes = competicoes;
            ViewBag.Configuracoes = configuracoes;

            return View();
        }

        [HttpPost]
        public IActionResult CriarUtilizador(string nome, string email, string senha)
        {
            var novoUtilizador = new Utilizador
            {
                UtilizadorNome = nome,
                Email = email,
                Senha = senha
            };

            _context.Utilizadores.Add(novoUtilizador);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CriarCompeticao(string nome, string tipo, int numJogadores, int numEquipas)
        {
            var novaCompeticao = new Competicao
            {
                Nome = nome,
                TipoCompeticao = tipo,
                NumJogadores = numJogadores,
                NumEquipas = numEquipas,
                PontosVitoria = 3,
                PontosEmpate = 1
            };

            _context.Competicoes.Add(novaCompeticao);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
} 