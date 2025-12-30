
using CapaAplicacion;
using CapaData.DTOs;
using CapaData.Entities;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace PracticaIdentity.Controllers
{
    public class ImportarPromosController : Controller
    {
        private readonly ILogger<ImportarPromosController> _logger;
        private readonly Dependencias _dependencias;

        public ImportarPromosController(ILogger<ImportarPromosController> logger, Dependencias dependencias)
        {
            _logger = logger;
            _dependencias = dependencias;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            // Lista de promociones desde el servicio (DTO)
            var promos = await _dependencias.PromocionServices.ObtenerTodosAsync(ct);
            return View(promos); // IEnumerable<TblPromocion>
        }

        [HttpPost]
        [RequestSizeLimit(104857600)] // 100 MB
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Microsoft.AspNetCore.Http.IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Selecciona un archivo Excel válido (.xlsx).";
                var current = await _dependencias.PromocionServices.ObtenerTodosAsync(ct);
                return View(current);
            }

            // Guardar archivo en disco (opcional, auditoría)
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsPath);
            var savedPath = Path.Combine(uploadsPath, $"promotions_{DateTimeOffset.Now:yyyyMMdd_HHmmss}.xlsx");
            await using (var stream = new FileStream(savedPath, FileMode.Create))
                await file.CopyToAsync(stream, ct);

            int ok = 0, updated = 0, created = 0, errors = 0;

            try
            {
                using var wb = new XLWorkbook(savedPath);
                var ws = wb.Worksheets.First();
                var range = ws.RangeUsed();
                var rows = range.RowsUsed().Skip(1); // Fila 1: ProductSku, DiscountPercent, StartUtc, EndUtc

                foreach (var row in rows)
                {
                    var sku = row.Cell(1).GetString().Trim();
                    var discountStr = row.Cell(2).GetString().Trim();
                    var startStr = row.Cell(3).GetString().Trim();
                    var endStr = row.Cell(4).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(sku)) { errors++; continue; }

                    // Parseo de porcentaje
                    decimal discount;

                    // 1️⃣ Normalizar separador decimal
                    string normalized = discountStr
                        .Trim()
                        .Replace(" ", "")
                        .Replace(",", ".");

                    // 2️⃣ Parsear de forma segura
                    if (!decimal.TryParse(
                            normalized,
                            NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                            CultureInfo.InvariantCulture,
                            out discount))
                    {
                        errors++;
                        continue;
                    }


                    // Parseo de fechas (UTC)
                    if (!DateTime.TryParse(startStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var startUtc) &&
                        !DateTime.TryParse(startStr, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out startUtc))
                    { errors++; continue; }

                    if (!DateTime.TryParse(endStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var endUtc) &&
                        !DateTime.TryParse(endStr, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out endUtc))
                    { errors++; continue; }

                    // Validaciones negocio
                    if (discount < 0 || discount > 100) { errors++; continue; }
                    if (endUtc <= startUtc) { errors++; continue; }

                    // Upsert vía servicio (por SKU)
                    var existing = await _dependencias.PromocionServices.ObtenerPorSKU_Async(sku, ct);
                    var dto = new TblPromocion
                    {
                        ProductSku = sku,
                        DiscountPercent = discount,
                        StartUtc = startUtc,
                        EndUtc = endUtc
                    };

                    if (existing is null)
                    {
                        await _dependencias.PromocionServices.AgregarAsync(dto, ct); // INSERT
                        created++; ok++;
                    }
                    else
                    {
                        dto.Id = existing.Id;
                        await _dependencias.PromocionServices.ActualizarAsync(dto, ct); // UPDATE
                        updated++; ok++;
                    }
                }

                TempData["Success"] = $"Importación promociones: {ok} OK, {created} nuevas, {updated} actualizadas, {errors} con error.";
                _logger.LogInformation("Importación promociones: OK={ok}, nuevas={created}, actualizadas={updated}, errores={errors}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando Excel de promociones");
                TempData["Error"] = $"Error procesando Excel: {ex.Message}";
            }

            var list = await _dependencias.PromocionServices.ObtenerTodosAsync(ct);
            return View(list); // IEnumerable<TblPromocion>
        }
    }
}
