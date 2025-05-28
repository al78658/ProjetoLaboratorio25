using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Data;
using ProjetoLaboratorio25.Models;
using System.Threading.Tasks;

namespace ProjetoLaboratorio25.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiCompeticoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiCompeticoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Competicoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Competicao>> GetCompeticao(int id)
        {
            var competicao = await _context.Competicoes.FindAsync(id);

            if (competicao == null)
            {
                return NotFound();
            }

            return competicao;
        }

        // GET: api/Competicoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Competicao>>> GetCompeticoes()
        {
            return await _context.Competicoes.ToListAsync();
        }
    }
}