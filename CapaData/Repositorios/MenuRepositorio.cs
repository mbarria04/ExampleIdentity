using CapaData.Entities;
using CapaData.Interfaces;
using CapaData.Data;
using Microsoft.EntityFrameworkCore;

namespace CapaData.Repositorios
{
    public class MenuRepositorio : IMenuRepositorio
    {
        private readonly DB_Context _context;

        public MenuRepositorio(DB_Context context)
        {
            _context = context;
        }

        // Listar todos los menús
        public async Task<List<Menu>> ObtenerTodosAsync()
        {
            return await _context.Set<Menu>().ToListAsync();
        }

        // Obtener un menú por Id
        public async Task<Menu?> ObtenerPorIdAsync(int id)
        {
            return await _context.Set<Menu>().FindAsync(id);
        }

        // Crear menú
        public async Task CrearAsync(Menu menu)
        {
            _context.Set<Menu>().Add(menu);
            await _context.SaveChangesAsync();
        }

        // Actualizar menú
        public async Task ActualizarAsync(Menu menu)
        {
            _context.Set<Menu>().Update(menu);
            await _context.SaveChangesAsync();
        }

        // Eliminar menú
        public async Task EliminarAsync(int id)
        {
            var menu = await _context.Set<Menu>().FindAsync(id);
            if (menu != null)
            {
                _context.Set<Menu>().Remove(menu);
                await _context.SaveChangesAsync();
            }
        }
    }
}