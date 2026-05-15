/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Sys_RoleAIApp",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Core.Enums;
using HDPro.Core.Filters;
using HDPro.Core.ManageUser;
using HDPro.Core.UserManager;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.Repositories;
using HDPro.Sys.Repositories;

namespace HDPro.CY.Order.Controllers
{
    public partial class Sys_RoleAIAppController
    {
        private readonly ISys_RoleAIAppService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public Sys_RoleAIAppController(
            ISys_RoleAIAppService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet, Route("GetBatchOptions")]
        [ApiActionPermission(ActionPermissionOptions.Search)]
        public async Task<IActionResult> GetBatchOptions()
        {
            var roleIds = GetManageableRoleIds();
            var rolesQuery = Sys_RoleRepository.Instance.FindAsIQueryable(x => true);
            if (!UserContext.Current.IsSuperAdmin)
            {
                rolesQuery = rolesQuery.Where(x => roleIds.Contains(x.Role_Id));
            }

            var roles = await rolesQuery
                .OrderBy(x => x.OrderNo)
                .ThenBy(x => x.Role_Id)
                .Select(x => new
                {
                    key = x.Role_Id,
                    label = x.RoleName
                })
                .ToListAsync();

            var apps = await Sys_AIAppRepository.Instance.FindAsIQueryable(x => x.Status == 1)
                .OrderBy(x => x.SortNo)
                .ThenBy(x => x.Id)
                .Select(x => new
                {
                    key = x.Id,
                    label = x.AppName,
                    appType = x.AppType,
                    description = x.Description
                })
                .ToListAsync();

            return Json(WebResponseContent.Instance.OK(null, new { roles, apps }));
        }

        [HttpGet, Route("GetAIAppsByRole")]
        [ApiActionPermission(ActionPermissionOptions.Search)]
        public async Task<IActionResult> GetAIAppsByRole(int roleId)
        {
            if (!CanManageRole(roleId))
            {
                return Json(WebResponseContent.Instance.Error("无权限操作该角色"));
            }

            var selected = await Sys_RoleAIAppRepository.Instance
                .FindAsIQueryable(x => x.Role_Id == roleId && x.Enable == 1)
                .Select(x => x.AIAppId)
                .ToListAsync();

            return Json(WebResponseContent.Instance.OK(null, selected));
        }

        [HttpPost, Route("SaveAIAppsByRole")]
        [ApiActionPermission(ActionPermissionOptions.Update)]
        public async Task<IActionResult> SaveAIAppsByRole([FromBody] RoleAIAppByRoleRequest request)
        {
            if (request == null || request.RoleId <= 0)
            {
                return Json(WebResponseContent.Instance.Error("请选择角色"));
            }
            if (!CanManageRole(request.RoleId))
            {
                return Json(WebResponseContent.Instance.Error("无权限操作该角色"));
            }

            var aiAppIds = request.AIAppIds?.Where(x => x > 0).Distinct().ToArray() ?? Array.Empty<long>();
            var validAIAppIds = await Sys_AIAppRepository.Instance
                .FindAsIQueryable(x => aiAppIds.Contains(x.Id) && x.Status == 1)
                .Select(x => x.Id)
                .ToListAsync();

            await SaveRoleAIAppRelationsAsync(request.RoleId, validAIAppIds);

            return Json(WebResponseContent.Instance.OK("按角色授权保存成功"));
        }

        [HttpGet, Route("GetRolesByAIApp")]
        [ApiActionPermission(ActionPermissionOptions.Search)]
        public async Task<IActionResult> GetRolesByAIApp(long aiAppId)
        {
            if (aiAppId <= 0)
            {
                return Json(WebResponseContent.Instance.Error("请选择智能体"));
            }

            var manageableRoleIds = GetManageableRoleIds();
            var query = Sys_RoleAIAppRepository.Instance
                .FindAsIQueryable(x => x.AIAppId == aiAppId && x.Enable == 1);
            if (!UserContext.Current.IsSuperAdmin)
            {
                query = query.Where(x => manageableRoleIds.Contains(x.Role_Id));
            }

            var selected = await query.Select(x => x.Role_Id).ToListAsync();
            return Json(WebResponseContent.Instance.OK(null, selected));
        }

        [HttpPost, Route("SaveRolesByAIApp")]
        [ApiActionPermission(ActionPermissionOptions.Update)]
        public async Task<IActionResult> SaveRolesByAIApp([FromBody] RoleAIAppByAIAppRequest request)
        {
            if (request == null || request.AIAppId <= 0)
            {
                return Json(WebResponseContent.Instance.Error("请选择智能体"));
            }

            var appExists = await Sys_AIAppRepository.Instance
                .FindAsIQueryable(x => x.Id == request.AIAppId && x.Status == 1)
                .AnyAsync();
            if (!appExists)
            {
                return Json(WebResponseContent.Instance.Error("智能体不存在或未启用"));
            }

            var manageableRoleIds = GetManageableRoleIds();
            var roleIds = request.RoleIds?.Where(x => x > 0).Distinct().ToArray() ?? Array.Empty<int>();
            if (!UserContext.Current.IsSuperAdmin && roleIds.Any(x => !manageableRoleIds.Contains(x)))
            {
                return Json(WebResponseContent.Instance.Error("包含无权限操作的角色"));
            }

            var validRoleQuery = Sys_RoleRepository.Instance.FindAsIQueryable(x => roleIds.Contains(x.Role_Id));
            if (!UserContext.Current.IsSuperAdmin)
            {
                validRoleQuery = validRoleQuery.Where(x => manageableRoleIds.Contains(x.Role_Id));
            }
            var validRoleIds = await validRoleQuery.Select(x => x.Role_Id).ToListAsync();

            await SaveAIAppRoleRelationsAsync(request.AIAppId, validRoleIds, manageableRoleIds);

            return Json(WebResponseContent.Instance.OK("按智能体授权保存成功"));
        }

