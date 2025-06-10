using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjetoLaboratorio25.Controllers
{
    public class AtribuirCompeticao : Controller
    {
        private readonly ApplicationDbContext _context;

        public AtribuirCompeticao(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int competicaoId)
        {
            // Carregar todos os utilizadores exceto os que têm @admin no email
            ViewBag.Organizadores = await _context.Utilizadores
                .Where(u => !u.Email.Contains("@admin"))
                .ToListAsync();

            var competicao = await _context.Competicoes
                .FirstOrDefaultAsync(c => c.Id == competicaoId);
                
            if (competicao == null)
            {
                TempData["Erro"] = "Competição não encontrada.";
                return RedirectToAction("Index", "Menu");
            }
            
            ViewBag.CompeticaoId = competicaoId;
            ViewBag.NomeCompeticao = competicao.Nome;
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Guardar(int organizadorId, int competicaoId)
        {
            var competicao = await _context.Competicoes
                .FirstOrDefaultAsync(c => c.Id == competicaoId);

            if (competicao == null)
            {
                TempData["Erro"] = "Competição não encontrada.";
                return RedirectToAction("Index", "Menu");
            }

            if (organizadorId <= 0)
            {
                TempData["Erro"] = "Por favor, selecione um organizador válido.";
                return RedirectToAction(nameof(Index), new { competicaoId });
            }

            var organizador = await _context.Utilizadores
                .Where(u => !u.Email.Contains("@admin"))
                .FirstOrDefaultAsync(u => u.Id == organizadorId);

            if (organizador == null)
            {
                TempData["Erro"] = "Organizador não encontrado ou não pode ser selecionado.";
                return RedirectToAction(nameof(Index), new { competicaoId });
            }

            competicao.OrganizadorId = organizadorId;
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Competição atribuída com sucesso ao organizador " + organizador.UtilizadorNome;
            return RedirectToAction("Index", "Menu", new { competicaoId = competicao.Id });
        }
    }
}
