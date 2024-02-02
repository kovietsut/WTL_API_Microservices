using Microsoft.AspNetCore.Http;
using Shared.Common.Interfaces;
using Shared.Enums;

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
    }
}
