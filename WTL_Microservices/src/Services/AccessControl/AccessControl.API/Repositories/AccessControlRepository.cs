using AccessControl.API.Persistence;
using AccessControl.API.Repositories.Interfaces;
using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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

        public async Task<IActionResult> GetListPermission(long albumId)
        {
            try
            {
                var permissions = FindAll().Where(x => x.AlbumId == albumId && x.IsEnabled == true)
                    .Select(x => new
                    {
                        Id = x.Id,
                        x.UserId,
                        x.ActionId
                        
                    }).ToList();
                if (permissions.IsNullOrEmpty())
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Empty list data");
                }
                return JsonUtil.Success(permissions);

            }
            catch(Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.SystemError, ex.Message);
            }
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
                return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.SystemError, ex.Message);
            }
        }

        public async Task<IActionResult> UpdatePermission(long permissionId, UpdatePermissionDto model)
        {
            try
            {
                var validator = new UpdatePermissionValidator();
                var check = await validator.ValidateAsync(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                var permission = await GetByIdAsync(permissionId);
                if (permission == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Permission not found!");
                }
                if(permission.ActionId == model.ActionId)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "ActionId is number existed");
                }
                permission.ActionId = model.ActionId;
                await UpdateAsync(permission);
                return JsonUtil.Success(permission);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.SystemError, ex.Message);
            }
        }
        public async Task<IActionResult> DeletePermission(long permissionId)
        {
            try
            {
                var currentPermission = await GetByIdAsync(permissionId);
                if(currentPermission == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Permission not found!");
                }
                currentPermission.IsEnabled = false;
                await UpdateAsync(currentPermission);
                return JsonUtil.Success(currentPermission.Id);
            }
            catch(Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.SystemError, ex.Message);
            }
        }
    }
}
