﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjetoLaboratorio25.Controllers
{
    public class EmparelhamentoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmparelhamentoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? competicaoId = null, string? fase = null)
        {
            // Primeiro tenta obter o ID da competição da URL
            if (competicaoId == null)
            {
                // Se não estiver na URL, tenta obter do TempData
                competicaoId = TempData["CompeticaoId"] as int?;
                
                // Se ainda não tiver o ID, busca a competição mais recente
                if (competicaoId == null)
                {
                    var latestCompetition = await _context.Competicoes
                        .OrderByDescending(c => c.Id)
                        .FirstOrDefaultAsync();
                        
                    if (latestCompetition == null)
                    {
                        ViewBag.CompeticaoId = null;
                        ViewBag.ErrorMessage = "Nenhuma competição encontrada.";
                        return View();
                    }
                    
                    competicaoId = latestCompetition.Id;
                }
            }
            
            // Busca a competição pelo ID
            var competition = await _context.Competicoes
                .Include(c => c.ConfiguracoesFase)
                .FirstOrDefaultAsync(c => c.Id == competicaoId);

            if (competition == null)
            {
                ViewBag.CompeticaoId = null;
                ViewBag.ErrorMessage = "Competição não encontrada.";
                return View();
            }

            // Verificar se é uma competição do tipo taça
            var isTaca = competition.ConfiguracoesFase.Any(cf => cf.Formato == "eliminacao");
            
            // Se for taça e estivermos na próxima fase, buscar apenas os vencedores
            if (isTaca && fase == "proxima")
            {
                // Buscar todos os jogos realizados
                var jogosRealizados = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competition.Id && e.JogoRealizado)
                    .ToListAsync();

                // Obter os vencedores
                var vencedores = jogosRealizados
                    .Select(j => {
                        if (j.PontuacaoClube1 > j.PontuacaoClube2)
                            return j.Clube1;
                        else if (j.PontuacaoClube2 > j.PontuacaoClube1)
                            return j.Clube2;
                        return null;
                    })
                    .Where(v => v != null)
                    .ToList();

                // Ordenar vencedores por número de vitórias
                var vencedoresComVitorias = vencedores
                    .GroupBy(v => v)
                    .Select(g => new { Nome = g.Key, Vitorias = g.Count() })
                    .OrderByDescending(v => v.Vitorias)
                    .ToList();

                // Passar os vencedores para a view
                ViewBag.Vencedores = vencedoresComVitorias.Select(v => v.Nome).ToList();
                ViewBag.IsFaseEliminatoria = true;
            }
            else
            {
                // Verificar se temos jogadores no TempData
                var jogadoresJson = TempData["JogadoresParaEmparelhar"] as string;
                if (!string.IsNullOrEmpty(jogadoresJson))
                {
                    // Deserializar os jogadores
                    var options = new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles
                    };
                    var jogadores = JsonSerializer.Deserialize<List<dynamic>>(jogadoresJson, options);
                    
                    // Se for competição por equipes, agrupar jogadores por clube
                    if (competition.TipoCompeticao == "equipas")
                    {
                        var clubes = jogadores
                            .Select(j => (string)j.GetProperty("Clube").GetString())
                            .Where(c => !string.IsNullOrEmpty(c))
                            .Distinct()
                            .ToList();
                            
                        ViewBag.Clubes = clubes;
                    }
                    else
                    {
                        // Para competição individual, passar os jogadores diretamente
                        ViewBag.Jogadores = jogadores.Select(j => new { nome = j.GetProperty("Nome").GetString() }).ToList();
                    }
                }
                else
                {
                    // Se não tiver jogadores no TempData, buscar do banco de dados
                    var jogadores = await _context.Jogadores
                        .Where(j => j.CompeticaoId == competition.Id)
                        .ToListAsync();
                        
                    if (competition.TipoCompeticao == "equipas")
                    {
                        var clubes = jogadores
                            .Where(j => !string.IsNullOrEmpty(j.Clube))
                            .Select(j => j.Clube)
                            .Distinct()
                            .ToList();
                            
                        ViewBag.Clubes = clubes;
                    }
                    else
                    {
                        ViewBag.Jogadores = jogadores.Select(j => new { nome = j.Nome }).ToList();
                    }
                }
                
                ViewBag.IsFaseEliminatoria = false;
            }

            ViewBag.CompeticaoId = competition.Id;
            ViewBag.CompeticaoNome = competition.Nome;
            ViewBag.TipoCompeticao = competition.TipoCompeticao;
            
            // Passar o ID da competição para o JavaScript
            ViewBag.CompeticaoIdJS = competition.Id;
            
            // Armazenar no TempData para persistência
            TempData["CompeticaoId"] = competition.Id;
            TempData["NomeCompeticao"] = competition.Nome;

            return View();
        }
        
        [HttpGet]
        public async Task<IActionResult> VerificarEmparelhamentos(int competicaoId)
        {
            try
            {
                var emparelhamentos = await _context.EmparelhamentosFinal
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
        
        [HttpGet]
        public async Task<IActionResult> ObterEmparelhamentos(int competicaoId)
        {
            try
            {
                var emparelhamentos = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId)
                    .Select(e => new
                    {
                        e.Id,
                        e.Clube1,
                        e.Clube2,
                        DataJogo = e.DataJogo.ToString("yyyy-MM-dd"),
                        HoraJogo = e.HoraJogo.ToString(@"hh\:mm"),
                        e.PontuacaoClube1,
                        e.PontuacaoClube2,
                        e.JogoRealizado
                    })
                    .ToListAsync();
                    
                return Ok(emparelhamentos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao obter emparelhamentos: {ex.Message}" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> AtualizarDataHora([FromBody] AtualizarDataHoraModel model)
        {
            try
            {
                var emparelhamento = await _context.EmparelhamentosFinal.FindAsync(model.EmparelhamentoId);
                
                if (emparelhamento == null)
                {
                    return NotFound(new { mensagem = "Emparelhamento não encontrado." });
                }
                
                // Atualizar data e hora
                if (DateTime.TryParse(model.DataJogo, out var novaData))
                {
                    emparelhamento.DataJogo = novaData;
                }
                
                if (TimeSpan.TryParse(model.HoraJogo, out var novaHora))
                {
                    emparelhamento.HoraJogo = novaHora;
                }
                
                await _context.SaveChangesAsync();
                
                // Criar notificação de alteração de data/hora
                var notificacao = new Notificacao
                {
                    Clube1 = emparelhamento.Clube1,
                    Clube2 = emparelhamento.Clube2,
                    Motivo = "Alteração de data/hora do jogo",
                    DataNotificacao = DateTime.Now,
                };
                
                _context.Notificacoes.Add(notificacao);
                await _context.SaveChangesAsync();
                
                return Ok(new { mensagem = "Data e hora atualizadas com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao atualizar data e hora: {ex.Message}" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> AtualizarPontuacao([FromBody] AtualizarPontuacaoModel model)
        {
            try
            {
                var emparelhamento = await _context.EmparelhamentosFinal.FindAsync(model.EmparelhamentoId);
                
                if (emparelhamento == null)
                {
                    return NotFound(new { mensagem = "Emparelhamento não encontrado." });
                }
                
                // Atualizar pontuações
                emparelhamento.PontuacaoClube1 = model.PontuacaoClube1;
                emparelhamento.PontuacaoClube2 = model.PontuacaoClube2;
                emparelhamento.JogoRealizado = true;
                
                await _context.SaveChangesAsync();
                
                // Determinar o clube vitorioso
                string? clubeVitorioso = null;
                if (model.PontuacaoClube1 > model.PontuacaoClube2)
                {
                    clubeVitorioso = emparelhamento.Clube1;
                }
                else if (model.PontuacaoClube2 > model.PontuacaoClube1)
                {
                    clubeVitorioso = emparelhamento.Clube2;
                }
                
                // Criar notificação de resultado
                var notificacao = new Notificacao
                {
                    Clube1 = emparelhamento.Clube1,
                    Clube2 = emparelhamento.Clube2,
                    ClubeVitorioso = clubeVitorioso,
                    Motivo = "Resultado do jogo registrado",
                    DataNotificacao = DateTime.Now,
                };
                
                _context.Notificacoes.Add(notificacao);
                await _context.SaveChangesAsync();
                
                return Ok(new { mensagem = "Pontuação atualizada com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao atualizar pontuação: {ex.Message}" });
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

                // Remover emparelhamentos existentes para a competição (EmparelhamentoFinal)
                var emparelhamentosExistentes = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId)
                    .ToListAsync();
                
                if (emparelhamentosExistentes.Any())
                {
                    _context.EmparelhamentosFinal.RemoveRange(emparelhamentosExistentes);
                    await _context.SaveChangesAsync();
                }

                var falhas = new List<EmparelhamentoViewModel>();
                var sucessos = new List<string>();

                // Adicionar novos emparelhamentos
                foreach (var e in request.Emparelhamentos)
                {
                    try
                    {
                        // Criar novo emparelhamento independente do tipo
                        var novoEmparelhamento = new EmparelhamentoFinal
                        {
                            CompeticaoId = competicaoId,
                            Clube1 = e.Clube1 ?? "",
                            Clube2 = e.Clube2 ?? "",
                            DataJogo = DateTime.TryParse(e.DataJogo, out var data) ? data : DateTime.Now,
                            HoraJogo = TimeSpan.TryParse(e.HoraJogo, out var hora) ? hora : TimeSpan.Zero,
                            PontuacaoClube1 = e.PontuacaoClube1,
                            PontuacaoClube2 = e.PontuacaoClube2,
                            JogoRealizado = e.JogoRealizado
                        };

                        _context.EmparelhamentosFinal.Add(novoEmparelhamento);
                        sucessos.Add($"Emparelhamento entre {e.Clube1} e {e.Clube2} salvo com sucesso.");
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
        public int? PontuacaoClube1 { get; set; }
        public int? PontuacaoClube2 { get; set; }
        public bool JogoRealizado { get; set; }
    }

    public class EmparelhamentoRequestModel
    {
        public int CompeticaoId { get; set; }
        public string? NomeCompeticao { get; set; }
        public List<EmparelhamentoViewModel> Emparelhamentos { get; set; } = new List<EmparelhamentoViewModel>();
    }
    
    public class AtualizarDataHoraModel
    {
        public int EmparelhamentoId { get; set; }
        public string DataJogo { get; set; } = string.Empty;
        public string HoraJogo { get; set; } = string.Empty;
    }
    
    public class AtualizarPontuacaoModel
    {
        public int EmparelhamentoId { get; set; }
        public int PontuacaoClube1 { get; set; }
        public int PontuacaoClube2 { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }
}
