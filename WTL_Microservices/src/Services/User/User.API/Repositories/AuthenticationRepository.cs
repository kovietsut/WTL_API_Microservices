using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Shared.DTOs.Authentication;
using User.API.Persistence;
using User.API.Repositories.Interfaces;
using UserEntity = User.API.Entities.User;

namespace User.API.Repositories
{
    public class AuthenticationRepository: RepositoryBase<UserEntity, long, IdentityContext>, IAuthenticationRepository
    {
        private readonly IEncryptionRepository _iEncryptionService;
        public AuthenticationRepository(IdentityContext dbContext, IUnitOfWork<IdentityContext> unitOfWork, IEncryptionRepository iEncryptionService) : base(dbContext, unitOfWork)
        {
            _iEncryptionService = iEncryptionService;
        }

        public async Task<long> SignUp(SignUpDto model)
        {
            UserEntity user = new()
            {
                IsEnabled = true,
                Email = model.Email.Trim(),
                RoleId = model.RoleId,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedAt = DateTimeOffset.UtcNow,
            };
            user.PasswordHash = _iEncryptionService.EncryptPassword(model.Password, user.SecurityStamp);
            return await CreateAsync(user);
        }

        public string CheckPassword(SignInDto model, string securityStamp)
        {
            return _iEncryptionService.EncryptPassword(model.Password, securityStamp);
        }
    }
}
