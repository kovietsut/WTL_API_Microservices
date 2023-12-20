using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shared.DTOs.RedisCache;
using Shared.DTOs.User;
using Shared.SeedWork;
using StackExchange.Redis;
using User.API.Repositories.Interfaces;
using UserEntity = User.API.Entities.User;

namespace User.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _iUserRepository;

        public UserController(IUserRepository iUserRepository)
        {
            _iUserRepository = iUserRepository;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(int userId)
        {
            return await _iUserRepository.GetUser(userId);
        }

        [HttpGet("get-list")]
        public IActionResult GetList(int? pageNumber, int? pageSize, string? searchText, int? roleId)
        {
            return _iUserRepository.GetList(pageNumber, pageSize, searchText, roleId);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserDto model)
        {
            return await _iUserRepository.CreateUserAsync(model);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUserAsync(int userId, [FromBody] UpdateUserDto model)
        {
            return await _iUserRepository.UpdateUserAsync(userId, model);
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUserAsync(int userId)
        {
            return await _iUserRepository.DeleteUserAsync(userId);
        }
    }
}
