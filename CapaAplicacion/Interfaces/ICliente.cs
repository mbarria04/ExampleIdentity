using CapaData.DTOs;
using CapaAplicacion.DTOs;
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
        Task<ClienteDTOs> ObtenerClientePorIdAsync(int id);
        Task<bool> ActualizarClienteAsync(ClienteDTOs cliente);


      
        Task<DataTableResponseDto<ProductoDTOs>> ListarProductosPaginadoAsync(DataTableRequestDto request);
        

    }
}
