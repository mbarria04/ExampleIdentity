using CapaAplicacion.Interfaces;
using CapaData.DTOs;
using CapaData.Interfaces;
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

    }
}
