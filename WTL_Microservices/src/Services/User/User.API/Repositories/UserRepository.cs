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
using ILogger = Serilog.ILogger;
using Nest;
using Shared.ElasticSearchModel;

namespace User.API.Repositories
{
    public class UserRepository: RepositoryBase<UserEntity, long, IdentityContext>, IUserRepository
    {
        private readonly IEncryptionRepository _iEncryptionRepository;
        private readonly IRedisCacheRepository _iRedisCacheRepository;
        private readonly ErrorCode _errorCodes;
        private readonly ILogger _logger;
        private readonly IElasticClient _elasticClient;

        public UserRepository(IdentityContext dbContext, IUnitOfWork<IdentityContext> unitOfWork, IEncryptionRepository iEncryptionRepository,
            IRedisCacheRepository iRedisCacheRepository, IOptions<ErrorCode> errorCodes, ILogger logger, IElasticClient elasticClient) 
            : base(dbContext, unitOfWork)
        {
            _iEncryptionRepository = iEncryptionRepository;
            _iRedisCacheRepository = iRedisCacheRepository;
            _errorCodes = errorCodes.Value;
            _logger = logger;
            _elasticClient = elasticClient;
        }

        public List<string> GetListEmail(List<long?> listIds)
        {
            var emails = FindAll().Where(x => listIds.Contains(x.Id)).Select(x => x.Email).ToList();
            return emails;
        }

        public IActionResult GetList(int? pageNumber, int? pageSize, string? searchText, int? roleId)
        {
            try
            {
                pageNumber ??= 1; pageSize ??= 10;
                var searchResponse = _elasticClient.Search<UserSearchResult>(s => s
                    .Index("users-search")
                    .Query(q => q
                        .Bool(b => b
                            .Must(
                                mu => mu.Term(t => t.Field(f => f.IsEnabled).Value(true)),
                                mu => roleId == null ? null : mu.Term(t => t.Field(f => f.RoleId).Value(roleId)),
                                mu => searchText == null ? null : mu.MultiMatch(m => m
                                    .Fields(f => f
                                        .Field(ff => ff.FullName)
                                        .Field(ff => ff.PhoneNumber)
                                        .Field(ff => ff.Address)
                                        .Field(ff => ff.Gender)
                                        .Field(ff => ff.Email)
                                    )
                                    .Query(searchText)
                                )
                            )
                        )
                    )
                    .From(((pageNumber ?? 1) - 1) * (pageSize ?? 10))
                    .Size(pageSize)
                    .Sort(srt => srt.Descending(x => x.Id))
                );
                var list = searchResponse.Documents;
                if (list != null)
                {
                    var totalRecords = list.Count();
                    return JsonUtil.Success(list, dataCount: totalRecords);
                }
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Empty List Data");

                //var list = FindAll().Include(x => x.Role).Where(x => x.IsEnabled == true && (x.RoleId == roleId || roleId == null) &&
                //(searchText == null || x.FullName.Contains(searchText.Trim()) || x.PhoneNumber.Contains(searchText.Trim())
                //    || x.Address.Contains(searchText.Trim()) || x.Gender.Contains(searchText.Trim()) || x.Email.Contains(searchText.Trim())))
                //    .Select(x => new
                //    {
                //        UserId = x.Id,
                //        x.IsEnabled,
                //        x.FullName,
                //        x.Email,
                //        x.PhoneNumber,
                //        x.AvatarPath,
                //        x.Gender,
                //        x.Address,
                //        x.RoleId,
                //        RoleName = x.Role.Name
                //    });
                //var listData = list.Skip(((int)pageNumber - 1) * (int)pageSize)
                //    .Take((int)pageSize).OrderByDescending(x => x.UserId).ToList();
                //if (list != null)
                //{
                //    var totalRecords = list.Count();
                //    return JsonUtil.Success(listData, dataCount: totalRecords);
                //}
                //return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Empty List Data");
            }
            catch (Exception ex)
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
            _logger.Information($"START: GetUserId {userId}");
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
                _logger.Information($"END: GetUser {resultCache.FullName}");
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
            _logger.Information($"END: GetUser {result.FullName}");
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

                var user = new UserEntity()
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
                // Elastic Here
                var indexedUser = new UserSearchResult()
                {
                    Id = user.Id,
                    IsEnabled = user.IsEnabled,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    AvatarPath = user.AvatarPath,
                    Gender = user.Gender,
                    Address = user.Address,
                    RoleId = user.RoleId,
                    RoleName = Util.GetRoleName(user.RoleId),
                };
                await _elasticClient.IndexDocumentAsync(indexedUser);
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
                // Elastic Search
                var indexedUser = new UserSearchResult()
                {
                    Id = currentUser.Id,
                    IsEnabled = currentUser.IsEnabled,
                    FullName = currentUser.FullName,
                    Email = currentUser.Email,
                    PhoneNumber = currentUser.PhoneNumber,
                    AvatarPath = currentUser.AvatarPath,
                    Gender = currentUser.Gender,
                    Address = currentUser.Address,
                    RoleId = currentUser.RoleId,
                    RoleName = Util.GetRoleName(currentUser.RoleId)
                };
                await _elasticClient.UpdateAsync<UserSearchResult>(currentUser.Id, u => u.Index("users-search").Doc(indexedUser));
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
                // Elastic search
                await _elasticClient.DeleteAsync<UserEntity>(user);
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
                foreach (var user in users)
                {
                    user.IsEnabled = false;
                    list.Add(user);
                }
                var listRemoved = users.Select(x => x.Id).ToList();
                if (list.Count != 0)
                {
                    await UpdateListAsync(list);
                    // Delete documents from Elasticsearch
                    var response = await _elasticClient.DeleteManyAsync(users);
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
