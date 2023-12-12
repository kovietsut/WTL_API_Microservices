using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared.DTOs;
using Shared.DTOs.Authentication;
using Shared.SeedWork;
using User.API.Persistence;
using User.API.Repositories.Interfaces;
using UserEntity = User.API.Entities.User;

namespace User.API.Repositories
{
    public class AuthenticationRepository: RepositoryBase<UserEntity, long, IdentityContext>, IAuthenticationRepository
    {
        private readonly IEncryptionRepository _iEncryptionRepository;
        private readonly ErrorCode _errorCodes;
        private readonly IUserRepository _iUserRepository;

        public AuthenticationRepository(IdentityContext dbContext, IUnitOfWork<IdentityContext> unitOfWork, IEncryptionRepository iEncryptionRepository,
            IUserRepository iUserRepository, IOptions<ErrorCode> errorCodes)
            : base(dbContext, unitOfWork)
        {
            _iEncryptionRepository = iEncryptionRepository;
            _iUserRepository = iUserRepository;
            _errorCodes = errorCodes.Value;
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
            user.PasswordHash = _iEncryptionRepository.EncryptPassword(model.Password, user.SecurityStamp);
            return await CreateAsync(user);
        }

        public string CheckPassword(SignInDto model, string securityStamp)
        {
            return _iEncryptionRepository.EncryptPassword(model.Password, securityStamp);
        }

        public async Task<IActionResult> UpdateEmailUser(int userId, UpdateEmailDto model)
        {
            try
            {
                var user = await _iUserRepository.GetByIdAsync(userId);
                if (user == null) return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Usser Doest Not Exist");
                user.ModifiedAt = DateTime.Now;
                user.Email = model.Email.Trim();
                await _iUserRepository.UpdateAsync(user);
                return JsonUtil.Success(user.Id);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> ChangePassword(int userId, PasswordDto model)
        {
            try
            {
                var validator = new PasswordValidator();
                var check = await validator.ValidateAsync(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                var user = await _iUserRepository.GetByIdAsync(userId);
                if (user == null) return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "User Does Not Exist");
                if (user.IsEnabled == false) return JsonUtil.Error(StatusCodes.Status403Forbidden, _errorCodes.Status403.Forbidden, "User Has Blocked");
                var checkPassword = _iEncryptionRepository.EncryptPassword(model.CurrentPassword, user.SecurityStamp);
                if (user.PasswordHash != checkPassword) return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Incorrect Password");
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.PasswordHash = _iEncryptionRepository.EncryptPassword(model.ConfirmPassword, user.SecurityStamp);
                await _iUserRepository.UpdateAsync(user);
                return JsonUtil.Success("Change Password Success");
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }
    }
}
