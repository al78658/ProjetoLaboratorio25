using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using Microsoft.EntityFrameworkCore;



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

            // Preserve TempData for the next request
            TempData.Keep("NomeCompeticao");
            TempData.Keep("TipoCompeticao");
            TempData.Keep("CompeticaoId");

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
    }
}
