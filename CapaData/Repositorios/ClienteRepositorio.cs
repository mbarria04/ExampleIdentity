
using CapaData.Data;
using CapaData.DTOs;
using CapaData.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;

namespace CapaData.Repositorios
{
    public class ClienteRepositorio : ICLienteRepositorio
    {
        private readonly DB_Context _context;
        private readonly ILogger<ClienteRepositorio> _logger;
        public ClienteRepositorio(DB_Context context, ILogger<ClienteRepositorio> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<bool> InsertarClienteAsync(ClienteDto cliente)
        {
            var connectionString = _context.Database.GetDbConnection().ConnectionString;

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(); // opcional, Dapper abre si hace falta

            var parametros = new DynamicParameters();
            parametros.Add("@Nombre", cliente.Nombre);
            parametros.Add("@Apellido", cliente.Apellido);
            parametros.Add("@Email", cliente.Email);
            parametros.Add("@Telefono", cliente.Telefono);
            parametros.Add("@FechaRegistro", cliente.FechaRegistro);

            // Si mantienes el patrón de OUTPUT:
            parametros.Add("@Resultado", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            try
            {
                // ¡Si el SP falla, Dapper lanzará excepción!
                await connection.ExecuteAsync(
                    "sp_AgregarCliente",
                    parametros,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 30);

                bool resultado = parametros.Get<bool>("@Resultado");
                return resultado;
            }
            catch (SqlException ex)
            {
                // Loguea con el máximo contexto y RE-LANZA
                _logger.LogError(ex,
                    "Error SQL al insertar cliente. Nombre={Nombre}, Email={Email}",
                    cliente.Nombre, cliente.Email);
                throw; // ⬅️ importante para que el middleware lo capture
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al insertar cliente.");
                throw; // ⬅️ deja que el middleware maneje
            }
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



        public async Task<DataTableResponse<TblProducto>> ListarProductosPaginadoAsync(DataTableRequest request)
        {
            using var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString);

            var parametros = new DynamicParameters();
            parametros.Add("@Start", request.Start);
            parametros.Add("@Length", request.Length);
            parametros.Add("@Search", request.Search?.Value);

            using var multi = await connection.QueryMultipleAsync(
                "sp_ListarProductosPaginado",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            // Orden exacto como lo devuelve el SP
            int total = await multi.ReadFirstAsync<int>();
            int filtered = await multi.ReadFirstAsync<int>();
            var data = await multi.ReadAsync<TblProducto>();

            return new DataTableResponse<TblProducto>
            {
                Draw = request.Draw,
                RecordsTotal = total,
                RecordsFiltered = filtered,
                Data = data
            };
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
