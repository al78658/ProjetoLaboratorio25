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

                // Se for taça, verificar se só resta um vencedor
                if (isTaca)
                {
                    var jogosRealizados = await _context.EmparelhamentosFinal
                        .Where(e => e.CompeticaoId == competicaoId && e.JogoRealizado)
                        .ToListAsync();

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

                    if (vitoriasDict.Any())
                    {
                        var maxVitorias = vitoriasDict.Values.Max();
                        var clubesComMaisVitorias = vitoriasDict
                            .Where(kvp => kvp.Value == maxVitorias)
                            .Select(kvp => kvp.Key)
                            .ToList();

                        if (clubesComMaisVitorias.Count == 1)
                        {
                            return Ok(new
                            {
                                mensagem = "Competição encerrada",
                                vencedorFinal = clubesComMaisVitorias[0]
                            });
                        }
                    }
                }

                // Se for round-robin, verificar se todos os emparelhamentos possíveis foram feitos
                if (isRoundRobin)
                {
                    var todosJogos = await _context.EmparelhamentosFinal
                        .Where(e => e.CompeticaoId == competicaoId)
                        .ToListAsync();

                    var participantes = new HashSet<string>();
                    foreach (var jogo in todosJogos)
                    {
                        if (!string.IsNullOrEmpty(jogo.Clube1)) participantes.Add(jogo.Clube1);
                        if (!string.IsNullOrEmpty(jogo.Clube2)) participantes.Add(jogo.Clube2);
                    }

                    var totalParticipantes = participantes.Count;
                    var totalJogosPossiveis = (totalParticipantes * (totalParticipantes - 1)) / 2;

                    if (todosJogos.Count >= totalJogosPossiveis)
                    {
                        // Calcular o vencedor baseado nas vitórias
                        var vitoriasDict = new Dictionary<string, int>();
                        foreach (var jogo in todosJogos.Where(j => j.JogoRealizado))
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

                        if (vitoriasDict.Any())
                        {
                            var maxVitorias = vitoriasDict.Values.Max();
                            var clubesComMaisVitorias = vitoriasDict
                                .Where(kvp => kvp.Value == maxVitorias)
                                .Select(kvp => kvp.Key)
                                .ToList();

                            if (clubesComMaisVitorias.Count == 1)
                            {
                                return Ok(new
                                {
                                    mensagem = "Competição encerrada",
                                    vencedorFinal = clubesComMaisVitorias[0]
                                });
                            }
                        }
                    }
                }

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
        public async Task<IActionResult> AtualizarPontuacao([FromBody] AtualizarPontuacaoModel model)
        {
            try
            {
                var jogo = await _context.EmparelhamentosFinal.FindAsync(model.EmparelhamentoId);
                if (jogo == null)
                {
                    return NotFound(new { mensagem = "Jogo não encontrado." });
                }

                // Verificar se é um jogo aguardando resolução do Losers Bracket
                if (jogo.Motivo == "Aguardando resolução do Losers Bracket")
                {
                    return BadRequest(new { mensagem = "Não é possível atualizar pontuações para este jogo enquanto estiver aguardando resolução do Losers Bracket." });
                }

                // Verificar se é um empate
                if (model.PontuacaoClube1 == model.PontuacaoClube2)
                {
                    return BadRequest(new { mensagem = "Não é possível registrar empate neste jogo." });
                }

                jogo.PontuacaoClube1 = model.PontuacaoClube1;
                jogo.PontuacaoClube2 = model.PontuacaoClube2;
                jogo.JogoRealizado = true;
                jogo.Motivo = model.Motivo;

                await _context.SaveChangesAsync();
                return Ok(new { mensagem = "Pontuações atualizadas com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao atualizar pontuações: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GerarProximaFaseDuploKO(int competicaoId)
        {
            try
            {
                // Verificar se é uma competição do formato Duplo KO
                var configuracaoFase = await _context.ConfiguracoesFase
                    .Where(cf => cf.CompeticaoId == competicaoId)
                    .OrderByDescending(cf => cf.FaseNumero)
                    .FirstOrDefaultAsync();

                if (configuracaoFase?.Formato?.ToLower() != "duplo-ko")
                {
                    return BadRequest(new { success = false, mensagem = "Esta competição não é do formato Duplo KO." });
                }

                // Buscar todos os jogos
                var todosJogos = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId)
                    .ToListAsync();

                // Verificar se existe um jogo do winners em espera
                var jogoWinnersEspera = todosJogos
                    .FirstOrDefault(e => e.Bracket == "WinnersEspera" && !e.JogoRealizado);

                if (jogoWinnersEspera != null)
                {
                    // Se existe um jogo em espera, não permitir gerar novos emparelhamentos
                    return BadRequest(new { 
                        success = false, 
                        mensagem = "Existe um jogo do Winners Bracket em espera. Complete este jogo antes de gerar novos emparelhamentos." 
                    });
                }

                // Resto da lógica existente para gerar novos emparelhamentos
                var novosEmparelhamentos = new List<EmparelhamentoFinal>();
                
                // ... existing code for generating new pairings ...

                // Quando criar novos emparelhamentos, verificar se há jogos em espera
                var jogosEmEspera = todosJogos
                    .Where(e => e.Motivo == "Aguardando resolução do Losers Bracket")
                    .ToList();

                foreach (var jogo in jogosEmEspera)
                {
                    // Atualizar o bracket para "Winners" normal e limpar o motivo
                    jogo.Bracket = "Winners";
                    jogo.Motivo = null;
                }

                // Adicionar os novos emparelhamentos
                if (novosEmparelhamentos.Any())
                {
                    await _context.EmparelhamentosFinal.AddRangeAsync(novosEmparelhamentos);
                }

                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    mensagem = "Nova fase gerada com sucesso.",
                    novosEmparelhamentos = novosEmparelhamentos.Count
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

    // Remover a definição duplicada do AtualizarPontuacaoModel que está no final do arquivo
}
