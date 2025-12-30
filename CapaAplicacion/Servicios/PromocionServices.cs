using CapaAplicacion.Interfaces;
using CapaData.DTOs;
using CapaData.Entities;
using CapaData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaAplicacion.Servicios
{
    public class PromocionServices : Ipromocion
    {
        private readonly IPromotionRepositorio _repo;

        public PromocionServices(IPromotionRepositorio repo)
        {
            _repo = repo;
        }

        public async Task<List<TblPromocion>> ObtenerTodosAsync(CancellationToken ct = default)
        {
            var entities = await _repo.ListarAsync(ct);
            return entities.Select(MapToDto).ToList();
        }

        public async Task<TblPromocion?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        {
            var entity = await _repo.ObtenerPorIdAsync(id, ct);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task<TblPromocion?> ObtenerPorSKU_Async(string name, CancellationToken ct = default)
        {
            var entity = await _repo.ObtenerPorSKU_Async(name, ct);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task AgregarAsync(TblPromocion dto, CancellationToken ct = default)
        {
            var entity = MapToEntity(dto);
            await _repo.AgregarAsync(entity, ct);
        }

        public async Task ActualizarAsync(TblPromocion dto, CancellationToken ct = default)
        {
            var entity = MapToEntity(dto);
            await _repo.ActualizarAsync(entity, ct);
        }

        public Task EliminarAsync(int id, CancellationToken ct = default)
            => _repo.EliminarAsync(id, ct);



        // ---- Mappers ----
        private static TblPromocion MapToDto(Promotion e) => new()
        {
            Id = e.Id,
            ProductSku = e.ProductSku,
            DiscountPercent = e.DiscountPercent,
            StartUtc = e.StartUtc,
            EndUtc = e.EndUtc
        };

        private static Promotion MapToEntity(TblPromocion d) => new()
        {
            Id = d.Id,
            ProductSku = d.ProductSku,
            DiscountPercent = d.DiscountPercent,
            StartUtc = d.StartUtc,
            EndUtc = d.EndUtc
        };



    }
}
