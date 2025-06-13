using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Models;
using ProjetoLaboratorio25.Data;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace ProjetoLaboratorio25.Controllers
{
    public class CriteriosdePontuacaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CriteriosdePontuacaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(int faseNumero, string formato, int? competicaoId = null, bool edicao = false, string origin = null)
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
            
            // Armazena a informação de origem (para saber de onde o usuário veio)
            if (!string.IsNullOrEmpty(origin))
            {
                TempData["Origin"] = origin;
            }
            else if (Request.Headers.ContainsKey("Referer"))
            {
                // Se não foi fornecido explicitamente, tenta obter do cabeçalho Referer
                var referer = Request.Headers["Referer"].ToString();
                if (referer.Contains("/FormatodaCompeticao"))
                {
                    TempData["Origin"] = "FormatodaCompeticao";
                }
            }

            // Busca a configuração da fase no banco de dados
            ConfiguracaoFase configuracao = null;
            if (compId > 0)
            {
                configuracao = _context.ConfiguracoesFase
                    .FirstOrDefault(c => c.CompeticaoId == compId && c.FaseNumero == faseNumero);
            }

            // Se não encontrou, cria uma nova configuração
            if (configuracao == null)
            {
                configuracao = new ConfiguracaoFase
                {
                    FaseNumero = faseNumero,
                    Formato = formato,
                    CompeticaoId = compId,
                    NumJogosPorFase = 1, // Valor padrão
                    PontosVitoria = 3,   // Valores padrão
                    PontosEmpate = 1,
                    PontosDerrota = 0,
                    PontosFaltaComparencia = 0,
                    PontosDesclassificacao = 0,
                    PontosExtra = 0
                };
            }

            // Recupera o tipo de competição do TempData, se existir
            string tipoCompeticao = null;
            if (TempData.ContainsKey("TipoCompeticao"))
            {
                tipoCompeticao = TempData["TipoCompeticao"] as string;
                TempData.Keep("TipoCompeticao");
            }
            else if (compId > 0)
            {
                // Tenta buscar do banco de dados
                var comp = _context.Competicoes.FirstOrDefault(c => c.Id == compId);
                if (comp != null)
                {
                    tipoCompeticao = comp.TipoCompeticao;
                    TempData["TipoCompeticao"] = tipoCompeticao;
                    TempData.Keep("TipoCompeticao");
                }
            }

            ViewBag.FaseNumero = faseNumero;
            ViewBag.Configuracao = configuracao;
            ViewBag.FormatoSelecionado = formato;
            ViewBag.TipoCompeticao = tipoCompeticao ?? "Individual";
            ViewBag.CompeticaoId = compId;
            ViewBag.Edicao = edicao;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(int faseNumero, string formato, int vitoria, int empate, int derrota,
            int faltaComparencia = 0, int desclassificacao = 0, int pontosExtra = 0,
            List<string> criterios = null, int numApurados = 0, string tipoSorteio = null,
            int? competicaoId = null, bool edicao = false)
        {
            try
            {
                // 1. Obter o ID da competição
                int compId = 0;
                if (competicaoId.HasValue && competicaoId.Value > 0)
                {
                    compId = competicaoId.Value;
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
                    TempData["Erro"] = "ID da competição não encontrado.";
                    return RedirectToAction("Index", new { faseNumero, formato, edicao });
                }

                // 2. Buscar ou criar configuração
                var configuracao = _context.ConfiguracoesFase
                    .FirstOrDefault(c => c.CompeticaoId == compId && c.FaseNumero == faseNumero);

                if (configuracao == null)
                {
                    // Criar nova configuração
                    configuracao = new ConfiguracaoFase
                    {
                        CompeticaoId = compId,
                        FaseNumero = faseNumero,
                        Formato = formato,
                        NumJogosPorFase = 1, // Valor padrão
                        PontosVitoria = vitoria,
                        PontosEmpate = empate,
                        PontosDerrota = derrota,
                        PontosFaltaComparencia = faltaComparencia,
                        PontosDesclassificacao = desclassificacao,
                        PontosExtra = pontosExtra,
                        CriteriosDesempate = criterios ?? new List<string>()
                    };
                    _context.ConfiguracoesFase.Add(configuracao);
                }
                else
                {
                    // Atualizar configuração existente
                    configuracao.Formato = formato;
                    configuracao.PontosVitoria = vitoria;
                    configuracao.PontosEmpate = empate;
                    configuracao.PontosDerrota = derrota;
                    configuracao.PontosFaltaComparencia = faltaComparencia;
                    configuracao.PontosDesclassificacao = desclassificacao;
                    configuracao.PontosExtra = pontosExtra;
                    configuracao.CriteriosDesempate = criterios ?? new List<string>();
                }

                // 3. Salvar alterações
                _context.SaveChanges();

                // Após guardar, redireciona SEMPRE para FormatodaCompeticao/Index.cshtml com edicao=false
                return RedirectToAction("Index", "FormatodaCompeticao", new { edicao = false });
            }
            catch (Exception ex)
            {
                TempData["Erro"] = "Erro ao salvar configuração: " + ex.Message;
                return RedirectToAction("Index", new { faseNumero, formato, competicaoId, edicao });
            }
        }

        private bool ValidateFormatCriteria(string formato, List<string> criterios)
        {
            if (criterios == null || !criterios.Any())
                return false;

            // Define os critérios válidos para cada formato
            var validCriteria = new Dictionary<string, string[]>
            {
                { "round-robin", new[] { "diferenca-frames", "confronto", "frames-ganhos", "media-tacadas" } },
                { "ave", new[] { "media-ave", "ranking-media" } },
                { "eliminacao", new[] { "fase-alcancada", "pontos-fase" } },
                { "campeonato", new[] { "diferenca-partidas", "confronto", "fair-play", "media-pontuacao" } },
                { "duplo-ko", new[] { "fase-chave", "pontos-chave", "ranking-final" } }
            };

            // Verifica se o formato existe
            if (!validCriteria.ContainsKey(formato))
                return false;

            // Verifica se todos os critérios selecionados são válidos para o formato
            return criterios.All(c => validCriteria[formato].Contains(c));
        }
    }
}