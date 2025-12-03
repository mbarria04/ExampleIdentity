using Microsoft.AspNetCore.Identity;

namespace CapaData.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // Agregar propiedades personalizadas si es necesario
        public string NombreCompleto { get; set; }
    }
}
