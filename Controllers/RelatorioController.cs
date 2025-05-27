using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace ProjetoLaboratorio25.Controllers
{
    public class RelatorioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RelatorioController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SalvarRelatorio([FromBody] Report report)
        {
            try
            {
                report.DataCriacao = DateTime.Now;
                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Erro ao salvar o relatório: " + ex.Message + " — " + ex.InnerException?.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConsultarRelatorio(int? id, string categoria = null, string codigo = null)
        {
            if (string.IsNullOrEmpty(categoria))
                return View((Report)null);

            // Guarda os parâmetros reais que vieram na URL
            if (Request.Query.ContainsKey("codigo"))
                codigo = Request.Query["codigo"].ToString();

            if (Request.Query.ContainsKey("categoria"))
                categoria = Request.Query["categoria"].ToString();

            // Se mesmo assim não houver código, termina
            if (string.IsNullOrEmpty(codigo))
                return View((Report)null);

            ViewBag.Categoria = categoria;
            ViewBag.Codigo = codigo;

            var reports = _context.Reports.Where(r => r.Categoria == categoria && r.Codigo == codigo);

            Report report = id.HasValue
                ? await reports.FirstOrDefaultAsync(r => r.Id == id.Value)
                : await reports.OrderByDescending(r => r.DataCriacao).FirstOrDefaultAsync();

            if (report == null)
                return View((Report)null);

            var relatorios = await reports.OrderBy(r => r.DataCriacao).ToListAsync();
            var indexAtual = relatorios.FindIndex(r => r.Id == report.Id);
            ViewBag.AnteriorId = indexAtual > 0 ? relatorios[indexAtual - 1].Id : (int?)null;
            ViewBag.ProximoId = indexAtual < relatorios.Count - 1 ? relatorios[indexAtual + 1].Id : (int?)null;

            return View(report);
        }

        [HttpGet]
        public async Task<IActionResult> ConsultarRelatorios(string categoria = null, string codigo = null)
        {
            var reports = _context.Reports.AsQueryable();

            if (!string.IsNullOrEmpty(categoria))
                reports = reports.Where(r => r.Categoria == categoria);

            if (!string.IsNullOrEmpty(codigo))
                reports = reports.Where(r => r.Codigo == codigo);

            var result = await reports.ToListAsync();
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> ObterDetalhes(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
                return Json(new { success = false, message = "Relatório não encontrado" });

            return Json(report);
        }
    }
}
