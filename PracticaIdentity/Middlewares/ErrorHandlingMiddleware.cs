using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PracticaIdentity.Middlewares
{



    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var correlationId = context.TraceIdentifier;
            var userName = context.User?.Identity?.IsAuthenticated == true ? context.User.Identity.Name : "anonymous";

            // Log detallado
            _logger.LogError(ex,
                "Error en la aplicación | Método: {Method} | Ruta: {Path} | Usuario: {User} | TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                userName,
                correlationId);

            // Construir ProblemDetails
            var problem = new ProblemDetails
            {
                Title = "Error interno en el servidor",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "Ocurrió un error inesperado. Contacte soporte con el TraceId.",
                Instance = context.Request.Path,
                Type = "https://httpstatuses.com/500"
            };

            problem.Extensions["traceId"] = correlationId;
            problem.Extensions["method"] = context.Request.Method;
            problem.Extensions["user"] = userName;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
        }
    }

}
