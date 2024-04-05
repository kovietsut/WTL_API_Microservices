using ElasticSearch.API.Models;
using ElasticSearch.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nest;
using Shared.DTOs;
using Shared.SeedWork;
using ILogger = Serilog.ILogger;

namespace ElasticSearch.API.Repositories
{
    public class UserElasticSearchRepository: IUserElasticSearchRepository
    {
        private readonly IElasticClient _elasticClient;
        private readonly ErrorCode _errorCodes;
        private readonly ILogger _logger;

        public UserElasticSearchRepository(IElasticClient elasticClient, IOptions<ErrorCode> errorCodes, ILogger logger)
        {
            _elasticClient = elasticClient;
            _errorCodes = errorCodes.Value;
            _logger = logger;
        }

        public List<UserSearchResult> GetList(int? pageNumber, int? pageSize, string? searchText, int? roleId)
        {
            try
            {
                pageNumber ??= 1; pageSize ??= 10;
                //var check = _elasticClient.Search<UserSearchResult>(s => s.Index("users-search")
                //    .Query(q => q
                //        .Bool(b => b
                //            .Must(
                //                //mu => mu.Term(t => t.Field(f => f.Email).Value("nguyentienphat9x@gmail.com"))
                //                //mu => mu.Term(t => t.Field(f => f.PhoneNumber).Value("0369427215"))
                //                mu => mu.Term(t => t.Field(f => f.FullName.ToString()).Value("Phat Nguyen"))
                //                //mu => mu.Term(t => t.Field(f => f.Address).Value("Ho Chi Minh"))
                //             )
                //        )
                //    )
                //);
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
                var list = searchResponse.Documents.ToList();
                if (list != null)
                {
                    return list;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error($"Something went wrong: {ex.Message}");
                return null;
            }
        }

        public async Task<IActionResult> CreateUserAsync(UserSearchResult model)
        {
            try
            {
                var userByEmail = _elasticClient.Search<UserSearchResult>(s => s.Index("users-search")
                    .Query(q => q
                        .Term(t => t.Email, model.Email)
                    )
                );
                if (userByEmail != null && userByEmail.Documents.Any())
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Email existed");
                }
                var userByPhoneNumber = _elasticClient.Search<UserSearchResult>(s => s.Index("users-search")
                    .Query(q => q
                        .Term(t => t.PhoneNumber, model.PhoneNumber)
                    )
                );
                if (userByPhoneNumber != null && userByPhoneNumber.Documents.Any())
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Phone number existed");
                }
                // Elastic Here
                var indexedUser = new UserSearchResult()
                {
                    Id = model.Id,
                    IsEnabled = true,
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    AvatarPath = model.AvatarPath,
                    Gender = model.Gender,
                    Address = model.Address,
                    RoleId = model.RoleId,
                    RoleName = Util.GetRoleName(model.RoleId),
                };
                await _elasticClient.IndexDocumentAsync(indexedUser);
                return JsonUtil.Success(indexedUser);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }

        public async Task<IActionResult> UpdateUserAsync(int userId, UserSearchResult model)
        {
            try
            {
                var currentUser = await _elasticClient.GetAsync<UserSearchResult>(userId);
                if (currentUser == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "User does not exist");
                }
                var userByEmail = _elasticClient.Search<UserSearchResult>(s => s.Index("users-search")
                    .Query(q => q
                        .Term(t => t.Email, model.Email)
                    )
                );
                if (userByEmail != null && userByEmail.IsValid && userByEmail.Documents.Any(x => !string.IsNullOrEmpty(x.Email)
                    && x.Email == model.Email && x.Email != currentUser.Source.Email))
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Email existed");
                }
                var userByPhoneNumber = _elasticClient.Search<UserSearchResult>(s => s.Index("users-search")
                    .Query(q => q
                        .Term(t => t.PhoneNumber, model.PhoneNumber)
                    )
                );
                if (userByPhoneNumber != null && !userByPhoneNumber.IsValid && userByPhoneNumber.Documents.Any(x => !string.IsNullOrEmpty(x.PhoneNumber)
                    && x.PhoneNumber == model.PhoneNumber && x.PhoneNumber != currentUser.Source.PhoneNumber))
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Phone number existed");
                }

                if (currentUser.Source.RoleId == 1)
                {
                    currentUser.Source.RoleId = model.RoleId;
                }
                // Elastic Search
                var indexedUser = new UserSearchResult()
                {
                    Id = currentUser.Source.Id,
                    IsEnabled = true,
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    AvatarPath = model.AvatarPath,
                    Gender = model.Gender,
                    Address = model.Address,
                    RoleId = model.RoleId,
                    RoleName = Util.GetRoleName(model.RoleId)
                };
                await _elasticClient.UpdateAsync<UserSearchResult>(currentUser.Id, u => u.Index("users-search").Doc(indexedUser));
                return JsonUtil.Success(currentUser.Id);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }

        public async Task<IActionResult> DeleteUserAsync(long id)
        {
            try
            {
                var user = await _elasticClient.GetAsync<UserSearchResult>(id);
                if (user != null)
                {
                    await _elasticClient.DeleteAsync<UserSearchResult>(user.Id);
                }
                return JsonUtil.Success(id);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }

        public async Task<IActionResult> RemoveSoftListUser(string ids)
        {
            try
            {
                if (ids.IsNullOrEmpty())
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Ids cannot be null");
                }
                var listIds = Util.SplitStringToArray(ids);
                if (listIds.Count != 0)
                {
                    // Delete documents from Elasticsearch
                    var indexedUsers = await _elasticClient.GetManyAsync<UserSearchResult>(listIds);
                    var response = await _elasticClient.DeleteManyAsync(indexedUsers);
                }
                return JsonUtil.Success(listIds);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }
    }
}
