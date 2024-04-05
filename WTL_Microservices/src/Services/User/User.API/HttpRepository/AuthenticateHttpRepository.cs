using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared.DTOs;
using Shared.DTOs.Authentication;
using User.API.Extensions;
using User.API.HttpRepository.Interfaces;

namespace User.API.HttpRepository
{
    public class AuthenticateHttpRepository: IAuthenticateHttpRepository
    {
        private readonly HttpClient _client;
        private readonly ErrorCode _errorCodes;

        public AuthenticateHttpRepository(HttpClient client, IOptions<ErrorCode> errorCodes)
        {
            _client = client;
            _errorCodes = errorCodes.Value;
        }

        public async Task<IActionResult> AuthenticateAsync(SignInDto model)
        {
            var result = await _client.PostAsJsonAsync("api/authentication/sign-in", model);
            if (result.EnsureSuccessStatusCode().IsSuccessStatusCode)
            {
                var response = await result.ReadContentAs<IActionResult>();
                return response;
            }
            return null;
        }
    }
}
