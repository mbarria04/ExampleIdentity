using CapaData.Entities;
using CapaData.DTOs; // muy mal , solo es entities se debe corregir
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
        Task<ClienteData> ObtenerClientePorIdAsync(int id);
        Task<bool> ActualizarClienteAsync(ClienteData cliente); // ⬅️ AGREGADO


        Task<DataTableResponseData<Product>> ListarProductosPaginadoAsync(DataTableRequestData request);
    }
}
