using CapaAplicacion.Interfaces;
using CapaData.DTOs;
using CapaData.Interfaces;
using CapaData.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaAplicacion.Servicios
{
    public class Cliente : ICliente
    {
        private readonly IMenuRepositorio _menuRepositorio;
        private readonly ICLienteRepositorio _cLienteRepositorio;

        public Cliente(IMenuRepositorio menuRepositorio, ICLienteRepositorio cLienteRepositorio)
        {
            _menuRepositorio = menuRepositorio;
            _cLienteRepositorio = cLienteRepositorio;
        }

        public async Task<bool> AgregarClienteAsync(ClienteDto cliente)
        {
            if (cliente == null) throw new ArgumentNullException(nameof(cliente));
            return await _cLienteRepositorio.InsertarClienteAsync(cliente);
        }

        public async Task<bool> ActualizarClienteAsync(ClienteDto cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            return await _cLienteRepositorio.ActualizarClienteAsync(cliente);
        }

        public async Task<ClienteDto> ObtenerClientePorIdAsync(int id)
        {
            return await _cLienteRepositorio.ObtenerClientePorIdAsync(id);
        }


        public async Task<DataTableResponse<TblProducto>> ListarProductosPaginadoAsync(DataTableRequest request)
        {
            return await _cLienteRepositorio.ListarProductosPaginadoAsync(request);
        }


    }
}
