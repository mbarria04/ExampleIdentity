using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace PracticaIdentity.Controllers
{
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                    if (result.Succeeded)
                    {
                        TempData["Success"] = "Rol creado exitosamente.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                TempData["Error"] = "El rol ya existe.";
            }
            else
            {
                TempData["Error"] = "El nombre del rol no puede estar vacío.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["Error"] = "El nombre del rol no puede estar vacío.";
                return RedirectToAction(nameof(Index));
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["Error"] = "Rol no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            role.Name = roleName;
            var result = await _roleManager.UpdateAsync(role);

            TempData[result.Succeeded ? "Success" : "Error"] =
                result.Succeeded ? "Rol actualizado exitosamente." : "Error al actualizar el rol.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role != null)
            {
                var result = await _roleManager.DeleteAsync(role);
                TempData[result.Succeeded ? "Success" : "Error"] =
                    result.Succeeded ? "Rol eliminado exitosamente." : "Error al eliminar el rol.";
            }
            else
            {
                TempData["Error"] = "Rol no encontrado.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}