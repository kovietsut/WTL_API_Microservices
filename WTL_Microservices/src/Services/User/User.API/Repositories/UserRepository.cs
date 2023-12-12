using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using UserEntity = User.API.Entities.User;
using User.API.Persistence;
using User.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace User.API.Repositories
{
    public class UserRepository : RepositoryBase<UserEntity, long, IdentityContext>, IUserRepository
    {
        public UserRepository(IdentityContext dbContext, IUnitOfWork<IdentityContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }

        public Task<UserEntity> GetUserByEmail(string email) =>
            FindByCondition(x => x.Email.Equals(email)).SingleOrDefaultAsync();
            
        public Task<UserEntity> GetUserById(long id) =>
            FindByCondition(x => x.Id == id).SingleOrDefaultAsync();

        public Task CreateUserAsync(UserEntity user) => CreateAsync(user);

        public Task UpdateUserAsync(UserEntity user) => UpdateAsync(user);

        public async Task DeleteUserAsync(long id)
        {
            var user = await GetUserById(id);
            if(user != null)
            {
                user.IsEnabled = false;
                await UpdateAsync(user);
            }
        }
    }
}
