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

        public async Task<IActionResult> Index()
        {
            // Carregar todos os utilizadores exceto os que têm @admin no email
            ViewBag.Organizadores = await _context.Utilizadores
                .Where(u => !u.Email.Contains("@admin"))
                .ToListAsync();
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Guardar(int organizadorId)
        {
            if (organizadorId <= 0)
            {
                TempData["Erro"] = "Por favor, selecione um organizador válido.";
                return RedirectToAction(nameof(Index));
            }

            var organizador = await _context.Utilizadores
                .Where(u => !u.Email.Contains("@admin"))
                .FirstOrDefaultAsync(u => u.Id == organizadorId);

            if (organizador == null)
            {
                TempData["Erro"] = "Organizador não encontrado ou não pode ser selecionado.";
                return RedirectToAction(nameof(Index));
            }

            // Aqui você pode adicionar a lógica para atribuir a competição ao organizador
            // Por exemplo, criar um registro na tabela de atribuições ou atualizar a competição

            TempData["Sucesso"] = "Competição atribuída com sucesso ao organizador " + organizador.UtilizadorNome;
            return RedirectToAction("Index", "Menu");
        }
    }
}
