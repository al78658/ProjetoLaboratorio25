using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;



namespace ProjetoLaboratorio25.Controllers
{
    public class FormatodaCompeticaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FormatodaCompeticaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Get the competition data from TempData
            ViewBag.NomeCompeticao = TempData["NomeCompeticao"];
            ViewBag.TipoCompeticao = TempData["TipoCompeticao"];
            ViewBag.CompeticaoId = TempData["CompeticaoId"];

            var competicaoId = TempData["CompeticaoId"]?.ToString();
            ViewBag.FormatosSelecionados = competicaoId != null ? (TempData[$"FormatosSelecionados_{competicaoId}"] as string ?? "{}") : "{}";
            ViewBag.NumFases = TempData[$"NumFases_{competicaoId}"] ?? 2;

            // Preserve TempData for the next request
            TempData.Keep("NomeCompeticao");
            TempData.Keep("TipoCompeticao");
            TempData.Keep("CompeticaoId");
            if (competicaoId != null)
            {
                TempData.Keep($"FormatosSelecionados_{competicaoId}");
                TempData.Keep($"NumFases_{competicaoId}");
            }

            return View();
        }

        [HttpPost]
        public IActionResult SalvarConfiguracao(int competicaoId, int numFases, int numJogosPorFase)
        {
            // Obter a competição existente
            var competicao = _context.Competicoes
                .Include(c => c.ConfiguracoesFase)
                .FirstOrDefault(c => c.Id == competicaoId);

            if (competicao == null)
            {
                return NotFound();
            }

            // Verificar se já existem configurações para esta competição
            var configuracoesExistentes = _context.ConfiguracoesFase
                .Where(c => c.CompeticaoId == competicaoId)
                .ToList();

            // Remover configurações existentes se o número de fases mudou
            if (configuracoesExistentes.Count > 0 && configuracoesExistentes.Count != numFases)
            {
                _context.ConfiguracoesFase.RemoveRange(configuracoesExistentes);
                _context.SaveChanges();
                configuracoesExistentes.Clear();
            }

            // Recuperar os formatos selecionados, se existirem
            Dictionary<int, string> formatos = new Dictionary<int, string>();
            if (TempData.ContainsKey($"FormatosSelecionados_{competicaoId}"))
            {
                string formatosJson = TempData[$"FormatosSelecionados_{competicaoId}"] as string;
                if (!string.IsNullOrEmpty(formatosJson))
                {
                    try
                    {
                        formatos = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, string>>(formatosJson);
                    }
                    catch
                    {
                        // Em caso de erro, continua com o dicionário vazio
                    }
                }
                TempData.Keep($"FormatosSelecionados_{competicaoId}");
            }

            // Adicionar ou atualizar configurações de fase
            for (int i = 1; i <= numFases; i++)
            {
                // Verificar se já existe uma configuração para esta fase
                var configuracaoExistente = configuracoesExistentes.FirstOrDefault(c => c.FaseNumero == i);
                
                if (configuracaoExistente != null)
                {
                    // Atualizar a configuração existente
                    configuracaoExistente.NumJogosPorFase = numJogosPorFase;
                    
                    // Atualizar o formato se estiver definido
                    if (formatos.ContainsKey(i))
                    {
                        configuracaoExistente.Formato = formatos[i];
                    }
                    
                    _context.ConfiguracoesFase.Update(configuracaoExistente);
                }
                else
                {
                    // Determinar o formato a ser usado
                    string formato = "round-robin"; // Formato padrão
                    if (formatos.ContainsKey(i))
                    {
                        formato = formatos[i];
                    }
                    
                    // Criar uma nova configuração
                    var configuracao = new ConfiguracaoFase
                    {
                        CompeticaoId = competicaoId,
                        FaseNumero = i,
                        NumJogosPorFase = numJogosPorFase,
                        Formato = formato,
                        PontosVitoria = competicao.PontosVitoria,
                        PontosEmpate = competicao.PontosEmpate,
                        PontosDerrota = 0,
                        PontosFaltaComparencia = 0,
                        PontosDesclassificacao = 0,
                        PontosExtra = 0,
                        CriteriosDesempate = new List<string>() { "confronto" } // Critério padrão
                    };
                    _context.ConfiguracoesFase.Add(configuracao);
                }
            }

            // Salvar mudanças
            _context.SaveChanges();

            // Armazenar dados temporários para próxima view
            TempData["NomeCompeticao"] = competicao.Nome;
            TempData["TipoCompeticao"] = competicao.TipoCompeticao;
            TempData["CompeticaoId"] = competicao.Id;
            TempData["NumFases"] = numFases;
            TempData["NumJogosPorFase"] = numJogosPorFase;

            // Redirecionar para a próxima fase
            return RedirectToAction("Index", "ListadeJogadores");
        }

        [HttpPost]
        public IActionResult SalvarFormatos([FromBody] Dictionary<int, string> formatos, [FromQuery] string competicaoId)
        {
            if (!string.IsNullOrEmpty(competicaoId) && int.TryParse(competicaoId, out int compId))
            {
                // Salvar os formatos no TempData para uso posterior
                TempData[$"FormatosSelecionados_{competicaoId}"] = System.Text.Json.JsonSerializer.Serialize(formatos);
                TempData.Keep($"FormatosSelecionados_{competicaoId}");
                
                // Atualizar as configurações de fase no banco de dados
                foreach (var formato in formatos)
                {
                    int faseNumero = formato.Key;
                    string formatoSelecionado = formato.Value;
                    
                    // Buscar a configuração existente
                    var configuracao = _context.ConfiguracoesFase
                        .FirstOrDefault(c => c.CompeticaoId == compId && c.FaseNumero == faseNumero);
                    
                    if (configuracao != null)
                    {
                        // Atualizar o formato
                        configuracao.Formato = formatoSelecionado;
                        _context.ConfiguracoesFase.Update(configuracao);
                    }
                    else
                    {
                        // Criar uma nova configuração se não existir
                        var novaConfiguracao = new ConfiguracaoFase
                        {
                            CompeticaoId = compId,
                            FaseNumero = faseNumero,
                            Formato = formatoSelecionado,
                            NumJogosPorFase = 1, // Valor padrão
                            PontosVitoria = 3,   // Valores padrão
                            PontosEmpate = 1,
                            PontosDerrota = 0,
                            PontosFaltaComparencia = 0,
                            PontosDesclassificacao = 0,
                            PontosExtra = 0,
                            CriteriosDesempate = new List<string>()
                        };
                        _context.ConfiguracoesFase.Add(novaConfiguracao);
                    }
                }
                
                // Salvar as alterações
                _context.SaveChanges();
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult SalvarNumFases([FromQuery] string competicaoId, [FromBody] int numFases)
        {
            if (!string.IsNullOrEmpty(competicaoId) && int.TryParse(competicaoId, out int compId))
            {
                // Salvar o número de fases no TempData
                TempData[$"NumFases_{competicaoId}"] = numFases;
                TempData.Keep($"NumFases_{competicaoId}");
                
                // Buscar as configurações existentes
                var configuracoesExistentes = _context.ConfiguracoesFase
                    .Where(c => c.CompeticaoId == compId)
                    .ToList();
                
                // Se o número de fases aumentou, adicionar novas configurações
                int fasesExistentes = configuracoesExistentes.Count;
                
                if (numFases > fasesExistentes)
                {
                    // Buscar a competição para obter valores padrão
                    var competicao = _context.Competicoes.Find(compId);
                    if (competicao != null)
                    {
                        // Adicionar novas fases
                        for (int i = fasesExistentes + 1; i <= numFases; i++)
                        {
                            var novaConfiguracao = new ConfiguracaoFase
                            {
                                CompeticaoId = compId,
                                FaseNumero = i,
                                NumJogosPorFase = 1, // Valor padrão
                                Formato = "round-robin", // Formato padrão
                                PontosVitoria = competicao.PontosVitoria,
                                PontosEmpate = competicao.PontosEmpate,
                                PontosDerrota = 0,
                                PontosFaltaComparencia = 0,
                                PontosDesclassificacao = 0,
                                PontosExtra = 0,
                                CriteriosDesempate = new List<string>()
                            };
                            _context.ConfiguracoesFase.Add(novaConfiguracao);
                        }
                    }
                }
                else if (numFases < fasesExistentes)
                {
                    // Se o número de fases diminuiu, remover as fases excedentes
                    var fasesParaRemover = configuracoesExistentes
                        .Where(c => c.FaseNumero > numFases)
                        .ToList();
                    
                    if (fasesParaRemover.Any())
                    {
                        _context.ConfiguracoesFase.RemoveRange(fasesParaRemover);
                    }
                }
                
                // Salvar as alterações
                _context.SaveChanges();
            }
            return Ok();
        }
    }
}
