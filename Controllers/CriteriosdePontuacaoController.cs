using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Models;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ProjetoLaboratorio25.Controllers
{
    public class CriteriosdePontuacaoController : Controller
    {
        public static List<ConfiguracaoFase> _configuracoes = new List<ConfiguracaoFase>();
        private readonly ApplicationDbContext _context;

        public CriteriosdePontuacaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(int faseNumero, string formato)
        {
            // Busca a configuração da fase ou cria uma nova
            var configuracao = _configuracoes.FirstOrDefault(c => c.FaseNumero == faseNumero);
            if (configuracao == null)
            {
                configuracao = new ConfiguracaoFase 
                { 
                    FaseNumero = faseNumero,
                    Formato = formato
                };
            }

            ViewBag.FaseNumero = faseNumero;
            ViewBag.Configuracao = configuracao;
            ViewBag.FormatoSelecionado = formato;
            return View();
        }

        [HttpPost]
        public IActionResult Index(int faseNumero, string formato, int vitoria, int empate, int derrota, List<string> criterios)
        {
            // Validação dos critérios específicos por formato
            if (!ValidateFormatCriteria(formato, criterios))
            {
                TempData["Erro"] = "Critérios inválidos para o formato selecionado.";
                return RedirectToAction("Index", new { faseNumero, formato });
            }

            var configuracao = new ConfiguracaoFase
            {
                FaseNumero = faseNumero,
                Formato = formato,
                PontosVitoria = vitoria,
                PontosEmpate = empate,
                PontosDerrota = derrota,
                CriteriosDesempate = criterios ?? new List<string>()
            };

            // Verifica se a configuração já existe
            var existente = _configuracoes.FirstOrDefault(c => c.FaseNumero == faseNumero);
            if (existente != null)
            {
                // Atualiza a configuração existente
                existente.Formato = configuracao.Formato;
                existente.PontosVitoria = configuracao.PontosVitoria;
                existente.PontosEmpate = configuracao.PontosEmpate;
                existente.PontosDerrota = configuracao.PontosDerrota;
                existente.CriteriosDesempate = configuracao.CriteriosDesempate;
            }
            else
            {
                // Adiciona uma nova configuração
                _configuracoes.Add(configuracao);
            }

            TempData["Mensagem"] = "Configuração salva com sucesso!";
            return RedirectToAction("Index", "FormatodaCompeticao");
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
