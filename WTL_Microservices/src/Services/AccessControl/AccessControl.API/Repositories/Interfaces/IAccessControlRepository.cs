using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Permission;

namespace AccessControl.API.Repositories.Interfaces
{
    public interface IAccessControlRepository
    {
        Task<IActionResult> GrantPermission(GrantPermissionDto model);
        Task<IActionResult> UpdatePermission(long permissionId, UpdatePermissionDto model);
    }
}
