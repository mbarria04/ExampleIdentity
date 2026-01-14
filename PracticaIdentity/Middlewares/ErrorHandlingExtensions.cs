namespace PracticaIdentity.Middlewares
{

    public static class ErrorHandlingExtensions
    {
        public static IApplicationBuilder UseGlobalErrorHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }

}
