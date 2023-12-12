using Contracts.Domains.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Authentication;
using UserEntity = User.API.Entities.User;

namespace User.API.Repositories.Interfaces
{
    public interface IAuthenticationRepository : IRepositoryBase<UserEntity, long>
    {
        Task<long> SignUp(SignUpDto model);
        string CheckPassword(SignInDto model, string securityStamp);
        Task<IActionResult> UpdateEmailUser(int userId, UpdateEmailDto model);
        Task<IActionResult> ChangePassword(int userId, PasswordDto model);
    }
}
