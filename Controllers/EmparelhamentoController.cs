﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoLaboratorio25.Controllers
{
    public class EmparelhamentoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmparelhamentoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch the latest competition ID
            var latestCompetition = await _context.Competicoes
                .OrderByDescending(c => c.Id)
                .FirstOrDefaultAsync();

            if (latestCompetition == null)
            {
                ViewBag.CompeticaoId = null;
                ViewBag.ErrorMessage = "Nenhuma competição encontrada.";
                return View(new List<string>());
            }

            // Fetch clubs associated with the latest competition
            var clubes = await _context.Jogadores
                .Where(j => j.CompeticaoId == latestCompetition.Id && !string.IsNullOrEmpty(j.Clube))
                .Select(j => j.Clube != null ? j.Clube.Trim().ToLower() : "")
                .Distinct()
                .ToListAsync();

            ViewBag.CompeticaoId = latestCompetition.Id;
            ViewBag.Clubes = clubes;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SalvarEmparelhamentos(int competicaoId, [FromBody] List<EmparelhamentoEquipaViewModel> emparelhamentos)
        {
            var falhas = new List<EmparelhamentoEquipaViewModel>();

            try
            {
                if (emparelhamentos == null || !emparelhamentos.Any())
                {
                    return BadRequest("Nenhum emparelhamento fornecido.");
                }

                // Buscar a competição e validar
                var competicao = await _context.Competicoes.FindAsync(competicaoId);
                if (competicao == null)
                {
                    return NotFound("Competição não encontrada.");
                }

                string nomeCompeticao = competicao.Nome ?? "";

                // Atualizar lógica para usar NomeCompeticao em vez de CompeticaoId
                var competicaoExistente = await _context.Competicoes.FirstOrDefaultAsync(c => c.Nome == nomeCompeticao);
                if (competicaoExistente == null)
                {
                    return NotFound("Competição não encontrada.");
                }

                // Remover emparelhamentos existentes para a competição
                var emparelhamentosExistentes = _context.EmparelhamentosBase
                    .Where(e => e.NomeCompeticao == nomeCompeticao);
                _context.EmparelhamentosBase.RemoveRange(emparelhamentosExistentes);
                await _context.SaveChangesAsync();

                // Buscar a última configuração de fase associada à competição
                var ultimaConfiguracaoFase = await _context.ConfiguracoesFase
                    .Where(cf => cf.Competicao.Nome == nomeCompeticao)
                    .OrderByDescending(cf => cf.FaseNumero)
                    .FirstOrDefaultAsync();

                if (ultimaConfiguracaoFase == null)
                {
                    return NotFound("Nenhuma configuração de fase encontrada para a competição.");
                }

                // Usar o CompeticaoId da última configuração de fase
                competicaoId = ultimaConfiguracaoFase.CompeticaoId;

                // Adicionar novos emparelhamentos
                foreach (var e in emparelhamentos)
                {
                    try
                    {
                        // Atualizar lógica para salvar em EmparelhamentoBase
                        var novoEmparelhamento = new EmparelhamentoBase
                        {
                            CompeticaoId = competicaoId,
                            Clube1 = e.Clube1 ?? "", // Clube da esquerda na tabela
                            Clube2 = e.Clube2 ?? "", // Clube da direita na tabela
                            DataJogo = DateTime.TryParse(e.DataJogo, out var data) ? data : DateTime.Now,
                            HoraJogo = TimeSpan.TryParse(e.HoraJogo, out var hora) ? hora : TimeSpan.Zero
                        };

                        await _context.Set<EmparelhamentoBase>().AddAsync(novoEmparelhamento);
                    }
                    catch
                    {
                        falhas.Add(e);
                    }
                }

                await _context.SaveChangesAsync();

                if (falhas.Any())
                {
                    return StatusCode(207, new { mensagem = "Alguns emparelhamentos falharam.", falhas });
                }

                return Ok(new { mensagem = "Emparelhamentos salvos com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao salvar emparelhamentos: {ex.Message}", falhas });
            }
        }
    }

    public class EmparelhamentoEquipaViewModel
    {
        public string? Clube1 { get; set; }
        public string? Clube2 { get; set; }
        public string? DataJogo { get; set; }
        public string? HoraJogo { get; set; }
    }
}
