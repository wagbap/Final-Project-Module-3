using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccessLayer.Filters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PrivilegeUser : ActionFilterAttribute
    {
        private readonly string[] _userTypes;

        public PrivilegeUser(params string[] userTypes)
        {
            _userTypes = userTypes;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var secretKey = config["Jwt:Key"];
            var validIssuer = config["Jwt:Issuer"];
            var validAudience = config["Jwt:Audience"];

            string authorizationHeader = context.HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            string token = authorizationHeader.Substring("Bearer ".Length).Trim();
            if (!ValidateToken(token, secretKey, validIssuer, validAudience))
            {
                context.Result = new UnauthorizedResult();
            }

            base.OnActionExecuting(context);
        }

        private bool ValidateToken(string token, string secretKey, string validIssuer, string validAudience)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters(secretKey, validIssuer, validAudience);

            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var userTypeClaim = claimsPrincipal.Claims.FirstOrDefault(claim => claim.Type == "UserType");

                if (userTypeClaim != null && _userTypes.Contains(userTypeClaim.Value))
                {
                    return true;
                }
            }
            catch (Exception ex) when (ex is SecurityTokenException || ex is ArgumentException)
            {
                Console.WriteLine(ex);
            }

            return false;
        }

        private TokenValidationParameters GetValidationParameters(string secretKey, string validIssuer, string validAudience)
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = validAudience,
                ValidIssuer = validIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        }
    }
}
