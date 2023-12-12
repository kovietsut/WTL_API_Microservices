using Contracts.Domains.Interfaces;
using UserEntity = User.API.Entities.User;

namespace User.API.Repositories.Interfaces
{
    public interface IUserRepository : IRepositoryBase<UserEntity, long>
    {
        Task<UserEntity> GetUserByEmail(string email);
        Task<UserEntity> GetUserById(long id);
        Task CreateUserAsync(UserEntity user);
        Task UpdateUserAsync(UserEntity user);
        Task DeleteUserAsync(long id);
    }
}
