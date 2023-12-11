using UserEntity = User.API.Entities.User;

namespace User.API.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<UserEntity> GetUserByEmail(string email);
        Task<UserEntity> GetUserById(long id);
    }
}
