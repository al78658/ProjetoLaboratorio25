using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;

namespace ProjetoLaboratorio25.Controllers
{
    public class CompeticoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompeticoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var competicoes = await _context.Competicoes
                .Include(c => c.ConfiguracoesFase)
                .ToListAsync();
            return View(competicoes);
        }

        [HttpPost]
        public async Task<IActionResult> CriarCompeticao(string nome, string tipo, int numJogadores, int numEquipas)
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
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarConfiguracaoFase(int competicaoId, int faseNumero, string formato, int pontosVitoria, int pontosEmpate, int pontosDerrota)
        {
            var configuracao = new ConfiguracaoFase
            {
                CompeticaoId = competicaoId,
                FaseNumero = faseNumero,
                Formato = formato,
                PontosVitoria = pontosVitoria,
                PontosEmpate = pontosEmpate,
                PontosDerrota = pontosDerrota
            };

            _context.ConfiguracoesFase.Add(configuracao);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
