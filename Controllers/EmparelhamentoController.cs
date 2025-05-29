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
            
            // Buscar todos os jogos anteriores
            var jogosAnteriores = await _context.EmparelhamentosFinal
                .Where(e => e.CompeticaoId == competition.Id)
                .OrderBy(e => e.DataJogo)
                .ThenBy(e => e.HoraJogo)
                .Select(e => new
                {
                    e.Clube1,
                    e.Clube2,
                    DataJogo = e.DataJogo.ToString("dd/MM/yyyy"),
                    HoraJogo = e.HoraJogo.ToString(@"hh\:mm"),
                    e.PontuacaoClube1,
                    e.PontuacaoClube2,
                    e.JogoRealizado,
                    e.Motivo
                })
                .ToListAsync();
            
            ViewBag.JogosAnteriores = jogosAnteriores;
            
            // Se for taça e estivermos na próxima fase, buscar apenas os vencedores
            if (isTaca && fase == "proxima")
            {
                // Buscar todos os jogos realizados
                var jogosRealizados = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competition.Id && e.JogoRealizado)
                    .OrderBy(e => e.DataJogo)
                    .ThenBy(e => e.HoraJogo)
                    .ToListAsync();

                // Contar vitórias para cada clube
                var vitoriasDict = new Dictionary<string, int>();
                foreach (var jogo in jogosRealizados)
                {
                    if (jogo.PontuacaoClube1 > jogo.PontuacaoClube2)
                    {
                        if (!vitoriasDict.ContainsKey(jogo.Clube1))
                            vitoriasDict[jogo.Clube1] = 0;
                        vitoriasDict[jogo.Clube1]++;
                    }
                    else if (jogo.PontuacaoClube2 > jogo.PontuacaoClube1)
                    {
                        if (!vitoriasDict.ContainsKey(jogo.Clube2))
                            vitoriasDict[jogo.Clube2] = 0;
                        vitoriasDict[jogo.Clube2]++;
                    }
                }

                // Encontrar o maior número de vitórias
                var maxVitorias = vitoriasDict.Any() ? vitoriasDict.Max(x => x.Value) : 0;

                // Pegar apenas os clubes com o maior número de vitórias
                var clubesComMaisVitorias = vitoriasDict
                    .Where(x => x.Value == maxVitorias)
                    .Select(x => x.Key)
                    .ToList();

                // Se tivermos apenas um clube com o máximo de vitórias, ele é o vencedor absoluto
                if (clubesComMaisVitorias.Count == 1)
                {
                    ViewBag.VencedorAbsoluto = clubesComMaisVitorias[0];
                    ViewBag.Vencedores = new List<string>();
                }
                else
                {
                    // Se tivermos um número ímpar de clubes com o máximo de vitórias,
                    // o primeiro clube recebe um "bye"
                    if (clubesComMaisVitorias.Count % 2 != 0)
                    {
                        var clubeComBye = clubesComMaisVitorias[0];
                        clubesComMaisVitorias.RemoveAt(0);
                        clubesComMaisVitorias.Add($"{clubeComBye} (bye)");
                    }

                    ViewBag.Vencedores = clubesComMaisVitorias;
                }

                ViewBag.IsFaseEliminatoria = true;
                ViewBag.NumeroVitorias = vitoriasDict;
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
            
            if (request == null || request.Emparelhamentos == null || !request.Emparelhamentos.Any())
            {
                return BadRequest(new { mensagem = "Nenhum emparelhamento fornecido." });
            }

            try
            {
                // Buscar a competição
                Competicao competicao = null;
                
                if (!string.IsNullOrEmpty(request.NomeCompeticao))
                {
                    competicao = await _context.Competicoes
                        .FirstOrDefaultAsync(c => c.Nome == request.NomeCompeticao);
                }
                
                if (competicao == null && request.CompeticaoId > 0)
                {
                    competicao = await _context.Competicoes.FindAsync(request.CompeticaoId);
                }
                
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

                // Verificar se é um formato Round-Robin
                var configuracaoFase = await _context.ConfiguracoesFase
                    .Where(cf => cf.CompeticaoId == competicaoId)
                    .OrderByDescending(cf => cf.FaseNumero)
                    .FirstOrDefaultAsync();

                bool isRoundRobin = configuracaoFase?.Formato?.ToLower() == "round-robin";

                if (isRoundRobin)
                {
                    // Se estamos na página inicial de emparelhamento, permitir o emparelhamento normal
                    if (request.Emparelhamentos != null && request.Emparelhamentos.Any())
                    {
                        // Verificar se é uma solicitação para gerar novos emparelhamentos
                        var todosJogosRealizados = request.Emparelhamentos.All(e => 
                            e.JogoRealizado && 
                            e.PontuacaoClube1.HasValue && 
                            e.PontuacaoClube2.HasValue);

                        if (todosJogosRealizados)
                        {
                            // Obter todos os participantes únicos dos jogos existentes
                            var participantes = new HashSet<string>();
                            foreach (var jogo in request.Emparelhamentos)
                            {
                                participantes.Add(jogo.Clube1);
                                participantes.Add(jogo.Clube2);
                            }

                            // Verificar se já existem todos os emparelhamentos possíveis
                            var totalParticipantes = participantes.Count;
                            var totalJogosPossiveis = (totalParticipantes * (totalParticipantes - 1)) / 2;

                            if (request.Emparelhamentos.Count >= totalJogosPossiveis)
                            {
                                return BadRequest(new { mensagem = "Todos os jogos do Round-Robin já foram realizados e registrados." });
                            }

                            // Criar matriz de jogos já realizados
                            var jogosJaRealizados = new Dictionary<string, HashSet<string>>();
                            foreach (var jogo in request.Emparelhamentos)
                            {
                                if (!jogosJaRealizados.ContainsKey(jogo.Clube1))
                                    jogosJaRealizados[jogo.Clube1] = new HashSet<string>();
                                if (!jogosJaRealizados.ContainsKey(jogo.Clube2))
                                    jogosJaRealizados[jogo.Clube2] = new HashSet<string>();

                                jogosJaRealizados[jogo.Clube1].Add(jogo.Clube2);
                                jogosJaRealizados[jogo.Clube2].Add(jogo.Clube1);
                            }

                            // Gerar novos emparelhamentos
                            var novosEmparelhamentos = new List<EmparelhamentoFinal>();
                            var participantesList = participantes.ToList();

                            for (int i = 0; i < participantesList.Count; i++)
                            {
                                for (int j = i + 1; j < participantesList.Count; j++)
                                {
                                    var clube1 = participantesList[i];
                                    var clube2 = participantesList[j];

                                    // Verificar se este jogo já foi realizado
                                    if (jogosJaRealizados.ContainsKey(clube1) && 
                                        jogosJaRealizados[clube1].Contains(clube2))
                                    {
                                        continue;
                                    }

                                    // Criar novo emparelhamento
                                    var novoEmparelhamento = new EmparelhamentoFinal
                                    {
                                        CompeticaoId = competicaoId,
                                        Clube1 = clube1,
                                        Clube2 = clube2,
                                        DataJogo = DateTime.Now.AddDays(1), // Data padrão, pode ser ajustada
                                        HoraJogo = new TimeSpan(10, 0, 0), // Hora padrão, pode ser ajustada
                                        JogoRealizado = false
                                    };

                                    novosEmparelhamentos.Add(novoEmparelhamento);
                                }
                            }

                            // Adicionar novos emparelhamentos ao contexto
                            _context.EmparelhamentosFinal.AddRange(novosEmparelhamentos);
                            await _context.SaveChangesAsync();

                            return Ok(new { 
                                mensagem = "Novos emparelhamentos Round-Robin gerados com sucesso.",
                                competicaoId = competicaoId,
                                nomeCompeticao = nomeCompeticao,
                                novosEmparelhamentos = novosEmparelhamentos.Count
                            });
                        }
                        else
                        {
                            // Lógica existente para salvar emparelhamentos iniciais
                            var falhas = new List<EmparelhamentoViewModel>();
                            var sucessos = new List<string>();

                            foreach (var e in request.Emparelhamentos)
                            {
                                try
                                {
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
                    }
                    else
                    {
                        // Verificar jogos já realizados e com resultados registrados apenas quando gerando novos emparelhamentos
                        var jogosRealizados = await _context.EmparelhamentosFinal
                            .Where(e => e.CompeticaoId == competicaoId && 
                                      e.JogoRealizado && 
                                      e.PontuacaoClube1.HasValue && 
                                      e.PontuacaoClube2.HasValue)
                            .ToListAsync();

                        // Obter todos os participantes únicos
                        var participantes = new HashSet<string>();
                        foreach (var jogo in jogosRealizados)
                        {
                            participantes.Add(jogo.Clube1);
                            participantes.Add(jogo.Clube2);
                        }

                        // Se não houver participantes ainda, buscar todos os jogos para obter os participantes
                        if (!participantes.Any())
                        {
                            var todosJogos = await _context.EmparelhamentosFinal
                                .Where(e => e.CompeticaoId == competicaoId)
                                .ToListAsync();
                                
                            foreach (var jogo in todosJogos)
                            {
                                participantes.Add(jogo.Clube1);
                                participantes.Add(jogo.Clube2);
                            }
                        }

                        // Verificar se já existem todos os emparelhamentos possíveis
                        var totalParticipantes = participantes.Count;
                        var totalJogosPossiveis = (totalParticipantes * (totalParticipantes - 1)) / 2;

                        if (jogosRealizados.Count >= totalJogosPossiveis)
                        {
                            return BadRequest(new { mensagem = "Todos os jogos do Round-Robin já foram realizados e registrados." });
                        }

                        // Criar matriz de jogos já realizados
                        var jogosJaRealizados = new Dictionary<string, HashSet<string>>();
                        foreach (var jogo in jogosRealizados)
                        {
                            if (!jogosJaRealizados.ContainsKey(jogo.Clube1))
                                jogosJaRealizados[jogo.Clube1] = new HashSet<string>();
                            if (!jogosJaRealizados.ContainsKey(jogo.Clube2))
                                jogosJaRealizados[jogo.Clube2] = new HashSet<string>();

                            jogosJaRealizados[jogo.Clube1].Add(jogo.Clube2);
                            jogosJaRealizados[jogo.Clube2].Add(jogo.Clube1);
                        }

                        // Gerar novos emparelhamentos
                        var novosEmparelhamentos = new List<EmparelhamentoFinal>();
                        var participantesList = participantes.ToList();

                        for (int i = 0; i < participantesList.Count; i++)
                        {
                            for (int j = i + 1; j < participantesList.Count; j++)
                            {
                                var clube1 = participantesList[i];
                                var clube2 = participantesList[j];

                                // Verificar se este jogo já foi realizado
                                if (jogosJaRealizados.ContainsKey(clube1) && 
                                    jogosJaRealizados[clube1].Contains(clube2))
                                {
                                    continue;
                                }

                                // Criar novo emparelhamento
                                var novoEmparelhamento = new EmparelhamentoFinal
                                {
                                    CompeticaoId = competicaoId,
                                    Clube1 = clube1,
                                    Clube2 = clube2,
                                    DataJogo = DateTime.Now.AddDays(1), // Data padrão, pode ser ajustada
                                    HoraJogo = new TimeSpan(10, 0, 0), // Hora padrão, pode ser ajustada
                                    JogoRealizado = false
                                };

                                novosEmparelhamentos.Add(novoEmparelhamento);
                            }
                        }

                        // Adicionar novos emparelhamentos ao contexto
                        _context.EmparelhamentosFinal.AddRange(novosEmparelhamentos);
                        await _context.SaveChangesAsync();

                        return Ok(new { 
                            mensagem = "Novos emparelhamentos Round-Robin gerados com sucesso.",
                            competicaoId = competicaoId,
                            nomeCompeticao = nomeCompeticao,
                            novosEmparelhamentos = novosEmparelhamentos.Count
                        });
                    }
                }
                else
                {
                    // Lógica existente para outros formatos
                    var falhas = new List<EmparelhamentoViewModel>();
                    var sucessos = new List<string>();

                    foreach (var e in request.Emparelhamentos)
                    {
                        try
                        {
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
