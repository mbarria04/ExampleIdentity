using CapaData.DTOs;
using CapaData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaAplicacion.Interfaces
{
    public interface Ipromocion
    {
        Task<List<TblPromocion>> ObtenerTodosAsync(CancellationToken ct = default);
        Task<TblPromocion?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

        Task<TblPromocion?> ObtenerPorSKU_Async(string name, CancellationToken ct = default);
        Task AgregarAsync(TblPromocion dto, CancellationToken ct = default);
        Task ActualizarAsync(TblPromocion dto, CancellationToken ct = default);
        Task EliminarAsync(int id, CancellationToken ct = default);
    }
}
