using AccessControl.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Permission;

namespace AccessControl.API.Repositories.Interfaces
{
    public interface IAccessControlRepository
    {
        Task<IActionResult> GetListPermission(long albumId);
        Task<IActionResult> GrantPermission(GrantPermissionDto model);
        Task<IActionResult> UpdatePermission(long permissionId, UpdatePermissionDto model);
        Task<IActionResult> DeletePermission(long permissionId);
        Task<IActionResult> DeleteListPermission(string permissionIds);
    }
}
