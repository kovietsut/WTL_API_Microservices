using Contracts.Common.SeedWork;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shared.Common;
using Shared.DTOs;
using Shared.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Infrastructure.Middlewares
{
    public class JWTMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ErrorCode _errorCodes;

        public JWTMiddleware(RequestDelegate next, IConfiguration configuration, IOptions<ErrorCode> errorCodes)
        {
            _next = next;
            _configuration = configuration;
            _errorCodes = errorCodes.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //identity user for notifications
            if (context.Request.Path.Value.StartsWith("/notification"))
            {
                string bearerToken = context.Request.Query["access_token"];
                if (bearerToken != null)
                {
                    string[] authorization = { "Bearer " + bearerToken };
                    context.Request.Headers.Add("Authorization", authorization);
                }
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null)
            {
                AttachAccountToContext(context, token);
                await _next(context);
                return;
            }
            //check if current user have permission to access this resources
            string[] specialPathWithoutToken =
            {
            PathWithoutToken.USER_LOGIN,
            PathWithoutToken.USER_SIGNUP,
            PathWithoutToken.REFRESH_TOKEN,
            PathWithoutToken.MANGA_PATH,
            PathWithoutToken.CHAPTER_PATH,
            PathWithoutToken.GENRE_PATH,
            PathWithoutToken.COMMENT_PATH
        };
            string[] regexSpecification = { "", "", "", "", "", "", "", "", "" };
            var index = 0;
            //current path
            var api = context.Request.Path.ToString();
            Regex rgx;
            var path = specialPathWithoutToken.FirstOrDefault(value =>
            {
                rgx = new Regex($"^/.*/{value}/?{regexSpecification[index]}");
                bool isMatch = rgx.IsMatch(api);
                index++;
                if (isMatch)
                {
                    //if (!context.Request.Method.Equals("GET")) return false;
                    return true;
                }

                return false;
            });
            if (api.StartsWith(PathWithoutToken.NOTIFICATION) || !string.IsNullOrEmpty(path))
            {
                await _next(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                Status = StatusCodes.Status401Unauthorized,
                IsSuccess = false,
                Code = _errorCodes.Status401.Unauthorized,
                Message = "Unauthorized"
            });
        }

        private void AttachAccountToContext(HttpContext context, string token)
        {
            try
            {
                var jwt = BaseAuthService.ValidateJSONWebToken(token, _configuration);
                var expiredTimeString = jwt.Claims.First(x => x.Type == "exp").Value;
                var date = Util.TimeStampToDateTime(long.Parse(expiredTimeString));
                var expiredTime = date - DateTime.Now;
                var expired = expiredTime.TotalMinutes < 0;
                // if (expired) throw new UnauthorizedAccessException();
                if (expired)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.WriteAsJsonAsync(new
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        IsSuccess = false,
                        Code = _errorCodes.Status401.Unauthorized,
                        Message = "Token Expired"
                    });
                    return;
                }
                var accountId = jwt.Claims.First(x => x.Type == "Id").Value;
                var email = jwt.Claims.First(x => x.Type == "Email").Value;
                var role = jwt.Claims.First(x => x.Type.Contains("Role")).Value;
                context.Items["User"] = new { id = accountId, email, role };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
