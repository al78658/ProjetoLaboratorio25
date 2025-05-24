﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using System;
using System.Collections.Generic;
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
                return View();
            }

            // Fetch clubs associated with the latest competition
            var clubes = await _context.Jogadores
                .Where(j => j.CompeticaoId == latestCompetition.Id && !string.IsNullOrEmpty(j.Clube))
                .Select(j => j.Clube != null ? j.Clube.Trim().ToLower() : "")
                .Distinct()
                .ToListAsync();

            ViewBag.CompeticaoId = latestCompetition.Id;
            ViewBag.CompeticaoNome = latestCompetition.Nome;
            ViewBag.Clubes = clubes;
            
            // Passar o ID da competição para o JavaScript
            ViewBag.CompeticaoIdJS = latestCompetition.Id;

            return View();
        }
        
        [HttpGet]
        public async Task<IActionResult> VerificarEmparelhamentos(int competicaoId)
        {
            try
            {
                var emparelhamentos = await _context.EmparelhamentosBase
                    .Where(e => e.CompeticaoId == competicaoId)
                    .ToListAsync();
                    
                return Ok(new { 
                    emparelhamentosBase = emparelhamentos.Count,
                    mensagem = "Verificação concluída com sucesso."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao verificar emparelhamentos: {ex.Message}" });
            }
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> SalvarEmparelhamentos([FromBody] EmparelhamentoRequestModel request)
        {
            // Log para depuração
            Console.WriteLine($"Recebido request: CompeticaoId={request?.CompeticaoId}, NomeCompeticao={request?.NomeCompeticao}");
            Console.WriteLine($"Emparelhamentos recebidos: {request?.Emparelhamentos?.Count ?? 0}");
            
            // Log detalhado dos emparelhamentos
            if (request?.Emparelhamentos != null)
            {
                foreach (var emp in request.Emparelhamentos)
                {
                    Console.WriteLine($"Emparelhamento: Clube1={emp.Clube1}, Clube2={emp.Clube2}, Data={emp.DataJogo}, Hora={emp.HoraJogo}, IsClube={emp.IsClube}");
                }
            }
            
            if (request == null || request.Emparelhamentos == null || !request.Emparelhamentos.Any())
            {
                return BadRequest(new { mensagem = "Nenhum emparelhamento fornecido." });
            }

            try
            {
                // Buscar a competição mais recente se não for especificada
                Competicao competicao = null;
                
                // Primeiro tenta pelo nome da competição
                if (!string.IsNullOrEmpty(request.NomeCompeticao))
                {
                    competicao = await _context.Competicoes
                        .FirstOrDefaultAsync(c => c.Nome == request.NomeCompeticao);
                }
                
                // Se não encontrar pelo nome ou não tiver nome, tenta pelo ID
                if (competicao == null && request.CompeticaoId > 0)
                {
                    competicao = await _context.Competicoes.FindAsync(request.CompeticaoId);
                }
                
                // Se ainda não encontrou, pega a mais recente
                if (competicao == null)
                {
                    competicao = await _context.Competicoes
                        .OrderByDescending(c => c.Id)
                        .FirstOrDefaultAsync();
                }

                if (competicao == null)
                {
                    return NotFound(new { mensagem = "Nenhuma competição encontrada." });
                }

                int competicaoId = competicao.Id;
                string nomeCompeticao = competicao.Nome ?? "";

                // Remover emparelhamentos existentes para a competição (EmparelhamentoBase)
                var emparelhamentosBaseExistentes = await _context.EmparelhamentosBase
                    .Where(e => e.CompeticaoId == competicaoId)
                    .ToListAsync();
                
                if (emparelhamentosBaseExistentes.Any())
                {
                    _context.EmparelhamentosBase.RemoveRange(emparelhamentosBaseExistentes);
                    await _context.SaveChangesAsync();
                }

                // Removido: Não precisamos mais remover JogosEmparelhados

                var falhas = new List<EmparelhamentoViewModel>();
                var sucessos = new List<string>();

                // Adicionar novos emparelhamentos
                foreach (var e in request.Emparelhamentos)
                {
                    try
                    {
                        // Verificar se é emparelhamento de clubes ou jogadores
                        if (e.IsClube)
                        {
                            // Salvar como EmparelhamentoBase
                            var novoEmparelhamento = new EmparelhamentoBase
                            {
                                CompeticaoId = competicaoId,
                                Clube1 = e.Clube1 ?? "",
                                Clube2 = e.Clube2 ?? "",
                                DataJogo = DateTime.TryParse(e.DataJogo, out var data) ? data : DateTime.Now,
                                HoraJogo = TimeSpan.TryParse(e.HoraJogo, out var hora) ? hora : TimeSpan.Zero
                            };

                            _context.EmparelhamentosBase.Add(novoEmparelhamento);
                            sucessos.Add($"Emparelhamento entre {e.Clube1} e {e.Clube2} salvo com sucesso.");
                        }
                        else
                        {
                            // Buscar os jogadores pelo nome
                            var jogador1 = await _context.Jogadores
                                .FirstOrDefaultAsync(j => j.Nome == e.Jogador1Nome && j.CompeticaoId == competicaoId);
                            
                            var jogador2 = await _context.Jogadores
                                .FirstOrDefaultAsync(j => j.Nome == e.Jogador2Nome && j.CompeticaoId == competicaoId);

                            if (jogador1 != null && jogador2 != null)
                            {
                                // Agora usamos EmparelhamentoBase para jogadores também
                                var novoEmparelhamento = new EmparelhamentoBase
                                {
                                    CompeticaoId = competicaoId,
                                    Clube1 = jogador1.Nome ?? "",
                                    Clube2 = jogador2.Nome ?? "",
                                    DataJogo = DateTime.TryParse(e.DataJogo, out var data) ? data : DateTime.Now,
                                    HoraJogo = TimeSpan.TryParse(e.HoraJogo, out var hora) ? hora : TimeSpan.Zero
                                };

                                _context.EmparelhamentosBase.Add(novoEmparelhamento);
                                sucessos.Add($"Emparelhamento entre {e.Jogador1Nome} e {e.Jogador2Nome} salvo com sucesso.");
                            }
                            else
                            {
                                falhas.Add(e);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar emparelhamento: {ex.Message}");
                        falhas.Add(e);
                    }
                }

                await _context.SaveChangesAsync();

                if (falhas.Any())
                {
                    return StatusCode(207, new { 
                        mensagem = "Alguns emparelhamentos foram salvos com sucesso, mas outros falharam.", 
                        sucessos = sucessos,
                        falhas = falhas 
                    });
                }

                return Ok(new { 
                    mensagem = "Emparelhamentos salvos com sucesso.", 
                    competicaoId = competicaoId,
                    nomeCompeticao = nomeCompeticao,
                    sucessos = sucessos
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar emparelhamentos: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
                
                return StatusCode(500, new { mensagem = $"Erro ao salvar emparelhamentos: {ex.Message}" });
            }
        }
    }

    public class EmparelhamentoViewModel
    {
        public string? Clube1 { get; set; }
        public string? Clube2 { get; set; }
        public string? Jogador1Nome { get; set; }
        public string? Jogador2Nome { get; set; }
        public string? DataJogo { get; set; }
        public string? HoraJogo { get; set; }
        public bool IsClube { get; set; }
    }

    public class EmparelhamentoRequestModel
    {
        public int CompeticaoId { get; set; }
        public string? NomeCompeticao { get; set; }
        public List<EmparelhamentoViewModel> Emparelhamentos { get; set; } = new List<EmparelhamentoViewModel>();
    }
}
