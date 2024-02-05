using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using UserEntity = User.API.Entities.User;
using User.API.Persistence;
using User.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs.User;
using Microsoft.AspNetCore.Mvc;
using Shared.SeedWork;
using Shared.DTOs;
using Microsoft.Extensions.Options;
using FluentValidation;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;

namespace User.API.Repositories
{
    public class UserRepository: RepositoryBase<UserEntity, long, IdentityContext>, IUserRepository
    {
        private readonly IEncryptionRepository _iEncryptionRepository;
        private readonly IRedisCacheRepository _iRedisCacheRepository;
        private readonly ErrorCode _errorCodes;

        public UserRepository(IdentityContext dbContext, IUnitOfWork<IdentityContext> unitOfWork, IEncryptionRepository iEncryptionRepository,
            IRedisCacheRepository iRedisCacheRepository, IOptions<ErrorCode> errorCodes) 
            : base(dbContext, unitOfWork)
        {
            _iEncryptionRepository = iEncryptionRepository;
            _iRedisCacheRepository = iRedisCacheRepository;
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
                        UserId = x.Id, x.IsEnabled, x.FullName, x.Email, x.PhoneNumber, x.AvatarPath, x.Gender, x.Address, 
                        x.RoleId, RoleName = x.Role.Name
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

        public async Task<IActionResult> GetUser(long userId)
        {
            var key = $"/api/user/{userId}";
            var cacheUser = await _iRedisCacheRepository.GetCachedResponseAsync(key);
            if (!string.IsNullOrEmpty(cacheUser))
            {
                var response = JsonConvert.DeserializeObject<UserEntity>(cacheUser)!;
                var resultCache = new
                {
                    response.Id,
                    response.IsEnabled,
                    response.RoleId,
                    response.Email,
                    response.GoogleUserId,
                    response.FullName,
                    response.PhoneNumber,
                    response.Address,
                    response.Gender,
                    response.AvatarPath,
                    response.CreatedAt,
                    response.ModifiedAt
                };
                return JsonUtil.Success(resultCache);
            }
            var user = await GetUserById(userId);
            if (user == null)
            {
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "User does not exist");
            }
            var result = new
            {
                user.Id, user.IsEnabled, user.RoleId, user.Email, user.GoogleUserId, user.FullName, user.PhoneNumber, user.Address, 
                user.Gender, user.AvatarPath, user.CreatedAt, user.ModifiedAt
            };
            // Store user to cache
            await _iRedisCacheRepository.SetCachedResponseAsync(key, result);
            return JsonUtil.Success(result);
        }

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
                var key = $"/api/user/{userId}";
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
                if (currentUser.RoleId == 1)
                {
                    currentUser.RoleId = model.RoleId;
                }
                currentUser.ModifiedAt = DateTime.Now;
                currentUser.Email = model.Email.Trim();
                currentUser.FullName = model.FullName != null ? model.FullName.Trim() : model.FullName;
                currentUser.PhoneNumber = model.PhoneNumber.Trim();
                currentUser.Address = model.Address != null ? model.Address.Trim() : model.Address;
                currentUser.Gender = model.Gender != null ? model.Gender.Trim() : model.Gender;
                await UpdateAsync(currentUser);
                // Update cached
                await _iRedisCacheRepository.RemoveCached(key);
                await _iRedisCacheRepository.SetCachedResponseAsync(key, currentUser);
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

        public async Task<IActionResult> RemoveSoftListUser(string ids)
        {
            try
            {
                await BeginTransactionAsync();
                var list = new List<UserEntity>();
                if (ids.IsNullOrEmpty())
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Ids cannot be null");
                }
                var listIds = Util.SplitStringToArray(ids);
                // List User
                var users = FindAll().Where(x => listIds.Contains(x.Id));
                if (users == null || users.Count() == 0)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Cannot get list user");
                }
                foreach (var genre in users)
                {
                    genre.IsEnabled = false;
                    list.Add(genre);
                }
                var listRemoved = users.Select(x => x.Id).ToList();
                if (list.Count != 0)
                {
                    await UpdateListAsync(list);
                }
                await EndTransactionAsync();
                return JsonUtil.Success(listRemoved);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }
    }
}
