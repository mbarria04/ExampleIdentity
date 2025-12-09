using CapaData.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaAplicacion.Interfaces
{
    public interface ICliente
    {
        Task<bool> AgregarClienteAsync(ClienteDto cliente);
    }
}
