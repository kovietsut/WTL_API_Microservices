using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Shared.DTOs.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using User.API.Repositories.Interfaces;

namespace User.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationRepository _iAuthRepository;
        private readonly IUserRepository _iUserRepository;
        private readonly ITokenRepository _iTokenRepository;

        public AuthenticationController(IConfiguration configuration, IAuthenticationRepository authenticationService, IUserRepository iUserRepository,
            ITokenRepository iTokenRepository)
        {
            _configuration = configuration;
            _iAuthRepository = authenticationService;
            _iUserRepository = iUserRepository;
            _iTokenRepository = iTokenRepository;
        }

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDto model)
        {
            var userEntity = await _iUserRepository.GetUserByEmail(model.Email);
            if (userEntity != null) return BadRequest($"Email {userEntity.Email} is existed");
            var response = _iAuthRepository.SignUp(model);
            return Ok(response.Result);
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] SignInDto model)
        {
            var user = await _iUserRepository.GetUserByEmail(model.Email);
            var checkPass = _iAuthRepository.CheckPassword(model, user.SecurityStamp);
            if (user == null || checkPass != user.PasswordHash) return BadRequest("Incorrect Username or Password");
            var userToken = new UserTokenDto()
            {
                UserId = user.Id,
                Email = user.Email,
            };
            return Ok(new
            {
                UserId = user.Id,
                user.Email,
                user.FullName,
                TokenData = _iTokenRepository.GenerateToken(userToken)
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
                        return NotFound("Invalid token");
                    }
                }
                //check 3: Check accessToken expire
                var utcExpireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expireDate = _iTokenRepository.ConvertUnixTimeToDateTime(utcExpireDate);
                if (expireDate > DateTime.Now)
                {
                    return BadRequest("Access token has not expired yet");
                }
                //check 4: Check refreshtoken exist in DB
                var storedToken = await _iTokenRepository.GetTokenByRefreshToken(model.RefreshToken);
                if (storedToken == null)
                {
                    return NotFound("Refresh token does not exist");
                }
                //check 5: check refreshToken used or revoked?
                if (storedToken.RefreshTokenExpiration <= DateTime.Now)
                {
                    return BadRequest("Refresh token is used");
                }
                if (storedToken.IsRevoked)
                {
                    return BadRequest("Refresh token has been revoked");
                }
                //check 6: AccessToken id == JwtId in RefreshToken
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti)
                {
                    return NotFound("Token does not match");
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
                return Ok(token);
            }
            catch (Exception e)
            {
                return BadRequest("Some thing went wrong");
            }
        }
    }
}
