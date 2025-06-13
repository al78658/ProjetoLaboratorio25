﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using static ProjetoLaboratorio25.Controllers.EmparelhamentoController;
using System.Diagnostics;

namespace ProjetoLaboratorio25.Controllers
{
    public class PontuacoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PontuacoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? competicaoId = null, string data = null)
        {
            // Se não for fornecido um ID de competição, buscar a mais recente
            if (!competicaoId.HasValue)
            {
                var ultimaCompeticao = await _context.Competicoes
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefaultAsync();

                if (ultimaCompeticao != null)
                {
                    competicaoId = ultimaCompeticao.Id;
                }
            }

            if (competicaoId.HasValue)
            {
                var competicao = await _context.Competicoes
                    .FirstOrDefaultAsync(c => c.Id == competicaoId);

                if (competicao != null)
                {
                    ViewBag.CompeticaoId = competicao.Id;
                    ViewBag.CompeticaoNome = competicao.Nome;
                }
            }

            // Se for fornecida uma data, passar para a view
            if (!string.IsNullOrEmpty(data))
            {
                ViewBag.DataSelecionada = data;
            }

            return View("Index2");
        }

        [HttpGet]
        public async Task<IActionResult> ObterJogosPorData(int competicaoId, string data)
        {
            try
            {
                // Tentar converter a data
                DateTime dataFiltro;
                if (string.IsNullOrEmpty(data))
                {
                    dataFiltro = DateTime.Today;
                }
                else
                {
                    try
                    {
                        dataFiltro = DateTime.Parse(data);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao converter data: {ex.Message}, usando data atual");
                        dataFiltro = DateTime.Today;
                    }
                }
                
                Console.WriteLine($"Data filtrada final: {dataFiltro.ToShortDateString()}");

                // Verificar se a competição existe
                var competicao = await _context.Competicoes.FindAsync(competicaoId);
                if (competicao == null)
                {
                    return NotFound(new { mensagem = $"Competição com ID {competicaoId} não encontrada." });
                }

                // Buscar todos os emparelhamentos da competição (para depuração)
                var todosEmparelhamentos = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId)
                    .ToListAsync();

                // Buscar os emparelhamentos da competição para a data especificada
                // Converter a data para o início e fim do dia para garantir que todos os jogos sejam encontrados
                var inicioDia = dataFiltro.Date;
                var fimDia = inicioDia.AddDays(1).AddTicks(-1);
                
                var emparelhamentos = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId && e.DataJogo >= inicioDia && e.DataJogo <= fimDia)
                    .Select(e => new
                    {
                        e.Id,
                        e.Clube1,
                        e.Clube2,
                        e.DataJogo,
                        Horario = e.HoraJogo.ToString(@"hh\:mm"),
                        e.PontuacaoClube1,
                        e.PontuacaoClube2,
                        e.JogoRealizado,
                        Motivo = e.Motivo ?? string.Empty, // Garantir que Motivo nunca seja null
                        Resultado = e.JogoRealizado 
                            ? $"{e.PontuacaoClube1} - {e.PontuacaoClube2}" 
                            : "Por definir"
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Data = dataFiltro.ToString("yyyy-MM-dd"),
                    DataFormatada = dataFiltro.ToString("dd/MM/yyyy"),
                    Emparelhamentos = emparelhamentos,
                    TotalEmparelhamentos = todosEmparelhamentos.Count,
                    CompeticaoNome = competicao.Nome
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter jogos: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
                
                return StatusCode(500, new { mensagem = $"Erro ao obter jogos: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarPontuacao([FromBody] AtualizarPontuacaoModel model)
        {
            try
            {
                if (model.EmparelhamentoId <= 0)
                {
                    return BadRequest(new { mensagem = "ID do emparelhamento inválido." });
                }

                Console.WriteLine($"Recebido pedido para atualizar pontuação: EmparelhamentoId={model.EmparelhamentoId}, Pontuação={model.PontuacaoClube1}-{model.PontuacaoClube2}, Motivo={model.Motivo}");
                
                var emparelhamento = await _context.EmparelhamentosFinal.FindAsync(model.EmparelhamentoId);
                
                if (emparelhamento == null)
                {
                    Console.WriteLine($"Emparelhamento não encontrado: ID={model.EmparelhamentoId}");
                    return NotFound(new { mensagem = "Emparelhamento não encontrado." });
                }
                
                Console.WriteLine($"Emparelhamento encontrado: {emparelhamento.Clube1} vs {emparelhamento.Clube2}, CompeticaoId={emparelhamento.CompeticaoId}");
                
                // Atualizar pontuações
                emparelhamento.PontuacaoClube1 = model.PontuacaoClube1;
                emparelhamento.PontuacaoClube2 = model.PontuacaoClube2;
                emparelhamento.JogoRealizado = true;
                
                // Garantir que as pontuações não sejam nulas
                if (!emparelhamento.PontuacaoClube1.HasValue)
                {
                    emparelhamento.PontuacaoClube1 = 0;
                }
                if (!emparelhamento.PontuacaoClube2.HasValue)
                {
                    emparelhamento.PontuacaoClube2 = 0;
                }
                
                // Atualizar motivo se fornecido
                if (!string.IsNullOrWhiteSpace(model.Motivo))
                {
                    emparelhamento.Motivo = model.Motivo;
                    Console.WriteLine($"Motivo fornecido: {model.Motivo}");
                }
                
                Console.WriteLine($"Salvando pontuações: {emparelhamento.PontuacaoClube1}-{emparelhamento.PontuacaoClube2}, JogoRealizado={emparelhamento.JogoRealizado}, Motivo={emparelhamento.Motivo}");
                
                await _context.SaveChangesAsync();
                
                // Determinar o clube vitorioso e o motivo
                string? clubeVitorioso = null;
                string motivo = model.Motivo ?? "";

                Console.WriteLine($"Verificando vitória - Pontuações: {model.PontuacaoClube1}-{model.PontuacaoClube2}, Motivo: {motivo}");

                // Se houver um motivo fornecido (botão de vitória clicado), usar isso para determinar o vencedor
                if (!string.IsNullOrWhiteSpace(model.Motivo))
                {
                    Console.WriteLine("Botão de vitória clicado, determinando vencedor com base no motivo");
                    if (motivo.Contains(emparelhamento.Clube1))
                    {
                        clubeVitorioso = emparelhamento.Clube1;
                    }
                    else if (motivo.Contains(emparelhamento.Clube2))
                    {
                        clubeVitorioso = emparelhamento.Clube2;
                    }
                }
                // Se não houver motivo (botão de vitória não clicado), verificar vitória por pontuação (11 pontos)
                else if (model.PontuacaoClube1 >= 11 && model.PontuacaoClube1 > model.PontuacaoClube2)
                {
                    clubeVitorioso = emparelhamento.Clube1;
                    motivo = $"{emparelhamento.Clube1} venceu por {model.PontuacaoClube1}-{model.PontuacaoClube2}";
                }
                else if (model.PontuacaoClube2 >= 11 && model.PontuacaoClube2 > model.PontuacaoClube1)
                {
                    clubeVitorioso = emparelhamento.Clube2;
                    motivo = $"{emparelhamento.Clube2} venceu por {model.PontuacaoClube1}-{model.PontuacaoClube2}";
                }

                Console.WriteLine($"Resultado da verificação - Clube Vitorioso: {clubeVitorioso}, Motivo Final: {motivo}");

                // Criar notificação se houver motivo ou vencedor
                if (!string.IsNullOrWhiteSpace(motivo) || clubeVitorioso != null)
                {
                    Console.WriteLine("Criando nova notificação");
                    
                    // Verificar se já existe uma notificação para este jogo hoje
                    var notificacaoExistente = await _context.Notificacoes
                        .Where(n => n.Clube1 == emparelhamento.Clube1 
                               && n.Clube2 == emparelhamento.Clube2
                               && n.DataNotificacao.Date == DateTime.Today
                               && n.Pontuacao1 == model.PontuacaoClube1
                               && n.Pontuacao2 == model.PontuacaoClube2)
                        .FirstOrDefaultAsync();

                    if (notificacaoExistente == null)
                    {
                        // Criar a notificação com timestamp atual apenas se não existir
                        var agora = DateTime.Now;
                        var notificacao = new Notificacao
                        {
                            Clube1 = emparelhamento.Clube1,
                            Clube2 = emparelhamento.Clube2,
                            ClubeVitorioso = clubeVitorioso,
                            Motivo = motivo,
                            DataNotificacao = agora,
                            Pontuacao1 = model.PontuacaoClube1,
                            Pontuacao2 = model.PontuacaoClube2
                        };

                        // Adicionar a nova notificação
                        await _context.Notificacoes.AddAsync(notificacao);
                        
                        // Forçar o contexto a salvar imediatamente
                        await _context.SaveChangesAsync();
                        
                        // Limpar o tracking do contexto para evitar problemas de cache
                        _context.ChangeTracker.Clear();
                        
                        Console.WriteLine($"Notificação criada com sucesso - ID: {notificacao.Id}, Data: {agora}");
                    }
                    else
                    {
                        Console.WriteLine($"Notificação já existe para este jogo hoje - ID: {notificacaoExistente.Id}");
                    }
                }
                
                return Ok(new { mensagem = "Pontuação atualizada com sucesso." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar pontuação: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
                
                return StatusCode(500, new { mensagem = $"Erro ao atualizar pontuação: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarDataHora([FromBody] AtualizarDataHoraModel model)
        {
            try
            {
                Console.WriteLine($"Recebido pedido para atualizar data/hora: EmparelhamentoId={model.EmparelhamentoId}, Data={model.DataJogo}, Hora={model.HoraJogo}");
                
                // Buscar o emparelhamento diretamente do banco de dados com tracking ativado
                var emparelhamento = await _context.EmparelhamentosFinal
                    .Where(e => e.Id == model.EmparelhamentoId)
                    .FirstOrDefaultAsync();
                
                if (emparelhamento == null)
                {
                    Console.WriteLine($"Emparelhamento não encontrado: ID={model.EmparelhamentoId}");
                    return NotFound(new { mensagem = "Emparelhamento não encontrado." });
                }
                
                Console.WriteLine($"Emparelhamento encontrado: {emparelhamento.Clube1} vs {emparelhamento.Clube2}");
                Console.WriteLine($"Valores atuais: Data={emparelhamento.DataJogo.ToShortDateString()}, Hora={emparelhamento.HoraJogo}");
                
                // Atualizar data
                if (DateTime.TryParse(model.DataJogo, out var dataParseada))
                {
                    // Garantir que apenas a data seja alterada, mantendo a hora original
                    emparelhamento.DataJogo = dataParseada;
                    Console.WriteLine($"Nova data definida: {dataParseada.ToShortDateString()}");
                }
                else
                {
                    Console.WriteLine($"Falha ao converter data: {model.DataJogo}");
                }
                
                // Atualizar hora
                if (TimeSpan.TryParse(model.HoraJogo, out var horaParseada))
                {
                    emparelhamento.HoraJogo = horaParseada;
                    Console.WriteLine($"Nova hora definida: {horaParseada}");
                }
                else
                {
                    Console.WriteLine($"Falha ao converter hora: {model.HoraJogo}. Tentando formato alternativo.");
                    
                    // Tentar formato alternativo (hh:mm)
                    try {
                        var parts = model.HoraJogo.Split(':');
                        if (parts.Length == 2)
                        {
                            int hours = int.Parse(parts[0]);
                            int minutes = int.Parse(parts[1]);
                            var novaHora = new TimeSpan(hours, minutes, 0);
                            emparelhamento.HoraJogo = novaHora;
                            Console.WriteLine($"Hora convertida com sucesso usando formato alternativo: {novaHora}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao converter hora no formato alternativo: {ex.Message}");
                    }
                }
                
                // Marcar explicitamente a entidade como modificada
                _context.Entry(emparelhamento).State = EntityState.Modified;
                
                // Executar SQL direto para garantir a atualização
                var sql = $"UPDATE EmparelhamentosFinal SET DataJogo = '{emparelhamento.DataJogo:yyyy-MM-dd}', HoraJogo = '{emparelhamento.HoraJogo}' WHERE Id = {emparelhamento.Id}";
                Console.WriteLine($"Executando SQL: {sql}");
                await _context.Database.ExecuteSqlRawAsync(sql);
                
                // Salvar as alterações usando o Entity Framework também
                Console.WriteLine("Salvando alterações no banco de dados via Entity Framework...");
                await _context.SaveChangesAsync();
                Console.WriteLine("Alterações salvas com sucesso!");
                
                // Limpar o cache do contexto
                _context.ChangeTracker.Clear();
                
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
                
                return Ok(new { 
                    mensagem = "Data e hora atualizadas com sucesso.",
                    emparelhamento = new {
                        id = emparelhamento.Id,
                        clube1 = emparelhamento.Clube1,
                        clube2 = emparelhamento.Clube2,
                        dataJogo = emparelhamento.DataJogo.ToString("yyyy-MM-dd"),
                        horaJogo = emparelhamento.HoraJogo.ToString(@"hh\:mm"),
                        competicaoId = emparelhamento.CompeticaoId
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar data e hora: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
                
                return StatusCode(500, new { mensagem = $"Erro ao atualizar data e hora: {ex.Message}" });
            }
        }
    }

    // As classes AtualizarPontuacaoModel e AtualizarDataHoraModel foram movidas para o EmparelhamentoController
}
