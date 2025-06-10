using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using System.Diagnostics;

namespace ProjetoLaboratorio25.Controllers
{
    public class CompeticoesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompeticoesController> _logger;

        public CompeticoesController(ApplicationDbContext context, ILogger<CompeticoesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("=====================================================");
                _logger.LogInformation("Iniciando carregamento de competições");

                // Primeiro, vamos contar quantas competições existem no total
                var totalCompeticoes = await _context.Competicoes.CountAsync();
                _logger.LogInformation($"Total de competições na base de dados: {totalCompeticoes}");

                // Agora vamos contar quantas têm organizador null
                var competicoesNullCount = await _context.Competicoes
                    .Where(c => c.OrganizadorId == null)
                    .CountAsync();
                _logger.LogInformation($"Total de competições sem organizador: {competicoesNullCount}");

                // Carregar todas as competições
                var todasCompeticoes = await _context.Competicoes
                    .AsNoTracking() // Para melhor performance
                    .Include(c => c.Organizador)
                    .ToListAsync();

                _logger.LogInformation($"Competições carregadas: {todasCompeticoes.Count}");
                
                // Log detalhado de cada competição
                foreach (var comp in todasCompeticoes)
                {
                    _logger.LogInformation($"Competição: Id={comp.Id}, Nome={comp.Nome}, OrganizadorId={comp.OrganizadorId ?? -1}");
                }

                var userEmail = HttpContext.Session.GetString("UserEmail");
                var userId = HttpContext.Session.GetInt32("UserId");
                
                _logger.LogInformation($"Usuário atual: Email={userEmail ?? "não logado"}, Id={userId?.ToString() ?? "null"}");

                // CASO 1: Não autenticado → mostrar todas as competições
                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogInformation("Usuário não autenticado - retornando todas as competições");
                    return View(new CompeticoesViewModel
                    {
                        Competicoes = todasCompeticoes,
                        UserEmail = null,
                        IsAdmin = false,
                        UserId = null
                    });
                }

                // CASO 2: Admin → mostrar todas as competições
                var isAdmin = userEmail.ToLower() == "admin@admin.com";
                if (isAdmin)
                {
                    _logger.LogInformation("Usuário admin - retornando todas as competições");
                    return View(new CompeticoesViewModel
                    {
                        Competicoes = todasCompeticoes,
                        UserEmail = userEmail,
                        IsAdmin = true,
                        UserId = userId
                    });
                }

                // CASO 3: Organizador → mostrar apenas as suas competições
                if (userId.HasValue)
                {
                    var competicoesOrganizador = todasCompeticoes
                        .Where(c => c.OrganizadorId == userId.Value)
                        .ToList();

                    _logger.LogInformation($"Organizador (ID: {userId}) - retornando {competicoesOrganizador.Count} competições");
                    return View(new CompeticoesViewModel
                    {
                        Competicoes = competicoesOrganizador,
                        UserEmail = userEmail,
                        IsAdmin = false,
                        UserId = userId
                    });
                }

                // CASO 4: Logado mas sem permissão → nenhuma competição
                _logger.LogInformation("Usuário sem permissões especiais - retornando lista vazia");
                return View(new CompeticoesViewModel
                {
                    Competicoes = new List<Competicao>(),
                    UserEmail = userEmail,
                    IsAdmin = false,
                    UserId = userId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao carregar competições: {ex.Message}");
                _logger.LogError($"StackTrace: {ex.StackTrace}");
                throw;
            }
            finally
            {
                _logger.LogInformation("=====================================================");
            }
        }

        public async Task<IActionResult> Detalhes(int id)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userId = HttpContext.Session.GetInt32("UserId");

            _logger.LogInformation($"CompeticoesController.Detalhes - ID: {id}, UserEmail: {userEmail}, UserId: {userId}");

