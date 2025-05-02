using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Models;
using System.Collections.Generic;
using System.Linq;

namespace ProjetoLaboratorio25.Controllers
{
    public class CriteriosdePontuacaoController : Controller
    {
        private static List<ConfiguracaoFase> _configuracoes = new List<ConfiguracaoFase>();

        [HttpGet]
        public IActionResult Index(int faseNumero)
        {
            // Busca a configuração da fase ou cria uma nova
            var configuracao = _configuracoes.FirstOrDefault(c => c.FaseNumero == faseNumero);
            if (configuracao == null)
            {
                configuracao = new ConfiguracaoFase { FaseNumero = faseNumero };
            }

            ViewBag.FaseNumero = faseNumero;
            ViewBag.Configuracao = configuracao;
            return View();
        }

        [HttpPost]
        public IActionResult Index(int faseNumero, string formato, int vitoria, int empate, int derrota, List<string> criterios)
        {
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
    }
}
