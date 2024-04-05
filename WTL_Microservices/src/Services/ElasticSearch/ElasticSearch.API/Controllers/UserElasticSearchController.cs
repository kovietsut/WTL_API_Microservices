using ElasticSearch.API.Models;
using ElasticSearch.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElasticSearch.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserElasticSearchController : ControllerBase
    {
        private readonly IUserElasticSearchRepository _iUserElasticSearchRepository;

        public UserElasticSearchController(IUserElasticSearchRepository iUserRepository)
        {
            _iUserElasticSearchRepository = iUserRepository;
        }

        [HttpGet("get-list")]
        public IActionResult GetList(int? pageNumber, int? pageSize, string? searchText, int? roleId)
        {
            return _iUserElasticSearchRepository.GetList(pageNumber, pageSize, searchText, roleId);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserAsync([FromBody] UserSearchResult model)
        {
            return await _iUserElasticSearchRepository.CreateUserAsync(model);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUserAsync(int userId, [FromBody] UserSearchResult model)
        {
            return await _iUserElasticSearchRepository.UpdateUserAsync(userId, model);
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUserAsync(int userId)
        {
            return await _iUserElasticSearchRepository.DeleteUserAsync(userId);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteList(string ids)
        {
            return await _iUserElasticSearchRepository.RemoveSoftListUser(ids);
        }
    }
}
