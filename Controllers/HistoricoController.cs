using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;
using System;
using System.Threading.Tasks;

namespace ProjetoLaboratorio25.Controllers
{
    public class HistoricoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HistoricoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? competicaoId, string searchTerm = null, DateTime? dataInicio = null, DateTime? dataFim = null)
        {
            if (!competicaoId.HasValue)
            {
                return RedirectToAction("Index", "Menu");
            }

            // Buscar o nome da competição
            var competicao = await _context.Competicoes
                .FirstOrDefaultAsync(c => c.Id == competicaoId.Value);

            if (competicao == null)
            {
                return RedirectToAction("Index", "Menu");
            }

            ViewBag.CompeticaoNome = competicao.Nome;
            ViewBag.CompeticaoId = competicaoId.Value;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.TipoCompeticao = competicao.TipoCompeticao;
            ViewBag.DataInicio = dataInicio?.ToString("yyyy-MM-dd");
            ViewBag.DataFim = dataFim?.ToString("yyyy-MM-dd");

            // Buscar todas as partidas realizadas da competição específica
            var query = _context.EmparelhamentosFinal
                .Where(e => e.CompeticaoId == competicaoId.Value && e.JogoRealizado);

            // Aplicar filtro de pesquisa se fornecido
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(e => 
                    e.Clube1.ToLower().Contains(searchTerm) || 
                    e.Clube2.ToLower().Contains(searchTerm) ||
                    (e.Motivo != null && e.Motivo.ToLower().Contains(searchTerm))
                );
            }

            // Aplicar filtro de data se fornecido
            if (dataInicio.HasValue)
            {
                query = query.Where(e => e.DataJogo >= dataInicio.Value);
            }
            if (dataFim.HasValue)
            {
                query = query.Where(e => e.DataJogo <= dataFim.Value);
            }

            var partidas = await query
                .OrderByDescending(e => e.DataJogo)
                .ThenByDescending(e => e.HoraJogo)
                .Select(e => new
                {
                    Partidas = $"{e.Clube1} vs {e.Clube2}",
                    DataHora = $"{e.DataJogo:dd/MM/yyyy} - {e.HoraJogo:hh\\:mm}",
                    Resultado = $"{e.PontuacaoClube1} - {e.PontuacaoClube2}",
                    e.Motivo
                })
                .ToListAsync();

            return View(partidas);
        }
    }
}
