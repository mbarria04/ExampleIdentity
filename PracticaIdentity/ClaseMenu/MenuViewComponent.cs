using CapaData.Entities;
using CapaAplicacion;
using CapaData.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class MenuViewComponent : ViewComponent
{

    private readonly Dependencias _dependencias;
    private readonly UserManager<ApplicationUser> _userManager;

    public MenuViewComponent(Dependencias dependencias, UserManager<ApplicationUser> userManager)
    {
        _dependencias = dependencias;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {

        var usuario = await _userManager.GetUserAsync(HttpContext.User);
        if (usuario == null)
        {
            return View(new List<VM_MenuItems>());
        }

        var rolesUsuario = await _userManager.GetRolesAsync(usuario);

        //OJO SOLO SE HABILITA PARA EL PRIMER INICIO DE SESION Y SE DEBE ASIGANAR EL ROL 
        //if (rolesUsuario == null || !rolesUsuario.Any())
        //{
        //    rolesUsuario = new List<string> { "ADMIN" };
        //    // O simplemente permitir ver los menús sin restricción
        //    //rolesUsuario = new List<string>();
        //}



        // Como solo tiene un rol, tomamos el primero
        var rolUsuario = rolesUsuario.FirstOrDefault();

        var items = await _dependencias.MenuServices.ObtenerTodosAsync();


        var itemsPermitidos = items
            .Where(m => !string.IsNullOrEmpty(m.RolesUsers) &&
                        m.RolesUsers.Split(',')
                            .Select(r => r.Trim().ToUpper())
                            .Contains(rolUsuario.ToUpper()))
            .ToList();





        var currentPath = HttpContext.Request.Path.Value?.ToLower() ?? "";

        var menuRaiz = itemsPermitidos
            .Where(m => m.IdPadre == null)
            .Select(m => new VM_MenuItems
            {
                Id = m.Id,
                DescripcionMenu = m.DescripcionMenu,
                Attach = m.Attach,
                Hijos = itemsPermitidos
                    .Where(h => h.IdPadre == m.Id)
                    .Select(h => new VM_MenuItems
                    {
                        Id = h.Id,
                        DescripcionMenu = h.DescripcionMenu,
                        Attach = h.Attach,
                        IsActive = currentPath.Contains(h.Attach?.ToLower() ?? "")
                    }).ToList(),
                IsActive = itemsPermitidos.Any(h => h.IdPadre == m.Id && currentPath.Contains(h.Attach?.ToLower() ?? "")),
                Iconos = m.Iconos
            }).ToList();




        return View(menuRaiz);

    }
}
