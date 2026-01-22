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
        Task<ClienteDto> ObtenerClientePorIdAsync(int id);
        Task<bool> ActualizarClienteAsync(ClienteDto cliente);


      
        Task<DataTableResponse<TblProducto>> ListarProductosPaginadoAsync(DataTableRequest request);
        

    }
}
