using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;


namespace DataAccessLayer.Middlewares
{
    public class XXssProtectionMiddleware
    {
        private readonly RequestDelegate _next;

        public XXssProtectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Response.Headers.ContainsKey("X-XSS-Protection"))
            {
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            }
            await _next(context);
        }

    }

}
