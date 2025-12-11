
using CapaData.Data;
using CapaData.DTOs;
using CapaData.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Dapper;

namespace CapaData.Repositorios
{
    public class ClienteRepositorio : ICLienteRepositorio
    {
        private readonly DB_Context _context;

        public ClienteRepositorio(DB_Context context)
        {
            _context = context;
        }

        public async Task<bool> InsertarClienteAsync(ClienteDto cliente)
        {
            var connectionString = _context.Database.GetDbConnection().ConnectionString;

            using var connection = new SqlConnection(connectionString);
            var parametros = new DynamicParameters();

            parametros.Add("@Nombre", cliente.Nombre);
            parametros.Add("@Apellido", cliente.Apellido);
            parametros.Add("@Email", cliente.Email);
            parametros.Add("@Telefono", cliente.Telefono);
            parametros.Add("@FechaRegistro", cliente.FechaRegistro);

            // Aquí declaras el parámetro OUTPUT
            parametros.Add("@Resultado", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("sp_AgregarCliente", parametros, commandType: CommandType.StoredProcedure);

            // Recuperas el valor del parámetro OUTPUT
            bool resultado = parametros.Get<bool>("@Resultado");

            return resultado;
        }

        public async Task<bool> ActualizarClienteAsync(ClienteDto cliente)
        {
            using var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString);

            var parametros = new DynamicParameters();
            parametros.Add("@Id", cliente.Id);
            parametros.Add("@Nombre", cliente.Nombre);
            parametros.Add("@Apellido", cliente.Apellido);
            parametros.Add("@Email", cliente.Email);
            parametros.Add("@Telefono", cliente.Telefono);
            parametros.Add("@FechaRegistro", cliente.FechaRegistro);
            parametros.Add("@Resultado", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("sp_EditarCliente", parametros, commandType: CommandType.StoredProcedure);

            return parametros.Get<bool>("@Resultado");
        }

        public async Task<ClienteDto> ObtenerClientePorIdAsync(int id)
        {
            using var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString);

            var parametros = new DynamicParameters();
            parametros.Add("@Id", id);

            return await connection.QueryFirstOrDefaultAsync<ClienteDto>(
                "sp_ObtenerClientePorId",
                parametros,
                commandType: CommandType.StoredProcedure
            );
        }

        //public async Task<bool> InsertarClienteAsync(ClienteDto clienteDto)
        //{
        //    // Mapear DTO a entidad
        //    var cliente = new Cliente
        //    {
        //        Nombre = clienteDto.Nombre,
        //        Apellido = clienteDto.Apellido,
        //        Email = clienteDto.Email,
        //        Telefono = clienteDto.Telefono,
        //        FechaRegistro = clienteDto.FechaRegistro
        //    };

        //    // Agregar la entidad al DbSet
        //    _context.Clientes.Add(cliente);

        //    // Guardar cambios en la base de datos
        //    var result = await _context.SaveChangesAsync();
        //    return result > 0;
        //}

    }
}
