using AccessControl.API.Entities;
using AccessControl.API.Repositories;
using AccessControl.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Permission;
using Shared.DTOs.User;

namespace AccessControl.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccessControlController : ControllerBase
    {
        private readonly IAccessControlRepository _accessControlRepository;

        public AccessControlController(IAccessControlRepository accessControlRepository)
        {
            _accessControlRepository = accessControlRepository;
        }

        [HttpGet("permissions/{albumId}")]
        public async Task<IActionResult> GetListPermission(long albumId)
        {
            return await _accessControlRepository.GetListPermission(albumId);
        }

        [HttpPost]
        public async Task<IActionResult> GrantPermission([FromBody] GrantPermissionDto model)
        {
            return await _accessControlRepository.GrantPermission(model);
        }

        [HttpPut("{permissionId}")]
        public async Task<IActionResult> UpdatePermission(long permissionId, [FromBody] UpdatePermissionDto model)
        {
            return await _accessControlRepository.UpdatePermission(permissionId, model);
        }

        [HttpDelete("{perrmissionId}")]
        public async Task<IActionResult> DeletePermission(long perrmissionId)
        {
            return await _accessControlRepository.DeletePermission(perrmissionId);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteListPermission(string permissionIds)
        {
            return await _accessControlRepository.DeleteListPermission(permissionIds);
        }
    }
}
