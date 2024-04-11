using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Authentication;

namespace User.API.HttpRepository.Interfaces
{
    public interface IAuthenticateHttpRepository
    {
        Task<IActionResult> AuthenticateAsync(EmailTokenDto model);
    }
}
