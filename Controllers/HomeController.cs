using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjetoLaboratorio25.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace ProjetoLaboratorio25.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        public async Task<IActionResult> Logout()
        {
            // Limpar todos os cookies de autenticação
            await HttpContext.SignOutAsync("CookieAuth");

            // Limpar a sessão
            HttpContext.Session.Clear();

            // Redirecionar para a página Home
            return RedirectToAction("Index", "Home");
        }
        
        [HttpGet]
        public IActionResult PesquisarJogadores(string termo)
        {
            // Esta funcionalidade agora é tratada pelo JavaScript no cliente
            return Json(new List<object>());
        }
        
        [HttpGet]
        public async Task<IActionResult> ObterNotificacoes()
        {
            try
            {
                // Obter as 10 notificações mais recentes
                var notificacoes = await _context.Notificacoes
                    .OrderByDescending(n => n.DataNotificacao)
                    .Take(10)
                    .Select(n => new
                    {
                        n.Id,
                        n.Clube1,
                        n.Clube2,
                        n.ClubeVitorioso,
                        n.Motivo,
                        DataNotificacao = n.DataNotificacao.ToString("dd/MM/yyyy HH:mm"),
                    })
                    .ToListAsync();
                
                return Ok(notificacoes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao obter notificações: {ex.Message}" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> MarcarNotificacaoComoLida(int notificacaoId)
        {
            try
            {
                var notificacao = await _context.Notificacoes.FindAsync(notificacaoId);
                
                if (notificacao == null)
                {
                    return NotFound(new { mensagem = "Notificação não encontrada." });
                }
                
                await _context.SaveChangesAsync();
                
                return Ok(new { mensagem = "Notificação marcada como lida." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao marcar notificação como lida: {ex.Message}" });
            }
        }
    }
}
