﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjetoLaboratorio25.Controllers
{
    public class ListadeJogadoresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ListadeJogadoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? competicaoId = null)
        {
            // Primeiro tenta obter o ID da competição da URL
            if (competicaoId == null)
            {
                // Se não estiver na URL, tenta obter do TempData
                competicaoId = TempData["CompeticaoId"] as int?;
            }
            
            // Se ainda não tiver o ID, tenta buscar a competição mais recente
            if (competicaoId == null)
            {
                var ultimaCompeticao = _context.Competicoes.OrderByDescending(c => c.Id).FirstOrDefault();
                if (ultimaCompeticao != null)
                {
                    competicaoId = ultimaCompeticao.Id;
                }
            }
            
            // Preservar o TempData para uso posterior
            TempData.Keep("NomeCompeticao");
            TempData.Keep("TipoCompeticao");
            TempData.Keep("CompeticaoId");
            TempData.Keep("NumFases");
            TempData.Keep("NumJogosPorFase");
            
            // Passar dados para a view
            ViewBag.CompeticaoId = competicaoId;
            
            if (competicaoId.HasValue)
            {
                var competicao = _context.Competicoes.Find(competicaoId.Value);
                if (competicao != null)
                {
                    ViewBag.NomeCompeticao = competicao.Nome;
                    ViewBag.TipoCompeticao = competicao.TipoCompeticao;
                    
                    // Armazena no TempData também
                    TempData["NomeCompeticao"] = competicao.Nome;
                    TempData["TipoCompeticao"] = competicao.TipoCompeticao;
                    TempData["CompeticaoId"] = competicao.Id;
                }
            }
            
            return View();
        }
        
        [HttpGet]
        public IActionResult ObterJogadores(int competicaoId)
        {
            try
            {
                // Buscar jogadores associados a esta competição
                var jogadores = _context.Jogadores
                    .Where(j => j.CompeticaoId == competicaoId)
                    .Select(j => new JogadorViewModel
                    {
                        Id = j.Id,
                        Nome = j.Nome,
                        Codigo = j.Codigo,
                        DataNascimento = j.DataNascimento,
                        DataNascimentoStr = j.DataNascimento.ToString("yyyy-MM-dd"),
                        Categoria = j.Categoria,
                        Clube = j.Clube
                    })
                    .ToList();
                
                return Json(jogadores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter jogadores: {ex.Message}");
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> SalvarJogadores([FromBody] List<JogadorViewModel> jogadores, int competicaoId)
        {
            try
            {
                if (jogadores == null || jogadores.Count == 0)
                {
                    return BadRequest("Nenhum jogador fornecido");
                }

                var competicao = await _context.Competicoes.FindAsync(competicaoId);
                if (competicao == null)
                {
                    return NotFound("Competição não encontrada");
                }

                var jogadoresExistentes = _context.Jogadores.Where(j => j.CompeticaoId == competicaoId).ToList();
                _context.Jogadores.RemoveRange(jogadoresExistentes);
                await _context.SaveChangesAsync();

                // Correção no método SalvarJogadores para garantir que a propriedade CompeticaoId seja usada corretamente
                var novosJogadores = jogadores.Select(j => new Jogador
                {
                    Nome = j.Nome ?? "",
                    Codigo = j.Codigo ?? "",
                    DataNascimento = DateTime.TryParse(j.DataNascimentoStr, out var data) ? data : DateTime.Now,
                    Categoria = string.IsNullOrEmpty(j.Categoria) ? "N/A" : j.Categoria,
                    Clube = string.IsNullOrEmpty(j.Clube) ? "N/A" : j.Clube,
                    CompeticaoId = competicaoId
                }).ToList();

                await _context.Jogadores.AddRangeAsync(novosJogadores);
                await _context.SaveChangesAsync();

                competicao.NumJogadores = novosJogadores.Count;
                _context.Competicoes.Update(competicao);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, jogadores = novosJogadores.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao salvar jogadores: {ex.Message}");
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Emparelhar(int competicaoId)
        {
            // Armazenar o ID da competição no TempData para persistência
            TempData["CompeticaoId"] = competicaoId;
            
            // Buscar a competição
            var competicao = await _context.Competicoes
                .FirstOrDefaultAsync(c => c.Id == competicaoId);
                
            if (competicao == null)
            {
                return NotFound("Competição não encontrada");
            }
            
            // Buscar jogadores associados a esta competição, evitando referências circulares
            var jogadores = await _context.Jogadores
                .Where(j => j.CompeticaoId == competicaoId)
                .Select(j => new
                {
                    Nome = j.Nome,
                    Clube = j.Clube
                })
                .ToListAsync();
                
            // Armazenar os jogadores no TempData
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
            TempData["JogadoresParaEmparelhar"] = JsonSerializer.Serialize(jogadores, options);
            
            // Redirect to the Emparelhamento page with the competition ID
            return RedirectToAction("Index", "Emparelhamento", new { competicaoId });
        }
    }
    
    public class JogadorViewModel
    {
        public int Id { get; set; }
        public string? Nome { get; set; } = string.Empty;
        public string? Codigo { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
        public string? DataNascimentoStr { get; set; } = string.Empty;
        public string? Categoria { get; set; } = string.Empty;
        public string? Clube { get; set; } = string.Empty;
    }
}
