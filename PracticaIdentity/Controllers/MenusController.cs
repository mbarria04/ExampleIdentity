using CapaAplicacion;
using CapaData.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PracticaIdentity.Controllers
{
    public class MenusController : Controller
    {
        private readonly Dependencias _dependencias;
        private readonly RoleManager<IdentityRole> _roleManager;

        public MenusController(Dependencias dependencias, RoleManager<IdentityRole> roleManager)
        {
            _dependencias = dependencias;
            _roleManager = roleManager;
        }

        // Listado de menús
        public async Task<IActionResult> Index()
        {
            var menus = await _dependencias.MenuServices.ObtenerTodosAsync();
            ViewBag.Roles = _roleManager.Roles.ToList(); // lista de roles para dropdown
            return View(menus);
        }

        // Crear menú (GET)
        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View();
        }

        // Crear menú (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Menu menu, string[] RolesUsers)
        {
            if (ModelState.IsValid)
            {
                // Guardar roles seleccionados como string separado por comas
                menu.RolesUsers = string.Join(",", RolesUsers);

                await _dependencias.MenuServices.CrearAsync(menu);
                TempData["Success"] = "Menú creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View(menu);
        }

        // Editar menú (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var menu = await _dependencias.MenuServices.ObtenerPorIdAsync(id);
            if (menu == null)
            {
                TempData["Error"] = "Menú no encontrado.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View(menu);
        }

        // Editar menú (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(Menu menu, string[] RolesUsers)
        {
            if (ModelState.IsValid)
            {
                menu.RolesUsers = string.Join(",", RolesUsers);

                await _dependencias.MenuServices.ActualizarAsync(menu);
                TempData["Success"] = "Menú actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Roles = _roleManager.Roles.ToList();
            return View(menu);
        }

        // Eliminar menú
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _dependencias.MenuServices.EliminarAsync(id);
            TempData["Success"] = "Menú eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}