using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;



namespace ProjetoLaboratorio25.Controllers
{
    public class FormatodaCompeticaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FormatodaCompeticaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Get the competition data from TempData
            ViewBag.NomeCompeticao = TempData["NomeCompeticao"];
            ViewBag.TipoCompeticao = TempData["TipoCompeticao"];
            ViewBag.CompeticaoId = TempData["CompeticaoId"];

            var competicaoId = TempData["CompeticaoId"]?.ToString();
            ViewBag.FormatosSelecionados = competicaoId != null ? (TempData[$"FormatosSelecionados_{competicaoId}"] as string ?? "{}") : "{}";
            ViewBag.NumFases = TempData[$"NumFases_{competicaoId}"] ?? 2;

            // Preserve TempData for the next request
            TempData.Keep("NomeCompeticao");
            TempData.Keep("TipoCompeticao");
            TempData.Keep("CompeticaoId");
            if (competicaoId != null)
            {
                TempData.Keep($"FormatosSelecionados_{competicaoId}");
                TempData.Keep($"NumFases_{competicaoId}");
            }

            return View();
        }

        [HttpPost]
        public IActionResult SalvarConfiguracao(int competicaoId, int numFases, int numJogosPorFase)
        {
            // Obter a competição existente
            var competicao = _context.Competicoes
                .Include(c => c.ConfiguracoesFase)
                .FirstOrDefault(c => c.Id == competicaoId);

            if (competicao == null)
            {
                return NotFound();
            }

            // Adicionar configurações de fase
            for (int i = 1; i <= numFases; i++)
            {
                var configuracao = new ConfiguracaoFase
                {
                    CompeticaoId = competicaoId,
                    FaseNumero = i,
                    NumJogosPorFase = numJogosPorFase,
                    Formato = "A definir",
                    PontosVitoria = competicao.PontosVitoria,
                    PontosEmpate = competicao.PontosEmpate,
                    PontosDerrota = 0
                };
                _context.ConfiguracoesFase.Add(configuracao);
            }

            // Salvar mudanças
            _context.SaveChanges();

            // Armazenar dados temporários para próxima view
            TempData["NomeCompeticao"] = competicao.Nome;
            TempData["TipoCompeticao"] = competicao.TipoCompeticao;
            TempData["CompeticaoId"] = competicao.Id;
            TempData["NumFases"] = numFases;
            TempData["NumJogosPorFase"] = numJogosPorFase;

            // Redirecionar para a próxima fase
            return RedirectToAction("Index", "ListadeJogadores");
        }

        [HttpPost]
        public IActionResult SalvarFormatos([FromBody] Dictionary<int, string> formatos, [FromQuery] string competicaoId)
        {
            if (!string.IsNullOrEmpty(competicaoId))
            {
                TempData[$"FormatosSelecionados_{competicaoId}"] = System.Text.Json.JsonSerializer.Serialize(formatos);
                TempData.Keep($"FormatosSelecionados_{competicaoId}");
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult SalvarNumFases([FromQuery] string competicaoId, [FromBody] int numFases)
        {
            if (!string.IsNullOrEmpty(competicaoId))
            {
                TempData[$"NumFases_{competicaoId}"] = numFases;
                TempData.Keep($"NumFases_{competicaoId}");
            }
            return Ok();
        }
    }
}
