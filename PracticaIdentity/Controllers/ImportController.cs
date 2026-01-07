using CapaAplicacion;
using CapaData.Data;
using CapaData.DTOs;
using CapaData.Entities;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PracticaIdentity.Controllers
{
    public class ImportController : Controller
    {
        private readonly DB_Context _db;
        private readonly ILogger<ImportController> _logger;
        private readonly Dependencias _dependencias;

        public ImportController(DB_Context db, ILogger<ImportController> logger, Dependencias dependencias)
        {
            _db = db;
            _logger = logger;
            _dependencias = dependencias;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //var products = await _db.Products.OrderBy(p => p.Sku).ToListAsync();
            var products = await _dependencias.ProductoServices.ObtenerTodosAsync();

            var ordered = products.OrderBy(p => p.Sku).ToList();
            return View(ordered);


        }

        [HttpPost]
        [RequestSizeLimit(104857600)] // 100 MB
        public async Task<IActionResult> Index(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Selecciona un archivo Excel válido (.xlsx).";
                var current = await _db.Products.OrderBy(p => p.Sku).ToListAsync();
                return View(current);
            }

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsPath);
            var savedPath = Path.Combine(uploadsPath, $"import_{DateTimeOffset.Now:yyyyMMdd_HHmmss}.xlsx");
            using (var stream = new FileStream(savedPath, FileMode.Create))
                await file.CopyToAsync(stream);

            int ok = 0, updated = 0, created = 0, errors = 0;
            try
            {
                using var wb = new XLWorkbook(savedPath);
                var ws = wb.Worksheets.First();
                var range = ws.RangeUsed();
                var rows = range.RowsUsed().Skip(1); // Fila 1: Sku, Nombre, PrecioBase

                foreach (var row in rows)
                {
                    var sku = row.Cell(1).GetString().Trim();
                    var name = row.Cell(2).GetString().Trim();
                    var priceStr = row.Cell(3).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(priceStr)) priceStr = row.Cell(3).GetValue<string>();

                    if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(name))
                    { errors++; continue; }

                    // Intenta parsear con invariante y luego con cultura local
                    if (!decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var basePrice))
                    {
                        if (!decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.CurrentCulture, out basePrice))
                        { errors++; continue; }
                    }

                    var product = await _db.Products.FirstOrDefaultAsync(p => p.Sku == sku);
                    if (product == null)
                    {
                        product = new Product { Sku = sku, Name = name, BasePrice = basePrice, CurrentPrice = basePrice };
                        await _db.Products.AddAsync(product);
                        created++; ok++;
                    }
                    else
                     {
                        product.Name = name;
                        product.BasePrice = basePrice;
                        product.CurrentPrice = basePrice; // al importar, iguala el actual al base
                        updated++; ok++;
                    }
                }

                await _db.SaveChangesAsync();
                TempData["Success"] = $"Importación completada: {ok} filas OK, {created} nuevas, {updated} actualizadas, {errors} con error.";
                _logger.LogInformation("Importación: OK={ok}, nuevas={created}, actualizadas={updated}, errores={errors}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando Excel");
                TempData["Error"] = $"Error procesando Excel: {ex.Message}";
            }

            var products = await _db.Products
                     .OrderBy(p => p.Sku)
                     .Select(p => new TblProducto
                     {
                                   Id = p.Id,
                                  Sku = p.Sku,
                                 Name = p.Name,
                            BasePrice = p.BasePrice,
                         CurrentPrice = p.CurrentPrice
                     })
                        .ToListAsync();

               return View(products);
        }

    }
}
