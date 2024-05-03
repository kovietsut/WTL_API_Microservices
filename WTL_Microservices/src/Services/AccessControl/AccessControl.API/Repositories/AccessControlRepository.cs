using AccessControl.API.Persistence;
using AccessControl.API.Repositories.Interfaces;
using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared.DTOs;
using Shared.DTOs.Permission;
using Shared.SeedWork;
using PermissionEntity = AccessControl.API.Entities.Permission;

namespace AccessControl.API.Repositories
{
    public class AccessControlRepository : RepositoryBase<PermissionEntity, long, AccessControlContext>, IAccessControlRepository
    {
        private readonly ErrorCode _errorCodes;

        public AccessControlRepository(AccessControlContext dbContext, IUnitOfWork<AccessControlContext> unitOfWork, 
            IOptions<ErrorCode> errorCode) : base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
        }

        public async Task<IActionResult> GrantPermission(GrantPermissionDto model)
        {
            try
            {
                PermissionEntity permission = new()
                {
                    IsEnabled = true,
                    ActionId = model.ActionId,
                    MangaId = model.MangaId,
                    UserId = model.UserId,
                    AlbumId = model.AlbumId,
                    PermissionType = model.Type != null ? model.Type.Trim() : model.Type,
                };
                await CreateAsync(permission);
                return JsonUtil.Success(permission);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }
    }
}
