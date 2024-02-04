using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared.Common.Interfaces;
using Shared.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Shared.Common
{
    public class BaseAuthService: IBaseAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public BaseAuthService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetUserRole()
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"];
            return user != null
                ? user.GetType().GetProperty("role")?.GetValue(user, null)?.ToString()
                : string.Empty;
        }

        private string GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.Items["User"];
            return user.GetType().GetProperty("id")?.GetValue(user, null).ToString();
        }

        protected bool IsAdmin()
        {
            return GetUserRole().Equals(RoleEnum.ADMIN.ToString());
        }

        protected bool IsUser()
        {
            return GetUserRole().Equals(RoleEnum.USER.ToString());
        }

        public int GetCurrentUserId()
        {
            return int.Parse(GetUserId());
        }

        public static ClaimsPrincipal ValidateJSONWebToken(string token, IConfiguration configuration)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Jwt:Secret"]));
            try
            {
                var validationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    IssuerSigningKey = securityKey,
                    ValidateLifetime = false
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken securityToken);
                return principal;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
