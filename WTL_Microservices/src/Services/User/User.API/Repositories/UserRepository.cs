using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using UserEntity = User.API.Entities.User;
using User.API.Persistence;
using User.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shared.SeedWork;
using Shared.DTOs;
using Microsoft.Extensions.Options;
using Shared.DTOs.Authentication;
using FluentValidation;

namespace User.API.Repositories
{
    public class UserRepository : RepositoryBase<UserEntity, long, IdentityContext>, IUserRepository
    {
        private readonly IEncryptionRepository _iEncryptionRepository;
        private readonly ErrorCode _errorCodes;

        public UserRepository(IdentityContext dbContext, IUnitOfWork<IdentityContext> unitOfWork, IEncryptionRepository iEncryptionRepository,
            IOptions<ErrorCode> errorCodes) 
            : base(dbContext, unitOfWork)
        {
            _iEncryptionRepository = iEncryptionRepository;
            _errorCodes = errorCodes.Value;
        }

        public IActionResult GetList(int? pageNumber, int? pageSize, string? searchText, int? roleId)
        {
            try
            {
                pageNumber ??= 1; pageSize ??= 10;
                var list = FindAll().Include(x => x.Role).Where(x => x.IsEnabled == true && (x.RoleId == roleId || roleId == null) &&
                (searchText == null || x.FullName.Contains(searchText.Trim()) || x.PhoneNumber.Contains(searchText.Trim())
                    || x.Address.Contains(searchText.Trim()) || x.Gender.Contains(searchText.Trim()) || x.Email.Contains(searchText.Trim())))
                    .Select(x => new
                    {
                        UserId = x.Id, x.IsEnabled, x.FullName, x.Email, x.PhoneNumber, x.AvatarPath, x.Gender, x.Address
                    });
                var listData = list.Skip(((int)pageNumber - 1) * (int)pageSize)
                    .Take((int)pageSize).OrderByDescending(x => x.UserId).ToList();
                if (list != null)
                {
                    var totalRecords = list.Count();
                    return JsonUtil.Success(listData, dataCount: totalRecords);
                }
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Empty List Data");
            }
            catch(Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, ex.Message);
            }
        }

        public Task<UserEntity> GetUserByEmail(string email) =>
            FindByCondition(x => x.Email.Equals(email)).SingleOrDefaultAsync();
            
        public Task<UserEntity> GetUserById(long id) =>
            FindByCondition(x => x.Id == id).SingleOrDefaultAsync();

        public async Task<IActionResult> CreateUserAsync(CreateUserDto model)
        {
            try
            {
                // Validator
                var validator = new CreateUserValidator();
                var check = await validator.ValidateAsync(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                var userByEmail = await GetUserByEmail(model.Email);
                if (userByEmail != null && userByEmail.Email != null) {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Email existed");
                }
                var userByPhoneNumber = FindByCondition(x => x.PhoneNumber.Equals(model.PhoneNumber.Trim())).FirstOrDefault();
                if (userByPhoneNumber != null && userByPhoneNumber.PhoneNumber != null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Phone number existed");
                }

                UserEntity user = new()
                {
                    IsEnabled = true,
                    RoleId = model.RoleId,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    CreatedAt = DateTimeOffset.UtcNow,
                    Email = model.Email.Trim(),
                    FullName = model.FullName != null ? model.FullName.Trim() : model.FullName,
                    PhoneNumber = model.PhoneNumber.Trim(),
                    Address = model.Address != null ? model.Address.Trim() : model.Address,
                    Gender = model.Gender != null ? model.Gender.Trim() : model.Gender
                };
                user.PasswordHash = _iEncryptionRepository.EncryptPassword(model.Password, user.SecurityStamp);
                await CreateAsync(user);
                return JsonUtil.Success(user);
            }
            catch (Exception ex) {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }

        public async Task<IActionResult> UpdateUserAsync(int userId, UpdateUserDto model)
        {
            try
            {
                // Validator
                var validator = new UpdateUserValidator();
                var check = await validator.ValidateAsync(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                var currentUser = await GetByIdAsync(userId);
                if (currentUser == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "User does not exist");
                }
                var userByEmail = await GetUserByEmail(model.Email);
                if (userByEmail != null && currentUser.Email != userByEmail.Email)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Email existed");
                }

                var userByPhoneNumber = FindByCondition(x => x.PhoneNumber.Equals(model.PhoneNumber.Trim())).FirstOrDefault();
                if (userByPhoneNumber != null && currentUser.PhoneNumber != userByPhoneNumber.PhoneNumber)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Phone number existed");
                }
                currentUser.ModifiedAt = DateTime.Now;
                currentUser.RoleId = model.RoleId;
                currentUser.Email = model.Email.Trim();
                currentUser.FullName = model.FullName != null ? model.FullName.Trim() : model.FullName;
                currentUser.PhoneNumber = model.PhoneNumber.Trim();
                currentUser.Address = model.Address != null ? model.Address.Trim() : model.Address;
                currentUser.Gender = model.Gender != null ? model.Gender.Trim() : model.Gender;
                await UpdateAsync(currentUser);
                return JsonUtil.Success(currentUser.Id);
            }
            catch(Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }

        public async Task<IActionResult> DeleteUserAsync(long id)
        {
            var user = await GetUserById(id);
            if(user != null)
            {
                user.IsEnabled = false;
                await UpdateAsync(user);
            }
            return JsonUtil.Success(id);
        }
    }
}
