using CapaData.Data;
using CapaData.Entities;
using CapaData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CapaData.Repositorios
{

    public class ProductoRepositorio : IProductoRepositorio
    {
        private readonly DB_Context _db;

        public ProductoRepositorio(DB_Context db)
        {
            _db = db;
        }

        public Task<List<Product>> ListarAsync(CancellationToken ct = default)
        {
            return _db.Products
                      .AsNoTracking()
                      .OrderBy(p => p.Name)
                      .ToListAsync(ct);
        }

        public Task<Product?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        {
            return _db.Products
                      .AsNoTracking()
                      .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task AgregarAsync(Product entity, CancellationToken ct = default)
        {
            await _db.Products.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task ActualizarAsync(Product entity, CancellationToken ct = default)
        {
            _db.Products.Update(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task EliminarAsync(int id, CancellationToken ct = default)
        {
            var entity = await _db.Products.FindAsync(new object?[] { id }, ct);
            if (entity is null) return;
            _db.Products.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }

        public Task<bool> ExisteSkuAsync(string sku, CancellationToken ct = default)
        {
            return _db.Products
                      .AsNoTracking()
                      .AnyAsync(p => p.Sku == sku, ct);
        }

        public Task<Product?> ObtenerPorSkuAsync(string sku, CancellationToken ct = default)
        {
            return _db.Products
                      .AsNoTracking()
                      .FirstOrDefaultAsync(p => p.Sku == sku, ct);
        }

        public async Task<(List<Product> Items, int Total)> BuscarAsync(
            string? texto, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                var t = texto.Trim();
                query = query.Where(p => p.Sku.Contains(t) || p.Name.Contains(t));
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }
    }

}
