using CapaData.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace PracticaIdentity.Controllers
{
    public class UserRolesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRolesController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Lista usuarios y sus roles
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserRolesViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Roles = roles
                });
            }

            return View(userRoles);
        }

        // Devuelve roles en JSON para llenar el dropdown
        [HttpGet]
        public async Task<IActionResult> GetRolesDropdown(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Json(new { error = "Usuario no encontrado" });

            var roles = _roleManager.Roles.ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            var result = roles.Select(r => new
            {
                id = r.Id,
                name = r.Name,
                isSelected = userRoles.Contains(r.Name)
            });

            return Json(result);
        }

        // Asignar rol al usuario
        [HttpPost]
        public async Task<IActionResult> Manage(string UserId, string RoleId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var role = await _roleManager.FindByIdAsync(RoleId);
            if (role == null)
            {
                TempData["Error"] = "Rol no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Remover roles actuales (si quieres que solo tenga uno)
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Asignar nuevo rol
            var result = await _userManager.AddToRoleAsync(user, role.Name);

            if (result.Succeeded)
                TempData["Success"] = "Rol asignado exitosamente.";
            else
                TempData["Error"] = "Error al asignar el rol.";

            return RedirectToAction(nameof(Index));
        }
    }
}