        private static bool CanManageRole(int roleId)
        {
            if (UserContext.Current.IsSuperAdmin)
            {
                return true;
            }

            return GetManageableRoleIds().Contains(roleId);
        }

        private static int[] GetManageableRoleIds()
        {
            var currentRoleIds = UserContext.Current.RoleIds ?? Array.Empty<int>();
            return currentRoleIds
                .Concat(RoleContext.GetAllChildrenIds(currentRoleIds))
                .Where(x => x > 0)
                .Distinct()
                .ToArray();
        }

        private static async Task SaveRoleAIAppRelationsAsync(int roleId, List<long> selectedAIAppIds)
        {
            var repository = Sys_RoleAIAppRepository.Instance;
            var exists = await repository.FindAsIQueryable(x => x.Role_Id == roleId)
                .Select(x => new { x.Id, x.AIAppId, x.Enable })
                .ToListAsync();

            var selectedSet = selectedAIAppIds.ToHashSet();
            var user = UserContext.Current.UserInfo;
            var now = DateTime.Now;

            var add = selectedAIAppIds
                .Where(x => !exists.Any(e => e.AIAppId == x))
                .Select(x => new Sys_RoleAIApp
                {
                    Role_Id = roleId,
                    AIAppId = x,
                    Enable = 1,
                    CreateDate = now,
                    Creator = user?.UserTrueName,
                    CreateID = user?.User_Id
                })
                .ToList();

            var update = exists
                .Where(x => (selectedSet.Contains(x.AIAppId) && x.Enable != 1)
                    || (!selectedSet.Contains(x.AIAppId) && x.Enable == 1))
                .Select(x => new Sys_RoleAIApp
                {
                    Id = x.Id,
                    Enable = selectedSet.Contains(x.AIAppId) ? 1 : 0,
                    ModifyDate = now,
                    Modifier = user?.UserTrueName,
                    ModifyID = user?.User_Id
                })
                .ToList();

            repository.AddRange(add);
            repository.UpdateRange(update, x => new { x.Enable, x.ModifyDate, x.Modifier, x.ModifyID });
            repository.SaveChanges();
        }

        private static async Task SaveAIAppRoleRelationsAsync(long aiAppId, List<int> selectedRoleIds, int[] manageableRoleIds)
        {
            var repository = Sys_RoleAIAppRepository.Instance;
            var existsQuery = repository.FindAsIQueryable(x => x.AIAppId == aiAppId);
            if (!UserContext.Current.IsSuperAdmin)
            {
                existsQuery = existsQuery.Where(x => manageableRoleIds.Contains(x.Role_Id));
            }

            var exists = await existsQuery
                .Select(x => new { x.Id, x.Role_Id, x.Enable })
                .ToListAsync();

            var selectedSet = selectedRoleIds.ToHashSet();
            var user = UserContext.Current.UserInfo;
            var now = DateTime.Now;

            var add = selectedRoleIds
                .Where(x => !exists.Any(e => e.Role_Id == x))
                .Select(x => new Sys_RoleAIApp
                {
                    Role_Id = x,
                    AIAppId = aiAppId,
                    Enable = 1,
                    CreateDate = now,
                    Creator = user?.UserTrueName,
                    CreateID = user?.User_Id
                })
                .ToList();

            var update = exists
                .Where(x => (selectedSet.Contains(x.Role_Id) && x.Enable != 1)
                    || (!selectedSet.Contains(x.Role_Id) && x.Enable == 1))
                .Select(x => new Sys_RoleAIApp
                {
                    Id = x.Id,
                    Enable = selectedSet.Contains(x.Role_Id) ? 1 : 0,
                    ModifyDate = now,
                    Modifier = user?.UserTrueName,
                    ModifyID = user?.User_Id
                })
                .ToList();

            repository.AddRange(add);
            repository.UpdateRange(update, x => new { x.Enable, x.ModifyDate, x.Modifier, x.ModifyID });
            repository.SaveChanges();
        }
    }

    public class RoleAIAppByRoleRequest
    {
        public int RoleId { get; set; }
        public long[] AIAppIds { get; set; }
    }

    public class RoleAIAppByAIAppRequest
    {
        public long AIAppId { get; set; }
        public int[] RoleIds { get; set; }
    }
}
