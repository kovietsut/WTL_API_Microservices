using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.DTOs;
using Shared.DTOs.Authentication;
using Shared.SeedWork;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Entity = User.API.Entities.User;
using User.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace User.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationRepository _iAuthRepository;
        private readonly IUserRepository _iUserRepository;
        private readonly ITokenRepository _iTokenRepository;
        private readonly ErrorCode _errorCodes;

        public AuthenticationController(IConfiguration configuration, IAuthenticationRepository authenticationService, IUserRepository iUserRepository,
            ITokenRepository iTokenRepository, IOptions<ErrorCode> errorCodes)
        {
            _configuration = configuration;
            _iAuthRepository = authenticationService;
            _iUserRepository = iUserRepository;
            _iTokenRepository = iTokenRepository;
            _errorCodes = errorCodes.Value;
        }

        [AllowAnonymous]
        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto model)
        {
            var userEntity = await _iUserRepository.GetUserByEmail(model.Email);
            if (userEntity != null) return JsonUtil.Error(StatusCodes.Status404NotFound, null, $"Email {userEntity.Email} is existed");
            var response = await _iAuthRepository.SignUp(model);
            return JsonUtil.Success(response);
        }

        [AllowAnonymous]
        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] SignInDto model)
        {
            var user = await _iUserRepository.GetUserByEmail(model.Email);
            var checkPass = _iAuthRepository.CheckPassword(model, user.SecurityStamp);
            if (user == null || checkPass != user.PasswordHash)
            {
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Incorrect Username or Password");
            }
            var userToken = new UserTokenDto()
            {
                UserId = user.Id,
                Email = user.Email,
            };
            return JsonUtil.Success(new
            {
                UserId = user.Id,
                user.Email,
                user.FullName,
                TokenData = await _iTokenRepository.GenerateToken(userToken)
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RenewToken([FromBody] TokenDto model)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]);
            var tokenValidateParam = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = false
            };
            try
            {
                //check 1: AccessToken valid format
                var tokenInVerification = jwtTokenHandler.ValidateToken(model.AccessToken, tokenValidateParam, out var validatedToken);
                //check 2: Check alg
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase);
                    if (!result)//false
                    {
                        return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Invalid token");
                    }
                }
                //check 3: Check accessToken expire
                var utcExpireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expireDate = _iTokenRepository.ConvertUnixTimeToDateTime(utcExpireDate);
                if (expireDate > DateTime.Now)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Access token has not expired yet");
                }
                //check 4: Check refreshtoken exist in DB
                var storedToken = await _iTokenRepository.GetTokenByRefreshToken(model.RefreshToken);
                if (storedToken == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Refresh token does not exist");
                }
                //check 5: check refreshToken used or revoked?
                if (storedToken.RefreshTokenExpiration <= DateTime.Now)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Refresh token is used");
                }
                if (storedToken.IsRevoked)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Refresh token has been revoked");
                }
                //check 6: AccessToken id == JwtId in RefreshToken
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Token doesn't match");
                }
                //Update token is used
                storedToken.IsEnabled = false;
                await _iTokenRepository.UpdateAsync(storedToken);
                //Create new token
                var user = await _iUserRepository.GetUserById(storedToken.UserId);
                var userModel = new UserTokenDto()
                {
                    UserId = user.Id,
                    Email = user.Email,
                };
                var token = await _iTokenRepository.GenerateToken(userModel);
                return JsonUtil.Success(token);
            }
            catch (Exception e)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, e.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("sign-in-google")]
        public async Task<IActionResult> LoginGoogle([FromBody] GoogleAuthModel model)
        {
            try
            {
                var payload = await _iTokenRepository.VerifyGoogleToken(model);
                if (payload == null) return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Invalid External Authentication.");
                var info = new UserLoginInfo(model.Provider, payload.Subject, model.Provider);
                var user = await _iUserRepository.GetUserByEmail(payload.Email);
                if (user == null)
                {
                    var newUser = new Entity
                    {
                        IsEnabled = true,
                        Email = payload.Email,
                        RoleId = model.RoleId,
                        GoogleUserId = model.UserId,
                        CreatedAt = DateTime.Now
                    };
                    await _iUserRepository.CreateAsync(newUser);
                }
                var userToken = new UserTokenDto()
                {
                    UserId = user.Id,
                    Email = user.Email,
                };
                return JsonUtil.Success(new
                {
                    UserId = user.Id,
                    user.Email,
                    TokenData = await _iTokenRepository.GenerateToken(userToken)
                });
            }
            catch(Exception e)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, e.Message);
            }
        }

        [HttpPut("{userId}/email")]
        public async Task<IActionResult> UpdateEmail(int userId, [FromBody] UpdateEmailDto model)
        {
            try
            {
                return await _iAuthRepository.UpdateEmailUser(userId, model);
            }
            catch (Exception e)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, e.Message);
            }
        }

        [HttpPut("{userId}/change-password")]
        public async Task<IActionResult> ChangePassword(int userId, [FromBody] PasswordDto model)
        {
            try
            {
                return await _iAuthRepository.ChangePassword(userId, model);
            }
            catch (Exception e)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, e.Message);
            }
        }
    }
}
