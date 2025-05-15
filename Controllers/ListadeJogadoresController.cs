﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using System.Threading.Tasks;

namespace ProjetoLaboratorio25.Controllers
{
    public class ListadeJogadoresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ListadeJogadoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SalvarCompeticao(string[] jogadores)
        {
            // Recuperar dados da competição do TempData ou localStorage
            string nome = TempData["NomeCompeticao"]?.ToString();
            string tipo = TempData["TipoCompeticao"]?.ToString();

            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(tipo))
            {
                TempData["Erro"] = "Dados da competição não encontrados. Por favor, inicie o processo novamente.";
                return RedirectToAction("Index", "CriarCompeticao");
            }

            // Recuperar configurações das fases
            var configuracoesFase = CriteriosdePontuacaoController._configuracoes;

            // Criar nova competição
            var competicao = new Competicao
            {
                Nome = nome,
                TipoCompeticao = tipo,
                NumJogadores = jogadores?.Length ?? 0,
                NumEquipas = tipo == "equipas" ? jogadores?.Length ?? 0 : 0,
                PontosVitoria = 3, // Valores padrão
                PontosEmpate = 1   // Valores padrão
            };

            // Adicionar competição ao contexto
            _context.Competicoes.Add(competicao);
            await _context.SaveChangesAsync();

            // Adicionar configurações de fase
            foreach (var config in configuracoesFase)
            {
                config.CompeticaoId = competicao.Id;
                _context.ConfiguracoesFase.Add(config);
            }

            await _context.SaveChangesAsync();

            // Limpar dados temporários
            CriteriosdePontuacaoController._configuracoes.Clear();
            TempData.Remove("NomeCompeticao");
            TempData.Remove("TipoCompeticao");

            TempData["Sucesso"] = "Competição criada com sucesso!";
            return RedirectToAction("Index", "Competicoes");
        }
    }
}
