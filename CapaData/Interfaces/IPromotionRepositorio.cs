using CapaData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaData.Interfaces
{
    public interface IPromotionRepositorio
    {
        Task<List<Promotion>> ListarAsync(CancellationToken ct = default);
        Task<Promotion?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

        Task<Promotion?> ObtenerPorSKU_Async(string name, CancellationToken ct = default);

        // Escritura
        Task AgregarAsync(Promotion entity, CancellationToken ct = default);
        Task ActualizarAsync(Promotion entity, CancellationToken ct = default);
        Task EliminarAsync(int id, CancellationToken ct = default);

        // (Opcional) Utilidades comunes
        Task<bool> ExisteSkuAsync(string sku, CancellationToken ct = default);
        Task<Promotion?> ObtenerPorSkuAsync(string sku, CancellationToken ct = default);

        // (Opcional) Paginación y filtros
        Task<(List<Promotion> Items, int Total)> BuscarAsync(
            string? texto,             // busca por Sku/Name si lo implementas
            int page,                  // 1-based
            int pageSize,
            CancellationToken ct = default
        );
        Task<int> MarcarCompletadoAsync(Guid jobId, CancellationToken ct);
        Task<int> MarcarFallidoAsync(Guid jobId, string error, CancellationToken ct);

    }
}

