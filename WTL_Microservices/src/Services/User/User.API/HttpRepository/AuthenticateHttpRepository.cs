using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Authentication;
using User.API.Extensions;
using User.API.HttpRepository.Interfaces;

namespace User.API.HttpRepository
{
    public class AuthenticateHttpRepository: IAuthenticateHttpRepository
    {
        private readonly HttpClient _client;

        public AuthenticateHttpRepository(HttpClient client)
        {
            _client = client;
        }

        public async Task<IActionResult> AuthenticateAsync(EmailTokenDto model)
        {
            var result = await _client.PostAsJsonAsync("api/issuetoken/issue-token", model);
            if (result.EnsureSuccessStatusCode().IsSuccessStatusCode)
            {
                var response = await result.ReadContentAs<IActionResult>();
                return response;
            }
            return null;
        }
    }
}
