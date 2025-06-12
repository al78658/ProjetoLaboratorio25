using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoLaboratorio25.Controllers
{
    public class ProximosJogosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProximosJogosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? competicaoId = null)
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

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObterProximosJogos(int competicaoId)
        {
            try
            {
                var competicao = await _context.Competicoes
                    .FirstOrDefaultAsync(c => c.Id == competicaoId);

                if (competicao == null)
                {
                    return NotFound(new { mensagem = "Competição não encontrada." });
                }

                // Verificar se é uma competição do tipo taça
                var configuracaoFase = await _context.ConfiguracoesFase
                    .Where(cf => cf.CompeticaoId == competicaoId)
                    .OrderByDescending(cf => cf.FaseNumero)
                    .FirstOrDefaultAsync();

                bool isTaca = configuracaoFase?.Formato?.ToLower() == "eliminacao";
                bool isRoundRobin = configuracaoFase?.Formato?.ToLower() == "round-robin";
                bool isDuploKO = configuracaoFase?.Formato?.ToLower() == "duplo-ko";
                bool isCampeonato = configuracaoFase?.Formato?.ToLower() == "campeonato";
                bool isSistemaAve = configuracaoFase?.Formato?.ToLower() == "ave";

                // Se for Duplo KO, verificar se estamos na fase final
                bool isFinalPhase = false;
                if (isDuploKO)
                {
                    // Contar derrotas para cada equipe
                    var derrotasPorEquipe = new Dictionary<string, int>();
                    var todosJogos = await _context.EmparelhamentosFinal
                        .Where(e => e.CompeticaoId == competicaoId && e.JogoRealizado)
                        .ToListAsync();

                    // Primeiro, identificar todas as equipes
                    var todasEquipes = new HashSet<string>();
                    foreach (var jogo in todosJogos)
                    {
                        if (!string.IsNullOrEmpty(jogo.Clube1)) todasEquipes.Add(jogo.Clube1);
                        if (!string.IsNullOrEmpty(jogo.Clube2)) todasEquipes.Add(jogo.Clube2);
                    }

                    // Inicializar contadores
                    foreach (var equipe in todasEquipes)
                    {
                        derrotasPorEquipe[equipe] = 0;
                    }

                    // Contar derrotas
                    foreach (var jogo in todosJogos)
                    {
                        if (jogo.PontuacaoClube1 < jogo.PontuacaoClube2)
                        {
                            derrotasPorEquipe[jogo.Clube1]++;
                        }
                        else if (jogo.PontuacaoClube2 < jogo.PontuacaoClube1)
                        {
                            derrotasPorEquipe[jogo.Clube2]++;
                        }
                    }

                    // Contar equipes com 0 e 1 derrota
                    var equipesInvictas = derrotasPorEquipe.Count(x => x.Value == 0);
                    var equipesUmaDerrota = derrotasPorEquipe.Count(x => x.Value == 1);

                    isFinalPhase = (equipesInvictas == 1 && equipesUmaDerrota == 1);

                    // Debug info
                    Console.WriteLine($"Equipes invictas: {equipesInvictas}");
                    Console.WriteLine($"Equipes com uma derrota: {equipesUmaDerrota}");
                    Console.WriteLine($"isFinalPhase: {isFinalPhase}");
                    Console.WriteLine("Derrotas por equipe:");
                    foreach (var kvp in derrotasPorEquipe)
                    {
                        Console.WriteLine($"{kvp.Key}: {kvp.Value} derrotas");
                    }
                }

                // Verificar se todos os jogos foram realizados
                var todosJogosRealizados = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId)
                    .AllAsync(e => e.JogoRealizado);

                // Buscar todos os jogos emparelhados para a competição
                var jogos = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId)
                    .OrderBy(e => e.DataJogo)
                    .ThenBy(e => e.HoraJogo)
                    .Select(e => new
                    {
                        e.Id,
                        e.Clube1,
                        e.Clube2,
                        Data = e.DataJogo.ToString("yyyy-MM-dd"),
                        DataFormatada = e.DataJogo.ToString("dd/MM/yyyy"),
                        Horario = e.HoraJogo.ToString(@"hh\:mm")
                    })
                    .ToListAsync();

                // Agrupar por data
                var jogosPorData = jogos
                    .GroupBy(j => j.Data)
                    .Select(g => new
                    {
                        Data = g.Key,
                        DataFormatada = g.First().DataFormatada,
                        Jogos = g.ToList()
                    })
                    .OrderBy(g => g.Data)
                    .ToList();

                return Ok(new
                {
                    jogosPorData,
                    isTaca,
                    isRoundRobin,
                    isDuploKO,
                    isCampeonato,
                    isSistemaAve,
                    todosJogosRealizados,
                    isFinalPhase
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao obter próximos jogos: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> VerificarJogos(int competicaoId)
        {
            try
            {
                // Contar jogos por competição
                var jogosCount = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId)
                    .CountAsync();

                // Obter todas as competições
                var competicoes = await _context.Competicoes.ToListAsync();

                return Ok(new
                {
                    jogosParaCompeticao = jogosCount,
                    totalCompetições = competicoes.Count,
                    competicoes = competicoes.Select(c => new { c.Id, c.Nome }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao verificar jogos: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CriarJogosExemplo(int competicaoId)
        {
            try
            {
                // Verificar se já existem jogos para esta competição
                var jogosExistentes = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId)
                    .CountAsync();

                if (jogosExistentes > 0)
                {
                    return Ok(new { mensagem = $"Já existem {jogosExistentes} jogos para esta competição." });
                }

                // Criar jogos de exemplo
                var jogosExemplo = new List<EmparelhamentoFinal>();

                // Criar 10 jogos de exemplo
                for (int i = 1; i <= 10; i++)
                {
                    var jogo = new EmparelhamentoFinal
                    {
                        CompeticaoId = competicaoId,
                        Clube1 = $"Equipe A{i}",
                        Clube2 = $"Equipe B{i}",
                        DataJogo = DateTime.Today.AddDays(i),
                        HoraJogo = new TimeSpan(14 + (i % 8), 0, 0), // Horários entre 14:00 e 21:00
                        JogoRealizado = false
                    };

                    jogosExemplo.Add(jogo);
                }

                _context.EmparelhamentosFinal.AddRange(jogosExemplo);
                await _context.SaveChangesAsync();

                return Ok(new { mensagem = $"Foram criados {jogosExemplo.Count} jogos de exemplo para a competição." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao criar jogos de exemplo: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarDatasHoras([FromBody] List<JogoAtualizacao> alteracoes)
        {
            try
            {
                if (alteracoes == null || !alteracoes.Any())
                {
                    return BadRequest(new { mensagem = "Nenhuma alteração fornecida" });
                }

                foreach (var alteracao in alteracoes)
                {
                    var jogo = await _context.EmparelhamentosFinal.FindAsync(alteracao.Id);
                    if (jogo != null)
                    {
                        // Converter string para DateTime
                        if (DateTime.TryParse(alteracao.Data, out DateTime novaData))
                        {
                            jogo.DataJogo = novaData;
                        }

                        // Converter string para TimeSpan
                        if (TimeSpan.TryParse(alteracao.Hora, out TimeSpan novaHora))
                        {
                            jogo.HoraJogo = novaHora;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { mensagem = "Datas e horas atualizadas com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao atualizar datas e horas: {ex.Message}" });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> GerarProximaFaseTaca(int competicaoId)
        {
            try
            {
                // Verificar se é uma competição do formato taça (eliminação)
                var configuracaoFase = await _context.ConfiguracoesFase
                    .Where(cf => cf.CompeticaoId == competicaoId)
                    .OrderByDescending(cf => cf.FaseNumero)
                    .FirstOrDefaultAsync();

                if (configuracaoFase?.Formato?.ToLower() != "eliminacao")
                {
                    return BadRequest(new { success = false, mensagem = "Esta competição não é do formato Taça (eliminação)." });
                }

                // Buscar a competição
                var competicao = await _context.Competicoes
                    .FirstOrDefaultAsync(c => c.Id == competicaoId);

                if (competicao == null)
                {
                    return NotFound(new { success = false, mensagem = "Competição não encontrada." });
                }

                // Verificar se há jogos não realizados
                var jogosNaoRealizados = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId && !e.JogoRealizado)
                    .AnyAsync();

                if (jogosNaoRealizados)
                {
                    return BadRequest(new { success = false, mensagem = "Existem jogos pendentes que ainda não foram realizados. Todos os jogos devem ser concluídos antes de gerar a próxima fase." });
                }

                // Determinar a fase atual com base nos jogos já realizados
                // Primeiro, vamos obter todos os jogos realizados ordenados por data de criação
                var todosJogosRealizados = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId && e.JogoRealizado)
                    .OrderByDescending(e => e.DataJogo)  // Ordenar por data do jogo, mais recentes primeiro
                    .ThenByDescending(e => e.HoraJogo)   // Em seguida, por hora
                    .ToListAsync();

                // Se não houver jogos realizados, não há como gerar a próxima fase
                if (!todosJogosRealizados.Any())
                {
                    return BadRequest(new { success = false, mensagem = "Não há jogos realizados para gerar a próxima fase." });
                }

                // Encontrar a ronda mais recente
                var rondaAtual = todosJogosRealizados.Max(j => j.RondaBracket ?? 0);
                
                // Se não houver ronda definida, usar 1 como ronda atual
                if (rondaAtual == 0)
                {
                    rondaAtual = 1;
                }
                
                // Obter apenas os jogos da última ronda
                var jogosUltimaRonda = todosJogosRealizados
                    .Where(j => j.RondaBracket == rondaAtual)
                    .ToList();
                
                // Contar quantos jogos temos na última ronda
                int numeroJogosUltimaRonda = jogosUltimaRonda.Count;
                
                // Se tivermos apenas um jogo na última ronda, o torneio já está concluído
                if (numeroJogosUltimaRonda == 1)
                {
                    var jogoFinal = jogosUltimaRonda.First();
                    string vencedor = jogoFinal.PontuacaoClube1 > jogoFinal.PontuacaoClube2 ? jogoFinal.Clube1 : jogoFinal.Clube2;
                    
                    return Ok(new { 
                        success = true, 
                        mensagem = $"O torneio foi concluído! O vencedor é {vencedor}.",
                        vencedorAbsoluto = vencedor
                    });
                }

                // Obter os vencedores dos jogos da última ronda
                var vencedores = new List<string>();
                foreach (var jogo in jogosUltimaRonda)
                {
                    if (jogo.PontuacaoClube1 > jogo.PontuacaoClube2)
                    {
                        vencedores.Add(jogo.Clube1);
                    }
                    else if (jogo.PontuacaoClube2 > jogo.PontuacaoClube1)
                    {
                        vencedores.Add(jogo.Clube2);
                    }
                    // Em caso de empate, você pode definir uma regra específica ou considerar ambos
                    // Por hora, estamos ignorando empates
                }

                // Se temos apenas um vencedor, o torneio acabou
                if (vencedores.Count == 1)
                {
                    return Ok(new { 
                        success = true, 
                        mensagem = $"O torneio foi concluído! O vencedor é {vencedores[0]}.",
                        vencedorAbsoluto = vencedores[0]
                    });
                }

                // Se tivermos um número ímpar de vencedores, o último recebe um "bye"
                bool temBye = vencedores.Count % 2 != 0;
                string? byeEquipe = null;
                
                if (temBye)
                {
                    byeEquipe = vencedores.Last();
                    vencedores.RemoveAt(vencedores.Count - 1);
                }

                // Criar emparelhamentos para a próxima fase
                var novosJogos = new List<EmparelhamentoFinal>();
                
                // Emparelhar os vencedores
                for (int i = 0; i < vencedores.Count; i += 2)
                {
                    if (i + 1 < vencedores.Count)
                    {
                        var jogo = new EmparelhamentoFinal
                        {
                            CompeticaoId = competicaoId,
                            Clube1 = vencedores[i],
                            Clube2 = vencedores[i + 1],
                            DataJogo = DateTime.Today.AddDays(1), // Data padrão
                            HoraJogo = new TimeSpan(14, 0, 0), // Hora padrão 14:00
                            JogoRealizado = false,
                            RondaBracket = rondaAtual + 1 // Incrementar a ronda
                        };
                        
                        novosJogos.Add(jogo);
                    }
                }

                // Adicionar o jogo com bye, se aplicável
                if (temBye && byeEquipe != null)
                {
                    // Se ainda tivermos um número par de vencedores após emparelhar, o bye joga contra o primeiro da próxima fase
                    if (novosJogos.Count > 0)
                    {
                        var jogoComBye = new EmparelhamentoFinal
                        {
                            CompeticaoId = competicaoId,
                            Clube1 = byeEquipe,
                            Clube2 = "(Vencedor do primeiro jogo)", // Placeholder até sabermos o vencedor
                            DataJogo = DateTime.Today.AddDays(2),
                            HoraJogo = new TimeSpan(14, 0, 0),
                            JogoRealizado = false,
                            RondaBracket = rondaAtual + 1 // Incrementar a ronda
                        };
                        
                        novosJogos.Add(jogoComBye);
                    }
                    else
                    {
                        // Se o bye for o único vencedor restante, ele é o campeão
                        return Ok(new { 
                            success = true, 
                            mensagem = $"O torneio foi concluído! O vencedor é {byeEquipe}.",
                            vencedorAbsoluto = byeEquipe
                        });
                    }
                }

                // Salvar os novos jogos no banco de dados
                await _context.EmparelhamentosFinal.AddRangeAsync(novosJogos);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    mensagem = $"Foram gerados {novosJogos.Count} jogos para a próxima fase do torneio."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, mensagem = $"Erro ao gerar próxima fase: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GerarSistemaAve(int competicaoId)
        {
            try
            {
                // Buscar a competição e suas configurações
                var competicao = await _context.Competicoes
                    .Include(c => c.ConfiguracoesFase)
                    .FirstOrDefaultAsync(c => c.Id == competicaoId);

                if (competicao == null)
                {
                    return NotFound(new { success = false, mensagem = "Competição não encontrada." });
                }

                // Verificar se é realmente um formato Sistema Ave
                var configuracaoFase = competicao.ConfiguracoesFase
                    .OrderByDescending(cf => cf.FaseNumero)
                    .FirstOrDefault();

                if (configuracaoFase?.Formato?.ToLower() != "ave")
                {
                    return BadRequest(new { success = false, mensagem = "Esta competição não é do formato Sistema Ave." });
                }

                // Buscar todos os jogos já realizados e agendados
                var jogosRealizados = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId && e.JogoRealizado)
                    .ToListAsync();

                var jogosAgendados = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId && !e.JogoRealizado)
                    .ToListAsync();

                // Verificar se já existem jogos agendados para todos os participantes
                var participantes = await _context.Jogadores
                    .Where(j => j.CompeticaoId == competicaoId)
                    .ToListAsync();

                var participantesComJogosAgendados = new HashSet<string>();
                foreach (var jogo in jogosAgendados)
                {
                    participantesComJogosAgendados.Add(jogo.Clube1.Trim());
                    participantesComJogosAgendados.Add(jogo.Clube2.Trim());
                }

                // Verificar se todos os participantes já têm jogos agendados
                bool todosTemJogosAgendados = participantes.All(p =>
                {
                    var nome = competicao.TipoCompeticao == "equipas" ? p.Clube?.Trim() : p.Nome?.Trim();
                    return !string.IsNullOrEmpty(nome) && participantesComJogosAgendados.Contains(nome);
                });

                if (todosTemJogosAgendados)
                {
                    return BadRequest(new { 
                        success = false, 
                        mensagem = "Todos os participantes já têm jogos agendados. Por favor, aguarde até que estes jogos sejam realizados antes de gerar novos emparelhamentos." 
                    });
                }

                // Buscar todos os participantes (equipes ou jogadores)
                var participantesList = await _context.Jogadores
                    .Where(j => j.CompeticaoId == competicaoId)
                    .ToListAsync();

                // Calcular pontuação para cada participante
                var pontuacoes = new Dictionary<string, int>();
                foreach (var participante in participantesList)
                {
                    var nome = competicao.TipoCompeticao == "equipas" ? participante.Clube : participante.Nome;
                    if (!string.IsNullOrEmpty(nome))
                    {
                        pontuacoes[nome.Trim()] = 0; // Initialize with 0 points and trim the name
                    }
                }

                // NOVA REGRA: Não permitir emparelhamento se nenhum participante tiver pelo menos um jogo realizado
                bool alguemTemJogo = participantesList.Any(p =>
                    jogosRealizados.Any(j =>
                        (j.Clube1.Trim() == (competicao.TipoCompeticao == "equipas" ? p.Clube?.Trim() : p.Nome?.Trim())) ||
                        (j.Clube2.Trim() == (competicao.TipoCompeticao == "equipas" ? p.Clube?.Trim() : p.Nome?.Trim()))
                    )
                );

                if (!alguemTemJogo)
                {
                    return BadRequest(new { success = false, mensagem = "Não é possível emparelhar: nenhum participante tem jogos registados ainda." });
                }

                // Calcular pontuação baseada nos jogos realizados
                foreach (var jogo in jogosRealizados)
                {
                    var clube1 = jogo.Clube1.Trim();
                    var clube2 = jogo.Clube2.Trim();

                    if (!pontuacoes.ContainsKey(clube1))
                        pontuacoes[clube1] = 0;
                    if (!pontuacoes.ContainsKey(clube2))
                        pontuacoes[clube2] = 0;

                    if (jogo.PontuacaoClube1 > jogo.PontuacaoClube2)
                    {
                        pontuacoes[clube1] += 3;
                    }
                    else if (jogo.PontuacaoClube2 > jogo.PontuacaoClube1)
                    {
                        pontuacoes[clube2] += 3;
                    }
                    else
                    {
                        pontuacoes[clube1] += 1;
                        pontuacoes[clube2] += 1;
                    }
                }

                // Ordenar participantes por pontuação
                var participantesOrdenados = pontuacoes
                    .OrderByDescending(p => p.Value)
                    .Select(p => p.Key)
                    .ToList();

                // Criar um dicionário para rastrear jogos já realizados entre participantes
                var jogosEntreParticipantes = new Dictionary<string, HashSet<string>>();
                foreach (var participante in participantesOrdenados)
                {
                    jogosEntreParticipantes[participante] = new HashSet<string>();
                }

                // Preencher o dicionário com jogos já realizados
                foreach (var jogo in jogosRealizados)
                {
                    jogosEntreParticipantes[jogo.Clube1].Add(jogo.Clube2);
                    jogosEntreParticipantes[jogo.Clube2].Add(jogo.Clube1);
                }

                // Criar novos emparelhamentos
                var novosEmparelhamentos = new List<EmparelhamentoFinal>();
                var participantesUsados = new HashSet<string>();

                // Para cada participante, encontrar um oponente
                foreach (var participante1 in participantesOrdenados)
                {
                    if (participantesUsados.Contains(participante1))
                        continue;

                    // Encontrar o melhor oponente disponível
                    string melhorOponente = null;
                    int melhorPosicao = int.MaxValue;

                    // Procurar o oponente com menos pontos que ainda não jogou contra
                    for (int i = participantesOrdenados.Count - 1; i >= 0; i--)
                    {
                        var possivelOponente = participantesOrdenados[i];

                        // Verificar se:
                        // 1. Não é o mesmo participante
                        // 2. Ainda não foi usado
                        // 3. Ainda não jogou contra o participante1
                        if (possivelOponente != participante1 &&
                            !participantesUsados.Contains(possivelOponente) &&
                            !jogosEntreParticipantes[participante1].Contains(possivelOponente))
                        {
                            melhorOponente = possivelOponente;
                            melhorPosicao = i;
                            break;
                        }
                    }

                    if (melhorOponente != null)
                    {
                        var emparelhamento = new EmparelhamentoFinal
                        {
                            CompeticaoId = competicaoId,
                            Clube1 = participante1,
                            Clube2 = melhorOponente,
                            DataJogo = DateTime.Today.AddDays(1), // Data padrão, pode ser ajustada depois
                            HoraJogo = new TimeSpan(14, 0, 0), // Horário padrão, pode ser ajustado depois
                            JogoRealizado = false
                        };

                        novosEmparelhamentos.Add(emparelhamento);
                        participantesUsados.Add(participante1);
                        participantesUsados.Add(melhorOponente);
                    }
                }

                // Verificar se todos os participantes foram emparelhados
                if (participantesUsados.Count != participantesOrdenados.Count)
                {
                    return BadRequest(new { success = false, mensagem = "Não foi possível criar emparelhamentos para todos os participantes. Alguns participantes já jogaram contra todos os outros." });
                }

                // Adicionar os novos emparelhamentos ao banco de dados
                _context.EmparelhamentosFinal.AddRange(novosEmparelhamentos);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, mensagem = "Emparelhamentos gerados com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, mensagem = $"Erro ao gerar emparelhamentos: {ex.Message}" });
            }
        }
    }

    // Classe para receber os dados de atualização
    public class JogoAtualizacao
    {
        public int Id { get; set; }
        public string Data { get; set; }
        public string Hora { get; set; }
    }
}
