using CapaData.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaData.Interfaces
{
    public interface ICLienteRepositorio
    {
        Task<bool> InsertarClienteAsync(ClienteDto cliente);
    }
}
