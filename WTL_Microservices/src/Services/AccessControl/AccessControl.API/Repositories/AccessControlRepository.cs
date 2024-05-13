using AccessControl.API.Entities;
using AccessControl.API.Persistence;
using AccessControl.API.Repositories.Interfaces;
using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Common;
using Shared.Common.Interfaces;
using Shared.DTOs;
using Shared.DTOs.Permission;
using Shared.SeedWork;
using System.Security;
using PermissionEntity = AccessControl.API.Entities.Permission;

namespace AccessControl.API.Repositories
{
    public class AccessControlRepository : RepositoryBase<PermissionEntity, long, AccessControlContext>, IAccessControlRepository
    {
        private readonly ErrorCode _errorCodes;
        private readonly IBaseAuthService _baseAuthService;

        public AccessControlRepository(AccessControlContext dbContext, IUnitOfWork<AccessControlContext> unitOfWork, 
            IOptions<ErrorCode> errorCode, IBaseAuthService baseAuthService) : base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
            _baseAuthService = baseAuthService;
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
                var currentPermission = FindByCondition(x => x.UserId == model.UserId && x.AlbumId == model.AlbumId);
                if(currentPermission != null)
                {
                    return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.BadRequest, "This user already has permission");
                }
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
                var currentUser = _baseAuthService.GetCurrentUserId();
                var currentPermission = await GetByIdAsync(permissionId);
                if(currentPermission == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Permission not found!");
                } 
                else if ((int)currentPermission.UserId == currentUser)
                {
                    return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.BadRequest, "You cannot kick yourself");
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

        public async Task<IActionResult> DeleteListPermission(string permissionIds)
        {
            try
            {
                var list = new List<Permission>();
                if (permissionIds.IsNullOrEmpty())
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "permissionIds cannot be null");
                }
                var listIds = Util.SplitStringToArray(permissionIds);
                var permissions = FindAll().Where(x => listIds.Contains(x.Id));
                if (permissions == null || permissions.Count() == 0)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Cannot get list permission");
                }
                foreach (var permission in permissions)
                {
                    permission.IsEnabled = false;
                    list.Add(permission);
                }
                var listRemoved = permissions.Select(x => x.Id).ToList();
                if (list.Count != 0)
                {
                    await UpdateListAsync(list);
                }
                //await EndTransactionAsync();
                return JsonUtil.Success(listRemoved);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.SystemError, ex.Message);
            }
        }
    }
}
