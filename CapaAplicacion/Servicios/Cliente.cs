using CapaAplicacion.Interfaces;
using CapaData.DTOs; // muy mal esto , rompe la arquitectura
using CapaAplicacion.DTOs;
using CapaData.Entities;
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
            if (cliente == null) 
                throw new ArgumentNullException(nameof(cliente));
            return await _cLienteRepositorio.InsertarClienteAsync(cliente);
        }

        public async Task<bool> ActualizarClienteAsync(ClienteDTOs cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            var clienteEntity = new ClienteData
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Email = cliente.Email,
                Telefono = cliente.Telefono,
                FechaRegistro = cliente.FechaRegistro
            };

            return await _cLienteRepositorio.ActualizarClienteAsync(clienteEntity);
        }

        public async Task<ClienteDTOs> ObtenerClientePorIdAsync(int id)
        {
            var clienteEntity = await _cLienteRepositorio.ObtenerClientePorIdAsync(id);

            if (clienteEntity == null)
                return null;

            //MAPEO Entity -> DTO
            var clienteDto = new ClienteDTOs
            {
                Id = clienteEntity.Id,
                Nombre = clienteEntity.Nombre,
                Apellido = clienteEntity.Apellido,
                Email = clienteEntity.Email,
                Telefono = clienteEntity.Telefono,
                FechaRegistro = clienteEntity.FechaRegistro
            };

            return clienteDto;
        }


        public async Task<DataTableResponseDto<ProductoDTOs>> ListarProductosPaginadoAsync(DataTableRequestDto request)
        {
            // 1️⃣ Mapear Request de Aplicación → Data
            var requestData = new DataTableRequestData
            {
                Draw = request.Draw,
                Start = request.Start,
                Length = request.Length,
                Search = new SearchData
                {
                    Value = request.Search?.Value
                }
            };

            // 2️ Llamar repositorio (Data)
            var resultData = await _cLienteRepositorio.ListarProductosPaginadoAsync(requestData);

            // 3️ MAPEAR Entity → DTo
            var productosDto = resultData.Data.Select(p => new ProductoDTOs
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                BasePrice = p.BasePrice,
                CurrentPrice = p.CurrentPrice
            }).ToList();

            // 4️ Construir Response DTO
            return new DataTableResponseDto<ProductoDTOs>
            {
                Draw = resultData.Draw,
                RecordsTotal = resultData.RecordsTotal,
                RecordsFiltered = resultData.RecordsFiltered,
                Data = productosDto // ✅ ahora sí coincide el tipo
            };
        }



    }
}
