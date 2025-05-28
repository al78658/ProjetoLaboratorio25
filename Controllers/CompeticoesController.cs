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

        [HttpGet]
        public JsonResult ExisteNome(string nome)
        {
            var existe = _context.Competicoes.Any(c => c.Nome.Trim().ToLower() == nome.Trim().ToLower());
            return Json(existe);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarCompeticao(int id)
        {
            var competicao = await _context.Competicoes.FindAsync(id);
            if (competicao != null)
            {
                _context.Competicoes.Remove(competicao);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpGet]
        public IActionResult ObterIdCompeticao(string nome)
        {
            var competicao = _context.Competicoes
                .FirstOrDefault(c => c.Nome == nome);
            
            if (competicao == null)
            {
                return Json(new { id = 0 });
            }
            
            return Json(new { id = competicao.Id });
        }

        [HttpPost]
        public IActionResult AtualizarTempData(int competicaoId)
        {
            var competicao = _context.Competicoes
                .Include(c => c.ConfiguracoesFase)
                .FirstOrDefault(c => c.Id == competicaoId);
            
            if (competicao == null)
            {
                return NotFound();
            }
            
            // Atualiza o TempData com os dados da competição
            TempData["CompeticaoId"] = competicao.Id;
            TempData["NomeCompeticao"] = competicao.Nome;
            TempData["TipoCompeticao"] = competicao.TipoCompeticao;
            
            // Se houver configurações de fase, atualiza também
            if (competicao.ConfiguracoesFase.Any())
            {
                var formatosDict = competicao.ConfiguracoesFase
                    .OrderBy(c => c.FaseNumero)
                    .ToDictionary(c => c.FaseNumero, c => c.Formato);
                
                TempData[$"FormatosSelecionados_{competicaoId}"] = System.Text.Json.JsonSerializer.Serialize(formatosDict);
                TempData[$"NumFases_{competicaoId}"] = competicao.ConfiguracoesFase.Count;
            }
            
            return Ok();
        }
    }
}
