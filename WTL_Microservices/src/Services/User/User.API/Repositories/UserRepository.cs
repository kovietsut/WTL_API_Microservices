using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using UserEntity = User.API.Entities.User;
using User.API.Persistence;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class UserRepository : RepositoryBase<UserEntity, long, IdentityContext>, IUserRepository
    {
        public UserRepository(IdentityContext dbContext, IUnitOfWork<IdentityContext> unitOfWork) : base(dbContext, unitOfWork)
        {
        }
    }
}
