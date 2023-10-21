using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Middlewares
{
    using Microsoft.AspNetCore.Builder;

    public static class XXssProtectionMiddlewareExtensions
    {
        public static IApplicationBuilder UseXXssProtection(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<XXssProtectionMiddleware>();
        }
    }

}
