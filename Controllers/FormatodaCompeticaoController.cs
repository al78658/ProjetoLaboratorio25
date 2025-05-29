using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ProjetoLaboratorio25.Controllers
{
    public class FormatodaCompeticaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FormatodaCompeticaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? competicaoId = null, bool edicao = false)
        {
            try
            {
                // Tenta obter o ID da competição do parâmetro, se não estiver disponível, tenta do TempData
                int compId = 0;
                if (competicaoId.HasValue && competicaoId.Value > 0)
                {
                    compId = competicaoId.Value;
                    // Salva no TempData para uso futuro
                    TempData["CompeticaoId"] = compId;
                    TempData.Keep("CompeticaoId");
                }
                else if (TempData.ContainsKey("CompeticaoId") && TempData["CompeticaoId"] != null)
                {
                    compId = Convert.ToInt32(TempData["CompeticaoId"]);
                    TempData.Keep("CompeticaoId");
                }

                if (compId == 0)
                {
                    // Se não encontrou o ID da competição, redireciona para a página de competições
                    TempData["Erro"] = "ID da competição não encontrado. Por favor, selecione uma competição primeiro.";
                    return RedirectToAction("Index", "Competicoes");
                }

                // Busca a competição no banco de dados com suas configurações
                var competicao = _context.Competicoes
                    .Include(c => c.ConfiguracoesFase)
                    .FirstOrDefault(c => c.Id == compId);

                if (competicao == null)
                {
                    TempData["Erro"] = "Competição não encontrada no banco de dados.";
                    return RedirectToAction("Index", "Competicoes");
                }

                // Verifica se a competição tem configurações de fase
                if (!competicao.ConfiguracoesFase.Any())
                {
                    // Se não tiver configurações, cria as configurações padrão
                    var configuracaoPadrao = new ConfiguracaoFase
                    {
                        CompeticaoId = compId,
                        FaseNumero = 1,
                        NumJogosPorFase = 1,
                        Formato = "",
                        PontosVitoria = competicao.PontosVitoria,
                        PontosEmpate = competicao.PontosEmpate,
                        PontosDerrota = 0,
                        PontosFaltaComparencia = 0,
                        PontosDesclassificacao = 0,
                        PontosExtra = 0,
                        CriteriosDesempate = new List<string>() { "confronto" }
                    };
                    _context.ConfiguracoesFase.Add(configuracaoPadrao);
                    _context.SaveChanges();
                }

                // Atualiza o ViewBag com os dados da competição
                ViewBag.NomeCompeticao = competicao.Nome;
                ViewBag.TipoCompeticao = competicao.TipoCompeticao;
                ViewBag.CompeticaoId = compId;

                // Carrega as configurações de fase do banco de dados
                var configuracoes = competicao.ConfiguracoesFase
                    .OrderBy(c => c.FaseNumero)
                    .ToList();

                if (configuracoes.Any())
                {
                    // Converte configurações para formato de dicionário
                    var formatosDict = configuracoes.ToDictionary(c => c.FaseNumero, c => c.Formato);
                    ViewBag.FormatosSelecionados = System.Text.Json.JsonSerializer.Serialize(formatosDict);
                    ViewBag.NumFases = configuracoes.Count;
                }
                else
                {
                    // Se não houver configurações, usa valores padrão
                    ViewBag.FormatosSelecionados = "{}";
                    ViewBag.NumFases = 2;
                }

                // Passa o modo de edição para a view
                ViewBag.Edicao = edicao;

                // Preserva TempData para a próxima requisição
                TempData.Keep("NomeCompeticao");
                TempData.Keep("TipoCompeticao");
                TempData.Keep("CompeticaoId");
                TempData.Keep($"FormatosSelecionados_{compId}");
                TempData.Keep($"NumFases_{compId}");

                return View();
            }
            catch (Exception ex)
            {
                // Log the error (you should implement proper logging)
                TempData["Erro"] = "Ocorreu um erro ao carregar a competição. Por favor, tente novamente.";
                return RedirectToAction("Index", "Competicoes");
            }
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
                    string formato = ""; // Formato padrão
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
                                Formato = "", // Formato padrão
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
        [HttpPost]
        public IActionResult ConfirmarAlteracao()
        {
            // Obter o ID da competição do TempData
            if (TempData.ContainsKey("CompeticaoId") && TempData["CompeticaoId"] != null)
            {
                int competicaoId = Convert.ToInt32(TempData["CompeticaoId"]);
                // Redirecionar para o Menu mantendo o ID da competição
                return RedirectToAction("Index", "Menu", new { competicaoId = competicaoId });
            }
            
            // Se não encontrar o ID, redireciona para a página de competições
            return RedirectToAction("Index", "Competicoes");
        }
    }
}
