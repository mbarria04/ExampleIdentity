
using CapaData.DTOs;
using CapaAplicacion.Interfaces;
using CapaData.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CapaData.Interfaces;

namespace CapaAplicacion.Servicios
{
    public class ProductoServices : IProducto
    {
        private readonly IProductoRepositorio _repo;

        public ProductoServices(IProductoRepositorio repo)
        {
            _repo = repo;
        }

        public async Task<List<TblProducto>> ObtenerTodosAsync(CancellationToken ct = default)
        {
            var entities = await _repo.ListarAsync(ct);
            return entities.Select(MapToDto).ToList();
        }

        public async Task<TblProducto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repo.ObtenerPorIdAsync(id, ct);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task AgregarAsync(TblProducto dto, CancellationToken ct = default)
        {
            var entity = MapToEntity(dto);
            await _repo.AgregarAsync(entity, ct);
        }

        public async Task ActualizarAsync(TblProducto dto, CancellationToken ct = default)
        {
            var entity = MapToEntity(dto);
            await _repo.ActualizarAsync(entity, ct);
        }

        public Task EliminarAsync(int id, CancellationToken ct = default)
            => _repo.EliminarAsync(id, ct);


        // ---- Mappers ----
        private static TblProducto MapToDto(Product e) => new()
        {
            Id = e.Id,
            Sku = e.Sku,
            Name = e.Name,
            BasePrice = e.BasePrice,
            CurrentPrice = e.CurrentPrice
        };

        private static Product MapToEntity(TblProducto d) => new()
        {
            Id = d.Id,
            Sku = d.Sku,
            Name = d.Name,
            BasePrice = d.BasePrice,
            CurrentPrice = d.CurrentPrice
        };

    }
}
