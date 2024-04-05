using Contracts.Domains.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.User;
using UserEntity = User.API.Entities.User;

namespace User.API.Repositories.Interfaces
{
    public interface IUserRepository : IRepositoryBase<UserEntity, long>
    {
        List<string> GetListEmail(List<long?> listIds);
        Task<IActionResult> GetList(int? pageNumber, int? pageSize, string? searchText, int? roleId);
        Task<UserEntity> GetUserByEmail(string email);
        Task<UserEntity> GetUserById(long id);
        Task<IActionResult> GetUser(long userId);
        Task<IActionResult> CreateUserAsync(CreateUserDto model);
        Task<IActionResult> UpdateUserAsync(int userId, UpdateUserDto model);
        Task<IActionResult> DeleteUserAsync(long id);
        Task<IActionResult> RemoveSoftListUser(string ids);
    }
}
