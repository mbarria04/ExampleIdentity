using CapaData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaData.Interfaces
{
    public interface IProductoRepositorio
    {

        // Lectura
        Task<List<Product>> ListarAsync(CancellationToken ct = default);
        Task<Product?> ObtenerPorIdAsync(int id, CancellationToken ct = default);

        // Escritura
        Task AgregarAsync(Product entity, CancellationToken ct = default);
        Task ActualizarAsync(Product entity, CancellationToken ct = default);
        Task EliminarAsync(int id, CancellationToken ct = default);

        // (Opcional) Utilidades comunes
        Task<bool> ExisteSkuAsync(string sku, CancellationToken ct = default);
        Task<Product?> ObtenerPorSkuAsync(string sku, CancellationToken ct = default);

        // (Opcional) Paginación y filtros
        Task<(List<Product> Items, int Total)> BuscarAsync(
            string? texto,             // busca por Sku/Name si lo implementas
            int page,                  // 1-based
            int pageSize,
            CancellationToken ct = default
        );

    }
}
