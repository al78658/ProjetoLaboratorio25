﻿﻿﻿﻿﻿﻿﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ProjetoLaboratorio25.Controllers
{
    public class ListadeJogadoresController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> ImportarExcel(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                return BadRequest("Arquivo não fornecido ou vazio");
            }

            try
            {
                using (var reader = new StreamReader(arquivo.OpenReadStream()))
                {
                    string conteudo = await reader.ReadToEndAsync();
                    string[] linhas = conteudo.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    List<JogadorViewModel> jogadores = new List<JogadorViewModel>();
                    
                    foreach (var linha in linhas)
                    {
                        string[] dados = linha.Split(' ');
                        if (dados.Length >= 5)
                        {
                            // Formato esperado: Nome Sobrenome Codigo Data-Nascimento Categoria Clube
                            string nome = string.Join(" ", dados.Take(dados.Length - 4));
                            string codigo = dados[dados.Length - 4];
                            string dataNascimentoStr = dados[dados.Length - 3];
                            string categoria = dados[dados.Length - 2];
                            string clube = dados[dados.Length - 1];
                            
                            if (DateTime.TryParseExact(dataNascimentoStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dataNascimento))
                            {
                                var jogador = new JogadorViewModel
                                {
                                    Nome = nome,
                                    Codigo = codigo,
                                    DataNascimento = dataNascimento,
                                    Categoria = categoria,
                                    Clube = clube
                                };
                                
                                jogadores.Add(jogador);
                            }
                        }
                    }
                    
                    // Armazenar na sessão
                    HttpContext.Session.SetString("Jogadores", System.Text.Json.JsonSerializer.Serialize(jogadores));
                    
                    return Ok(new { success = true, jogadores });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao importar jogadores: {ex.Message}");
            }
        }
    }
    
    public class JogadorViewModel
    {
        public string Nome { get; set; }
        public string Codigo { get; set; }
        public DateTime DataNascimento { get; set; }
        public string Categoria { get; set; }
        public string Clube { get; set; }
    }
}
