using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Models;

namespace ProjetoLaboratorio25.Controllers
{
    public class CriarRelatorioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
