
using CapaAplicacion;
using CapaAplicacion.DTOs;
using CapaData.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace PracticaIdentity.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Dependencias _dependencias;
        private readonly IConfiguration _config;

        public ClientesController(
            IHttpClientFactory httpClientFactory,
            Dependencias dependencias,
            IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _dependencias = dependencias;
            _config = config;
        }

        public IActionResult Index() => View();

        /// <summary>
        /// Crea HttpClient con token Bearer válido (solicita/renueva si es necesario).
        /// </summary>
        private async Task<HttpClient> CrearClienteConTokenAsync()
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var token = HttpContext.Session.GetString("JWT");

            // Si no hay token o está por expirar, obtenemos uno nuevo
            if (string.IsNullOrEmpty(token) || DebeRenovar(token))
            {
                token = await ObtenerTokenAsync(client);
                HttpContext.Session.SetString("JWT", token);
            }

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        /// <summary>
        /// Verifica si el JWT está por expirar (margen de 60 segundos).
        /// </summary>
        private bool DebeRenovar(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return true;

            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                var expUtc = jwt.ValidTo; // hora en UTC
                return expUtc <= DateTime.UtcNow.AddSeconds(60);
            }
            catch
            {
                // Si no se puede decodificar, forzamos renovación
                return true;
            }
        }

        /// <summary>
        /// Pide token a la API interna con POST y API Key (X-API-KEY).
        /// Espera JSON: { "token": "..." }
        /// </summary>
        private async Task<string> ObtenerTokenAsync(HttpClient client)
        {
            var baseUrl = _config["ApiClientes:BaseUrl"] ?? string.Empty;
            var tokenPath = _config["ApiClientes:TokenEndpoint"] ?? string.Empty;
            var apiKey = _config["ApiClientes:ApiKey"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(baseUrl) ||
                string.IsNullOrWhiteSpace(tokenPath) ||
                string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Configuración ApiClientes incompleta (BaseUrl, TokenEndpoint o ApiKey).");
            }

            var tokenUrl = CombineUrl(baseUrl, tokenPath);

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            request.Headers.Add("X-API-KEY", apiKey);

            // Si tu endpoint no requiere body, igual enviamos "{}" para cumplir application/json
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"No se pudo obtener el token. Status: {(int)response.StatusCode}. Respuesta: {body}");
            }

            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("token", out var tokenProp))
            {
                throw new InvalidOperationException("La respuesta del endpoint de token no contiene el campo 'token'.");
            }

            var token = tokenProp.GetString();
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("El 'token' recibido está vacío.");

            return token!;
        }

        /// <summary>
        /// Helper para combinar baseUrl + path evitando dobles/ausencia de slash.
        /// </summary>
        private static string CombineUrl(string baseUrl, string path)
        {
            if (string.IsNullOrEmpty(baseUrl)) return path;
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            return baseUrl + path.TrimStart('/');
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerClientes()
        {
            try
            {
                var client = await CrearClienteConTokenAsync();

                var baseUrl = _config["ApiClientes:BaseUrl"]!;
                var clientesEP = _config["ApiClientes:ClientesEndpoint"]!;

                // 👉 api/clientes/ObtenerClientes
                var url = CombineUrl(baseUrl, $"{clientesEP}/ObtenerClientes");

                var clientes = await client.GetFromJsonAsync<List<ClienteDto>>(url);

                return Json(clientes ?? new List<ClienteDto>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al obtener clientes",
                    detalle = ex.Message
                });
            }
        }


        [HttpGet]
        public async Task<IActionResult> Consultar(string NOMBRE)
        {
            try
            {
                var client = await CrearClienteConTokenAsync();

                var baseUrl = _config["ApiClientes:BaseUrl"]!;
                var clientesEP = _config["ApiClientes:ClientesEndpoint"]!;

                var url = CombineUrl(baseUrl, $"{clientesEP}/ConsultarCliente?NOMBRE={NOMBRE}");

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorApi = await response.Content.ReadAsStringAsync();

                    return StatusCode((int)response.StatusCode, new
                    {
                        mensaje = "Error en la API de clientes",
                        detalle = errorApi
                    });
                }

                var clientes = await response.Content.ReadFromJsonAsync<List<ClienteDto>>();

                return Json(clientes ?? new List<ClienteDto>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error en consulta",
                    detalle = ex.Message
                });
            }
        }


        // 🔹 Agregar (lógica local, no llama API interna)
        [HttpPost]
        public async Task<IActionResult> Agregar([FromBody] ClienteDto cliente)
        {
            if (cliente == null)
                return BadRequest(new { mensaje = "Cliente inválido" });

            var resultado = await _dependencias._Cliente.AgregarClienteAsync(cliente);

            return resultado
                ? Ok(new { mensaje = "Cliente agregado correctamente" })
                : StatusCode(500, new { mensaje = "Error al agregar el cliente" });
        }

        // 🔹 Editar (lógica local, no llama API interna)
        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] ClienteDTOs cliente)
        {
            var existe = await _dependencias._Cliente.ObtenerClientePorIdAsync(cliente.Id);

            if (existe == null)
                return NotFound("El cliente no existe.");

            var actualizado = await _dependencias._Cliente.ActualizarClienteAsync(cliente);

            return actualizado
                ? Ok(new { mensaje = "Cliente actualizado correctamente" })
                : StatusCode(500, new { mensaje = "Error al actualizar el cliente" });
        }

        // 🔹 Obtener por id (lógica local)
        [HttpGet]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var cliente = await _dependencias._Cliente.ObtenerClientePorIdAsync(id);

            if (cliente == null)
                return NotFound();

            return Json(cliente);
        }


        [HttpPost]
        public IActionResult ValidarCampos([FromBody] ClienteDTOs cliente)
        {
            TryValidateModel(cliente);

            var errores = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return Ok(errores);
        }

    }
}
