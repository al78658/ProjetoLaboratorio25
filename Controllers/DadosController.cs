﻿﻿using Microsoft.AspNetCore.Mvc;

namespace ProjetoLaboratorio25.Controllers
{
    public class DadosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Detalhes(string id)
        {
            ViewBag.JogadorId = id;
            return View();
        }
    }
}
