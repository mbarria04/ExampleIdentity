using CapaAplicacion;
using CapaAplicacion.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace PracticaIdentity.Controllers
{
    public class ListadoProductosController : Controller
    {
        private readonly Dependencias _dependencias;

        public ListadoProductosController(Dependencias dependencias)
        {

            _dependencias = dependencias;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ListarProductos(DataTableRequestDto request)
        {
            var result = await _dependencias._Cliente.ListarProductosPaginadoAsync(request);
            return Json(result); // DataTables lo entiende automáticamente
        }

    }
}
