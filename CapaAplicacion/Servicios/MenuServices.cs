using CapaData.Entities;
using CapaAplicacion.Interfaces;
using CapaData.Interfaces;

namespace CapaAplicacion.Servicios
{
    public class MenuServices : IMenuServices  
    {
        private readonly IMenuRepositorio _menuRepositorio;

        public MenuServices(IMenuRepositorio menuRepositorio)
        {
            _menuRepositorio = menuRepositorio;
        }

        public async Task<List<Menu>> ObtenerTodosAsync()
        {
            return await _menuRepositorio.ObtenerTodosAsync();
        }

        public async Task<Menu?> ObtenerPorIdAsync(int id)
        {
            return await _menuRepositorio.ObtenerPorIdAsync(id);
        }

        public async Task CrearAsync(Menu menu)
        {
            await _menuRepositorio.CrearAsync(menu);
        }

        public async Task ActualizarAsync(Menu menu)
        {
            await _menuRepositorio.ActualizarAsync(menu);
        }

        public async Task EliminarAsync(int id)
        {
            await _menuRepositorio.EliminarAsync(id);
        }
    }
}