﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoLaboratorio25.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? competicaoId = null)
        {
            // Se não for fornecido um ID de competição, não buscar automaticamente a mais recente
            // Isso permitirá que o JavaScript lide com a seleção da competição
            if (!competicaoId.HasValue)
            {
                // Não definimos um ID padrão aqui, deixamos o JavaScript lidar com isso
                return View();
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
        public async Task<IActionResult> ObterTabelaClassificativa(int competicaoId)
        {
            try
            {
                // Buscar a competição para obter os pontos por vitória e empate
                var competicao = await _context.Competicoes
                    .FirstOrDefaultAsync(c => c.Id == competicaoId);

                if (competicao == null)
                {
                    return NotFound(new { mensagem = "Competição não encontrada." });
                }

                // Buscar todos os emparelhamentos da competição que já foram realizados
                var emparelhamentos = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId && e.JogoRealizado)
                    .ToListAsync();

                // Calcular a tabela classificativa
                var clubes = new Dictionary<string, ClassificacaoViewModel>();

                // Processar cada emparelhamento
                foreach (var emp in emparelhamentos)
                {
                    // Processar clube 1
                    if (!clubes.ContainsKey(emp.Clube1))
                    {
                        clubes[emp.Clube1] = new ClassificacaoViewModel { Nome = emp.Clube1 };
                    }

                    // Processar clube 2
                    if (!clubes.ContainsKey(emp.Clube2))
                    {
                        clubes[emp.Clube2] = new ClassificacaoViewModel { Nome = emp.Clube2 };
                    }

                    // Atualizar estatísticas
                    var clube1 = clubes[emp.Clube1];
                    var clube2 = clubes[emp.Clube2];

                    clube1.Jogos++;
                    clube2.Jogos++;

                    if (emp.PontuacaoClube1.HasValue && emp.PontuacaoClube2.HasValue)
                    {
                        // Atualizar frames
                        clube1.FramesGanhos += emp.PontuacaoClube1.Value;
                        clube1.FramesPerdidos += emp.PontuacaoClube2.Value;
                        clube2.FramesGanhos += emp.PontuacaoClube2.Value;
                        clube2.FramesPerdidos += emp.PontuacaoClube1.Value;

                        // Determinar resultado
                        if (emp.PontuacaoClube1.Value > emp.PontuacaoClube2.Value)
                        {
                            // Clube 1 venceu
                            clube1.Vitorias++;
                            clube1.Pontuacao += competicao.PontosVitoria;
                            clube1.Forma.Add("V");
                            
                            clube2.Derrotas++;
                            clube2.Forma.Add("D");
                        }
                        else if (emp.PontuacaoClube2.Value > emp.PontuacaoClube1.Value)
                        {
                            // Clube 2 venceu
                            clube2.Vitorias++;
                            clube2.Pontuacao += competicao.PontosVitoria;
                            clube2.Forma.Add("V");
                            
                            clube1.Derrotas++;
                            clube1.Forma.Add("D");
                        }
                        else
                        {
                            // Empate
                            clube1.Empates++;
                            clube1.Pontuacao += competicao.PontosEmpate;
                            clube1.Forma.Add("E");
                            
                            clube2.Empates++;
                            clube2.Pontuacao += competicao.PontosEmpate;
                            clube2.Forma.Add("E");
                        }
                    }
                }

                // Ordenar a tabela por pontuação (decrescente)
                var tabelaClassificativa = clubes.Values
                    .OrderByDescending(c => c.Pontuacao)
                    .ThenByDescending(c => c.Vitorias)
                    .ThenByDescending(c => c.FramesGanhos - c.FramesPerdidos)
                    .ToList();

                // Atribuir posições
                for (int i = 0; i < tabelaClassificativa.Count; i++)
                {
                    tabelaClassificativa[i].Posicao = i + 1;
                    
                    // Limitar a forma aos últimos 3 jogos
                    if (tabelaClassificativa[i].Forma.Count > 3)
                    {
                        tabelaClassificativa[i].Forma = tabelaClassificativa[i].Forma
                            .Skip(tabelaClassificativa[i].Forma.Count - 3)
                            .ToList();
                    }
                }

                return Ok(tabelaClassificativa);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao obter tabela classificativa: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObterResultados(int competicaoId, string data = null)
        {
            try
            {
                // Se não for fornecida uma data, usar a data atual
                DateTime dataFiltro;
                
                Console.WriteLine($"ObterResultados - Data recebida: '{data}'");
                
                if (string.IsNullOrEmpty(data))
                {
                    Console.WriteLine("Data não fornecida, usando data atual");
                    dataFiltro = DateTime.Today;
                }
                else
                {
                    // Tentar converter a string para DateTime
                    if (!DateTime.TryParse(data, out dataFiltro))
                    {
                        Console.WriteLine($"Formato de data inválido: {data}, tentando formato alternativo");
                        
                        // Tentar formato alternativo (yyyy-MM-dd)
                        try {
                            var parts = data.Split('-');
                            if (parts.Length == 3)
                            {
                                int year = int.Parse(parts[0]);
                                int month = int.Parse(parts[1]);
                                int day = int.Parse(parts[2]);
                                dataFiltro = new DateTime(year, month, day);
                                Console.WriteLine($"Data convertida com sucesso usando formato alternativo: {dataFiltro.ToShortDateString()}");
                            }
                            else
                            {
                                Console.WriteLine("Formato de data não reconhecido, usando data atual");
                                dataFiltro = DateTime.Today;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao converter data: {ex.Message}, usando data atual");
                            dataFiltro = DateTime.Today;
                        }
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
                // Registrar o erro para depuração
                Console.Error.WriteLine($"Erro ao obter resultados: {ex.Message}");
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Retornar uma mensagem de erro mais amigável para o usuário
                return StatusCode(500, new { 
                    mensagem = "Não foi possível obter os resultados. Por favor, tente novamente mais tarde.",
                    detalhes = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> AdicionarJogosExemplo(int competicaoId)
        {
            try
            {
                // Verificar se a competição existe
                var competicao = await _context.Competicoes.FindAsync(competicaoId);
                if (competicao == null)
                {
                    return NotFound(new { mensagem = $"Competição com ID {competicaoId} não encontrada." });
                }
                
                // Verificar se já existem jogos para esta competição
                var jogosExistentes = await _context.EmparelhamentosFinal
                    .Where(e => e.CompeticaoId == competicaoId)
                    .CountAsync();
                
                if (jogosExistentes > 0)
                {
                    return Ok(new { mensagem = $"Já existem {jogosExistentes} jogos para esta competição." });
                }
                
                // Adicionar jogos de exemplo
                var jogos = new List<EmparelhamentoFinal>
                {
                    // Jogos para hoje
                    new EmparelhamentoFinal
                    {
                        CompeticaoId = competicaoId,
                        Clube1 = "Benfica",
                        Clube2 = "Porto",
                        DataJogo = DateTime.Today,
                        HoraJogo = new TimeSpan(19, 30, 0),
                        JogoRealizado = false
                    },
                    new EmparelhamentoFinal
                    {
                        CompeticaoId = competicaoId,
                        Clube1 = "Sporting",
                        Clube2 = "Braga",
                        DataJogo = DateTime.Today,
                        HoraJogo = new TimeSpan(18, 45, 0),
                        JogoRealizado = true,
                        PontuacaoClube1 = 3,
                        PontuacaoClube2 = 2,
                        Motivo = "Vitória por mérito"
                    },
                    
                    // Jogos para amanhã
                    new EmparelhamentoFinal
                    {
                        CompeticaoId = competicaoId,
                        Clube1 = "Vitória SC",
                        Clube2 = "Boavista",
                        DataJogo = DateTime.Today.AddDays(1),
                        HoraJogo = new TimeSpan(20, 15, 0),
                        JogoRealizado = false
                    },
                    
                    // Jogos para ontem
                    new EmparelhamentoFinal
                    {
                        CompeticaoId = competicaoId,
                        Clube1 = "Famalicão",
                        Clube2 = "Rio Ave",
                        DataJogo = DateTime.Today.AddDays(-1),
                        HoraJogo = new TimeSpan(17, 0, 0),
                        JogoRealizado = true,
                        PontuacaoClube1 = 1,
                        PontuacaoClube2 = 1
                    }
                };
                
                _context.EmparelhamentosFinal.AddRange(jogos);
                await _context.SaveChangesAsync();
                
                return Ok(new { mensagem = $"Adicionados {jogos.Count} jogos de exemplo para a competição {competicao.Nome}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = $"Erro ao adicionar jogos de exemplo: {ex.Message}" });
            }
        }
    }

    public class ClassificacaoViewModel
    {
        public int Posicao { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Jogos { get; set; }
        public int Vitorias { get; set; }
        public int Empates { get; set; }
        public int Derrotas { get; set; }
        public int FramesGanhos { get; set; }
        public int FramesPerdidos { get; set; }
        public string Frames => $"{FramesGanhos}:{FramesPerdidos}";
        public int Pontuacao { get; set; }
        public List<string> Forma { get; set; } = new List<string>();
    }
}
