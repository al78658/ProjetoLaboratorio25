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
                bool isCampeonato = configuracaoFase?.Formato?.ToLower() == "campeonato";
                bool isSistemaAve = configuracaoFase?.Formato?.ToLower() == "ave";

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
                    isCampeonato,
                    isSistemaAve,
                    todosJogosRealizados
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

                // Buscar todos os jogos já realizados
                var jogosRealizados = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId && e.JogoRealizado)
                    .ToListAsync();

                // Buscar todos os participantes (equipes ou jogadores)
                var participantes = await _context.Jogadores
                    .Where(j => j.CompeticaoId == competicaoId)
                    .ToListAsync();

                // Calcular pontuação para cada participante
                var pontuacoes = new Dictionary<string, int>();
                foreach (var participante in participantes)
                {
                    var nome = competicao.TipoCompeticao == "equipas" ? participante.Clube : participante.Nome;
                    if (!string.IsNullOrEmpty(nome))
                    {
                        pontuacoes[nome] = 0;
                    }
                }

                // NOVA REGRA: Não permitir emparelhamento se nenhum participante tiver pelo menos um jogo realizado
                bool alguemTemJogo = participantes.Any(p =>
                    jogosRealizados.Any(j =>
                        (j.Clube1 == (competicao.TipoCompeticao == "equipas" ? p.Clube : p.Nome)) ||
                        (j.Clube2 == (competicao.TipoCompeticao == "equipas" ? p.Clube : p.Nome))
                    )
                );

                if (!alguemTemJogo)
                {
                    return BadRequest(new { success = false, mensagem = "Não é possível emparelhar: nenhum participante tem jogos registados ainda." });
                }

                // Calcular pontuação baseada nos jogos realizados
                foreach (var jogo in jogosRealizados)
                {
                    if (jogo.PontuacaoClube1 > jogo.PontuacaoClube2)
                    {
                        pontuacoes[jogo.Clube1] += 3;
                    }
                    else if (jogo.PontuacaoClube2 > jogo.PontuacaoClube1)
                    {
                        pontuacoes[jogo.Clube2] += 3;
                    }
                    else
                    {
                        pontuacoes[jogo.Clube1] += 1;
                        pontuacoes[jogo.Clube2] += 1;
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
