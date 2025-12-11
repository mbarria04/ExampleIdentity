using CapaAplicacion;
using CapaData.DTOs; // 👈 aquí estaría tu ClienteDto en la capa Data
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;


namespace PracticaIdentity.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Dependencias _dependencias;
        private readonly IConfiguration _config;
        public ClientesController(IHttpClientFactory httpClientFactory, Dependencias dependencias, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _dependencias = dependencias;
            _config = config;
        }


        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerClientes(string cedula)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            // 1. Pedir token a API1
            var tokenResponse = await httpClient
                .GetFromJsonAsync<Dictionary<string, string>>(
                    "https://localhost:7183/api/jwt/generate"
                );

            var token = tokenResponse["token"];

            // 2. Enviar token a API2
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // 3. Construir la URL de la API2
            string url = string.IsNullOrEmpty(cedula)
                ? "https://localhost:7096/api/clientes"
                : $"https://localhost:7096/api/clientes/{cedula}";

            // 4. Consumir API2
            var clientes = await httpClient.GetFromJsonAsync<List<ClienteDto>>(url);

            return Json(clientes);
        }



       







        // Acción que será llamada por Ajax desde la vista
        [HttpGet]
        public async Task<IActionResult> Consultar(string cedula)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            // 1. Pedir token al Identity Provider
            var tokenResponse = await httpClient.PostAsync("https://localhost:5001/connect/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "client_id", "app1-client" },
                    { "client_secret", "superSecret" },
                    { "grant_type", "client_credentials" },
                    { "scope", "app2-api" }
                }));

            var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var token = tokenJson["access_token"];

            // 2. Usar token para llamar a App2
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var clientes = await httpClient.GetFromJsonAsync<List<ClienteDto>>(
                $"https://localhost:7096/api/clientes/{cedula}");

            // Retorna JSON al Ajax
            return Json(clientes);
        }


        [HttpPost]
        public async Task<IActionResult> Agregar([FromBody] ClienteDto cliente)
        {

            if (cliente == null)
                return BadRequest(new { mensaje = "Cliente inválido" });

            // Aquí llamas al servicio de cliente desde Dependencias
            var resultado = await _dependencias._Cliente.AgregarClienteAsync(cliente);

            if (resultado)
                return Ok(new { mensaje = "Cliente agregado correctamente" });
            else
                return StatusCode(500, new { mensaje = "Error al agregar el cliente" });
        }


        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] ClienteDto cliente)
        {
            var existe = await _dependencias._Cliente.ObtenerClientePorIdAsync(cliente.Id);

            if (existe == null)
                return NotFound("El cliente no existe.");

            var actualizado = await _dependencias._Cliente.ActualizarClienteAsync(cliente);

            if (actualizado)
                return Ok(new { mensaje = "Cliente actualizado correctamente" });
            else
                return StatusCode(500, new { mensaje = "Error al actualizar el cliente" });
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var cliente = await _dependencias._Cliente.ObtenerClientePorIdAsync(id);

            if (cliente == null)
                return NotFound();

            return Json(cliente);
        }


    }

 }
