using Contracts.Domains.Interfaces;
using Shared.DTOs.Authentication;
using UserEntity = User.API.Entities.User;

namespace User.API.Repositories.Interfaces
{
    public interface IAuthenticationRepository : IRepositoryBase<UserEntity, long>
    {
        Task<long> SignUp(SignUpDto model);
        string CheckPassword(SignInDto model, string securityStamp);
    }
}
