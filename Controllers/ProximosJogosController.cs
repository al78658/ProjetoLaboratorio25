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
                
                return Ok(new { 
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
    }
    
    // Classe para receber os dados de atualização
    public class JogoAtualizacao
    {
        public int Id { get; set; }
        public string Data { get; set; }
        public string Hora { get; set; }
    }
}
