using CapaData.Data;
using CapaData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CapaData.Interfaces;

namespace CapaData.Repositorios
{
    public class PromotionRepositorio : IPromotionRepositorio
    {
        private readonly DB_Context _db;

        public PromotionRepositorio(DB_Context db)
        {
            _db = db;
        }

        public Task<List<Promotion>> ListarAsync(CancellationToken ct = default)
        {
            return _db.Promotions
                      .AsNoTracking()
                      .OrderBy(p => p.ProductSku)
                      .ToListAsync(ct);
        }

        public Task<Promotion?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        {
            return _db.Promotions
                      .AsNoTracking()
                      .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public Task<Promotion?> ObtenerPorSKU_Async(string name, CancellationToken ct = default)
        {
            return _db.Promotions
                      .AsNoTracking()
                      .FirstOrDefaultAsync(p => p.ProductSku == name, ct);
        }

        public async Task AgregarAsync(Promotion entity, CancellationToken ct = default)
        {
            await _db.Promotions.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task ActualizarAsync(Promotion entity, CancellationToken ct = default)
        {
            _db.Promotions.Update(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task EliminarAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.Promotions.FindAsync(new object?[] { id }, ct);
            if (entity is null) return;
            _db.Promotions.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }

        public Task<bool> ExisteSkuAsync(string sku, CancellationToken ct = default)
        {
            return _db.Promotions
                      .AsNoTracking()
                      .AnyAsync(p => p.ProductSku == sku, ct);
        }

        public Task<Promotion?> ObtenerPorSkuAsync(string sku, CancellationToken ct = default)
        {
            return _db.Promotions
                      .AsNoTracking()
                      .FirstOrDefaultAsync(p => p.ProductSku == sku, ct);
        }

        public async Task<(List<Promotion> Items, int Total)> BuscarAsync(
            string? texto, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Promotions.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                var t = texto.Trim();
                query = query.Where(p => p.ProductSku.Contains(t));
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(p => p.ProductSku)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }



        public Task<int> MarcarCompletadoAsync(Guid jobId, CancellationToken ct)
        {
            return _db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE dbo.Jobs
                   SET State = {"Completed"},
                       Error = {(string?)null}
                 WHERE Id = {jobId}
                   AND State = {"Processing"}", ct);
        }

        public Task<int> MarcarFallidoAsync(Guid jobId, string error, CancellationToken ct)
        {
            return _db.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE dbo.Jobs
                   SET State = {"Failed"},
                       Error = {error}
                 WHERE Id = {jobId}", ct);
        }
    


     }
}