            var competicao = await _context.Competicoes
                .Include(c => c.ConfiguracoesFase)
                .Include(c => c.Organizador)
                .Include(c => c.Jogadores)
                .Include(c => c.EmparelhamentosFinal)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (competicao == null)
            {
                _logger.LogInformation("CompeticoesController.Detalhes - Competição não encontrada");
                TempData["Erro"] = "Competição não encontrada.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar acesso
            if (!string.IsNullOrEmpty(userEmail))
            {
                // Admin pode ver tudo
                if (userEmail.Contains("@admin"))
                {
                    _logger.LogInformation("CompeticoesController.Detalhes - Usuário é admin, permitindo acesso");
                    return View(competicao);
                }
                // Organizador só pode ver suas competições
                else if (userId != null && competicao.OrganizadorId == userId)
                {
                    _logger.LogInformation("CompeticoesController.Detalhes - Usuário é o organizador desta competição, permitindo acesso");
                    return View(competicao);
                }
                else
                {
                    _logger.LogInformation("CompeticoesController.Detalhes - Usuário não tem permissão para ver esta competição");
                    TempData["Erro"] = "Você não tem permissão para ver esta competição.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Não logado pode ver qualquer competição
            _logger.LogInformation("CompeticoesController.Detalhes - Usuário não está logado, permitindo acesso");
            return View(competicao);
        }

        [HttpPost]
        public async Task<IActionResult> CriarCompeticao(string nome, string tipo, int numJogadores, int numEquipas)
        {
            var novaCompeticao = new Competicao
            {
                Nome = nome,
                TipoCompeticao = tipo,
                NumJogadores = numJogadores,
                NumEquipas = numEquipas,
                PontosVitoria = 3,
                PontosEmpate = 1
            };

            _context.Competicoes.Add(novaCompeticao);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarConfiguracaoFase(int competicaoId, int faseNumero, string formato, int pontosVitoria, int pontosEmpate, int pontosDerrota)
        {
            var configuracao = new ConfiguracaoFase
            {
                CompeticaoId = competicaoId,
                FaseNumero = faseNumero,
                Formato = formato,
                PontosVitoria = pontosVitoria,
                PontosEmpate = pontosEmpate,
                PontosDerrota = pontosDerrota
            };

            _context.ConfiguracoesFase.Add(configuracao);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult ExisteNome(string nome)
        {
            var existe = _context.Competicoes.Any(c => c.Nome.Trim().ToLower() == nome.Trim().ToLower());
            return Json(existe);
        }

        [HttpPost]
        public async Task<IActionResult> EliminarCompeticao(int id)
        {
            var competicao = await _context.Competicoes.FindAsync(id);
            if (competicao != null)
            {
                _context.Competicoes.Remove(competicao);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpGet]
        public IActionResult ObterIdCompeticao(string nome)
        {
            var competicao = _context.Competicoes
                .FirstOrDefault(c => c.Nome == nome);
            
            if (competicao == null)
            {
                return Json(new { id = 0 });
            }
            
            return Json(new { id = competicao.Id });
        }

        [HttpPost]
        public IActionResult AtualizarTempData(int competicaoId)
        {
            var competicao = _context.Competicoes
                .Include(c => c.ConfiguracoesFase)
                .FirstOrDefault(c => c.Id == competicaoId);
            
            if (competicao == null)
            {
                return NotFound();
            }
            
            // Atualiza o TempData com os dados da competição
            TempData["CompeticaoId"] = competicao.Id;
            TempData["NomeCompeticao"] = competicao.Nome;
            TempData["TipoCompeticao"] = competicao.TipoCompeticao;
            
            // Se houver configurações de fase, atualiza também
            if (competicao.ConfiguracoesFase.Any())
            {
                var formatosDict = competicao.ConfiguracoesFase
                    .OrderBy(c => c.FaseNumero)
                    .ToDictionary(c => c.FaseNumero, c => c.Formato);
                
                TempData[$"FormatosSelecionados_{competicaoId}"] = System.Text.Json.JsonSerializer.Serialize(formatosDict);
                TempData[$"NumFases_{competicaoId}"] = competicao.ConfiguracoesFase.Count;
            }
            
            return Ok();
        }
    }

    public class CompeticoesViewModel
    {
        public IEnumerable<Competicao> Competicoes { get; set; }
        public string UserEmail { get; set; }
        public bool IsAdmin { get; set; }
        public int? UserId { get; set; }
    }
}
