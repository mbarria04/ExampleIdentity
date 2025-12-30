using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaData.DTOs;

namespace CapaAplicacion.Interfaces
{
    public interface IProducto
    {

        Task<List<TblProducto>> ObtenerTodosAsync(CancellationToken ct = default);
        Task<TblProducto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
        Task AgregarAsync(TblProducto dto, CancellationToken ct = default);
        Task ActualizarAsync(TblProducto dto, CancellationToken ct = default);
        Task EliminarAsync(int id, CancellationToken ct = default);

    }
}
