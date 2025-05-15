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
    }
}
