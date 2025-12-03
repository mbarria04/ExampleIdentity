using CapaData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaAplicacion.Interfaces
{
    public interface IMenuServices
    {
        Task<List<Menu>> ObtenerTodosAsync();
     
        Task<Menu?> ObtenerPorIdAsync(int id);
        Task CrearAsync(Menu menu);
        Task ActualizarAsync(Menu menu);
        Task EliminarAsync(int id);

    }
}
