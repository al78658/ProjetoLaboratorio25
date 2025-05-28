using Microsoft.AspNetCore.Mvc;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjetoLaboratorio25.Controllers
{
    public class CriarCompeticaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CriarCompeticaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Limpar dados da sessão e TempData
            HttpContext.Session.Remove("CompeticaoId");
            HttpContext.Session.Remove("NomeCompeticao");
            HttpContext.Session.Remove("TipoCompeticao");
            
            // Limpar TempData
            TempData.Remove("CompeticaoId");
            TempData.Remove("NomeCompeticao");
            TempData.Remove("TipoCompeticao");
            
            // Limpar chaves de formatação de fases, se existirem
            var keysToRemove = TempData.Keys
                .Where(k => k.StartsWith("FormatosSelecionados_") || 
                           k.StartsWith("NumFases_") ||
                           k.StartsWith("Fase"))
                .ToList();
            
            foreach (var key in keysToRemove)
            {
                TempData.Remove(key);
            }
            
            // Adicionar um script para limpar o localStorage via JavaScript
            ViewBag.Script = @"
                <script>
                    // Limpar dados da competição anterior
                    localStorage.removeItem('jogadoresList');
                    localStorage.removeItem('jogadoresParaEmparelhar');
                    localStorage.removeItem('formatosSelecionados');
                    localStorage.removeItem('numFases');
                    localStorage.removeItem('nomeCompeticao');
                    localStorage.removeItem('tipoCompeticao');
                    localStorage.removeItem('competicaoId');
                    console.log('Dados da competição anterior removidos do localStorage');
                </script>";
            
            return View();
        }

        [HttpPost]
        public IActionResult Criar(string nome, string tipo)
        {
            // Validar dados do formulário
            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(tipo))
            {
                ModelState.AddModelError("", "Nome e tipo de competição são obrigatórios.");
                return View("Index");
            }

            // Verificar se já existe uma competição com o mesmo nome
            var competicaoExistente = _context.Competicoes
                .FirstOrDefault(c => c.Nome.ToLower() == nome.ToLower());

            if (competicaoExistente != null)
            {
                ModelState.AddModelError("", "Já existe uma competição com este nome.");
                return View("Index");
            }

            // Criar nova competição com dados básicos
            var competicao = new Competicao
            {
                Nome = nome,
                TipoCompeticao = tipo,
                NumJogadores = 2, // Valores padrão
                NumEquipas = 2,   // Valores padrão
                PontosVitoria = 3, // Valores padrão
                PontosEmpate = 1   // Valores padrão
            };

            try
            {
                // Adicionar competição ao contexto
                _context.Competicoes.Add(competicao);
                
                // Salvar mudanças no banco de dados para obter o ID da competição
                _context.SaveChanges();

                // Criar configurações de fase padrão (2 fases iniciais)
                int numFasesPadrao = 2;
                for (int i = 1; i <= numFasesPadrao; i++)
                {
                    var configuracaoFase = new ConfiguracaoFase
                    {
                        CompeticaoId = competicao.Id,
                        FaseNumero = i,
                        NumJogosPorFase = 1, // Valor padrão
                        Formato = "", // Formato padrão
                        PontosVitoria = competicao.PontosVitoria,
                        PontosEmpate = competicao.PontosEmpate,
                        PontosDerrota = 0,
                        PontosFaltaComparencia = 0,
                        PontosDesclassificacao = 0,
                        PontosExtra = 0,
                        CriteriosDesempate = new List<string>() { "confronto" } // Critério padrão
                    };
                    _context.ConfiguracoesFase.Add(configuracaoFase);
                }
                
                // Salvar as configurações de fase
                _context.SaveChanges();

                // Armazenar dados temporários para próxima view
                TempData["NomeCompeticao"] = nome;
                TempData["TipoCompeticao"] = tipo;
                TempData["CompeticaoId"] = competicao.Id;
                TempData["NumFases"] = numFasesPadrao;

                // Redirecionar para configuração de formato com parâmetros explícitos
                return RedirectToAction("Index", "FormatodaCompeticao", new { competicaoId = competicao.Id });
            }
            catch (Exception ex)
            {
                // Em caso de erro, mostrar mensagem e retornar à view
                ModelState.AddModelError("", "Erro ao criar competição: " + ex.Message);
                return View("Index");
            }
        }
    }
}
