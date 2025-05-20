using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Models;
using ProjetoLaboratorio25.Data;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

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
        public IActionResult Index(int faseNumero, string formato)
        {
            // Recupera o ID da competição do TempData
            int competicaoId = 0;
            if (TempData.ContainsKey("CompeticaoId") && TempData["CompeticaoId"] != null)
            {
                competicaoId = Convert.ToInt32(TempData["CompeticaoId"]);
                TempData.Keep("CompeticaoId");
            }

            // Busca a configuração da fase no banco de dados
            ConfiguracaoFase configuracao = null;
            if (competicaoId > 0)
            {
                configuracao = _context.ConfiguracoesFase
                    .FirstOrDefault(c => c.CompeticaoId == competicaoId && c.FaseNumero == faseNumero);
            }

            // Se não encontrou, cria uma nova configuração
            if (configuracao == null)
            {
                configuracao = new ConfiguracaoFase 
                { 
                    FaseNumero = faseNumero,
                    Formato = formato,
                    CompeticaoId = competicaoId,
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
            else if (competicaoId > 0)
            {
                // Tenta buscar do banco de dados
                var comp = _context.Competicoes.FirstOrDefault(c => c.Id == competicaoId);
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
            return View();
        }

        [HttpPost]
        public IActionResult Index(int faseNumero, string formato, int vitoria, int empate, int derrota, 
            int faltaComparencia = 0, int desclassificacao = 0, int pontosExtra = 0, 
            List<string> criterios = null, int numApurados = 0, string tipoSorteio = null)
        {
            // Recupera o ID da competição do TempData
            int competicaoId = 0;
            if (TempData.ContainsKey("CompeticaoId") && TempData["CompeticaoId"] != null)
            {
                competicaoId = Convert.ToInt32(TempData["CompeticaoId"]);
                TempData.Keep("CompeticaoId");
            }

            // Validação dos critérios específicos por formato
            if (criterios != null && !ValidateFormatCriteria(formato, criterios))
            {
                TempData["Erro"] = "Critérios inválidos para o formato selecionado.";
                return RedirectToAction("Index", new { faseNumero, formato });
            }

            try
            {
                // Busca a configuração existente ou cria uma nova
                var configuracao = _context.ConfiguracoesFase
                    .FirstOrDefault(c => c.CompeticaoId == competicaoId && c.FaseNumero == faseNumero);

                if (configuracao == null)
                {
                    // Cria uma nova configuração
                    configuracao = new ConfiguracaoFase
                    {
                        CompeticaoId = competicaoId,
                        FaseNumero = faseNumero,
                        Formato = formato,
                        NumJogosPorFase = 1, // Valor padrão, pode ser ajustado conforme necessário
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
                    // Atualiza a configuração existente
                    configuracao.Formato = formato;
                    configuracao.PontosVitoria = vitoria;
                    configuracao.PontosEmpate = empate;
                    configuracao.PontosDerrota = derrota;
                    configuracao.PontosFaltaComparencia = faltaComparencia;
                    configuracao.PontosDesclassificacao = desclassificacao;
                    configuracao.PontosExtra = pontosExtra;
                    configuracao.CriteriosDesempate = criterios ?? new List<string>();
                }

                // Salva as alterações no banco de dados
                _context.SaveChanges();

                TempData["Mensagem"] = "Configuração guardada com sucesso!";
                return RedirectToAction("Index", "FormatodaCompeticao");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = $"Erro ao salvar configuração: {ex.Message}";
                return RedirectToAction("Index", new { faseNumero, formato });
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
