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
    }
}
