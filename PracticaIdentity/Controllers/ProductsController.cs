using CapaAplicacion;
using CapaData.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace PracticaIdentity.Controllers
{
    public class ProductsController : Controller
    {
       
        private readonly Dependencias _dependencias;
        public ProductsController(Dependencias dependencias) 
        { 
       
            _dependencias = dependencias;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _dependencias.ProductoServices.ObtenerTodosAsync();
            return View(products);
        }
    }
}
