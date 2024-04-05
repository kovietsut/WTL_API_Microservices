using ElasticSearch.API.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Shared.SeedWork;

namespace ElasticSearch.API.Repositories.Interfaces
{
    public interface IUserElasticSearchRepository
    {
        List<UserSearchResult> GetList(int? pageNumber, int? pageSize, string? searchText, int? roleId);
        Task<IActionResult> CreateUserAsync(UserSearchResult model);
        Task<IActionResult> UpdateUserAsync(int userId, UserSearchResult model);
        Task<IActionResult> DeleteUserAsync(long id);
        Task<IActionResult> RemoveSoftListUser(string ids);
    }
}
