using Microsoft.AspNetCore.Mvc;
using CapaData.DTOs; // 👈 aquí estaría tu ClienteDto en la capa Data
using System.Net.Http.Headers;

namespace PracticaIdentity.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ClientesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        public IActionResult Index()
        {
            return View();
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
    }
}