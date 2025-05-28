using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using System.Linq;

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
            var competicoes = _context.Competicoes.ToList();
            var jogadores = _context.Jogadores.ToList();
            var emparelhamentosFinal = _context.EmparelhamentosFinal.ToList();

            return View(new { 
                competicoes = competicoes,
                jogadores = jogadores,
                emparelhamentosFinal = emparelhamentosFinal
            });
        }

        public IActionResult Privacy()
        {
            return View();
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