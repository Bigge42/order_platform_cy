/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Sys_Department",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.Sys.IServices;
using HDPro.Core.Filters;
using HDPro.Core.Enums;
using HDPro.Core.Extensions;
using HDPro.Sys.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using HDPro.Core.ManageUser;
using HDPro.Core.UserManager;

namespace HDPro.Sys.Controllers
{
    public partial class Sys_DepartmentController
    {
        private readonly ISys_DepartmentService _service;//访问业务代码
        private readonly ISys_DepartmentRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public Sys_DepartmentController(
            ISys_DepartmentService service,
             ISys_DepartmentRepository repository,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }


        /// <summary>
        /// treetable 获取子节点数据(2021.05.02)
        /// </summary>
        /// <param name="loadData"></param>
        /// <returns></returns>
        [ApiActionPermission(ActionPermissionOptions.Search)]
        [HttpPost, Route("GetPageData")]
        public override ActionResult GetPageData([FromBody] PageDataOptions loadData)
        {
            //if (loadData.Value.GetInt() == 1)
            //{
            //    return GetTreeTableRootData(loadData).Result;
            //}
            return base.GetPageData(loadData);
        }

        /// <summary>
        /// treetable 获取子节点数据
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("getTreeTableRootData")]
        [ApiActionPermission(ActionPermissionOptions.Search)]
        public async Task<ActionResult> GetTreeTableRootData([FromBody] PageDataOptions options)
        {
            //页面加载根节点数据条件x => x.ParentId == 0,自己根据需要设置
            var query = _repository.FindAsIQueryable(x => true);
            if (UserContext.Current.IsSuperAdmin)
            {
                query = query.Where(x => x.ParentId == null);
            }
            else
            {
                var deptIds = UserContext.Current.DeptIds;
                var list = DepartmentContext.GetAllDept().Where(c => deptIds.Contains(c.id)).ToList();
                deptIds = list.Where(c=>!list.Any(x=>x.id==c.parentId)).Select(x => x.id).ToList();
                query = query.Where(x => deptIds.Contains(x.DepartmentId));
            }
            var queryChild = _repository.FindAsIQueryable(x => true);
            var rows = await query.TakeOrderByPage(options.Page, options.Rows)
                .OrderBy(x => x.DepartmentName).Select(s => new
                {
                    s.DepartmentId,
                    s.ParentId,
                    s.DepartmentName,
                    s.DepartmentCode,
                    s.Enable,
                    s.DepartmentType,
                    s.Remark,
                    s.CreateDate,
                    s.Creator,
                    s.Modifier,
                    s.ModifyDate,
                    hasChildren = queryChild.Any(x => x.ParentId == s.DepartmentId)
                }).ToListAsync();
            return JsonNormal(new { total = await query.CountAsync(), rows });
        }

        /// <summary>
        ///treetable 获取子节点数据
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("getTreeTableChildrenData")]
        [ApiActionPermission(ActionPermissionOptions.Search)]
        public async Task<ActionResult> GetTreeTableChildrenData(Guid departmentId)
        {
            //点击节点时，加载子节点数据
            var query = _repository.FindAsIQueryable(x => true);
            var rows = await query.Where(x => x.ParentId == departmentId)
                .Select(s => new
                {
                    s.DepartmentId,
                    s.ParentId,
                    s.DepartmentName,
                    s.DepartmentCode,
                    s.Enable,
                    s.DepartmentType,
                    s.Remark,
                    s.CreateDate,
                    s.Creator,
                    s.Modifier,
                    s.ModifyDate,
                    hasChildren = query.Any(x => x.ParentId == s.DepartmentId)
                }).ToListAsync();
            return JsonNormal(new { rows });
        }

        /// <summary>
        /// 获取完整的组织架构树
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("getDepartmentTree")]
        public async Task<IActionResult> GetDepartmentTree()
        {
            try
            {
                // 获取用户有权限的部门ID列表
                var authorizedDeptIds = UserContext.Current.GetAllChildrenDeptIds();
                
                // 获取所有部门数据
                var query = _repository.FindAsIQueryable(x => x.Enable == 1);
                
                // 根据用户权限筛选部门
                if (!UserContext.Current.IsSuperAdmin)
                {
                    query = query.Where(x => authorizedDeptIds.Contains(x.DepartmentId));
                }
                
                var allDepartments = await query.Select(s => new DepartmentTreeDto
                {
                    DepartmentId = s.DepartmentId,
                    ParentId = s.ParentId,
                    DepartmentName = s.DepartmentName,
                    DepartmentType = s.DepartmentType,
                    DepartmentCode = s.DepartmentCode
                }).ToListAsync();

                // 构建树形结构
                var tree = BuildDepartmentTree(allDepartments);
                
                return JsonNormal(new { data = tree });
            }
            catch (Exception ex)
            {
                return Error("获取组织架构树失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 构建部门树形结构
        /// </summary>
        /// <param name="departments">部门列表</param>
        /// <returns></returns>
        private List<object> BuildDepartmentTree(List<DepartmentTreeDto> departments)
        {
            var tree = new List<object>();
            
            // 找到根节点（ParentId为null的节点）
            var rootDepartments = departments.Where(d => d.ParentId == null).ToList();
            
            foreach (var rootDept in rootDepartments)
            {
                var treeNode = new
                {
                    id = rootDept.DepartmentId.ToString(),
                    name = rootDept.DepartmentName,
                    code = rootDept.DepartmentCode,
                    type = string.IsNullOrEmpty(rootDept.DepartmentType) ? "department" : rootDept.DepartmentType.ToLower(),
                    children = BuildChildrenNodes(rootDept.DepartmentId, departments)
                };
                
                tree.Add(treeNode);
            }
            
            return tree;
        }

        /// <summary>
        /// 递归构建子节点
        /// </summary>
        /// <param name="parentId">父节点ID</param>
        /// <param name="allDepartments">所有部门列表</param>
        /// <returns></returns>
        private List<object> BuildChildrenNodes(Guid parentId, List<DepartmentTreeDto> allDepartments)
        {
            var children = new List<object>();
            var childDepartments = allDepartments.Where(d => d.ParentId == parentId).ToList();
            
            foreach (var childDept in childDepartments)
            {
                var childNode = new
                {
                    id = childDept.DepartmentId.ToString(),
                    name = childDept.DepartmentName,
                    code = childDept.DepartmentCode,
                    type = string.IsNullOrEmpty(childDept.DepartmentType) ? "department" : childDept.DepartmentType.ToLower(),
                    children = BuildChildrenNodes(childDept.DepartmentId, allDepartments)
                };
                
                children.Add(childNode);
            }
            
            return children;
        }
    }

    /// <summary>
    /// 部门树形结构DTO
    /// </summary>
    public class DepartmentTreeDto
    {
        public Guid DepartmentId { get; set; }
        public Guid? ParentId { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentType { get; set; }
        public string DepartmentCode { get; set; }
    }
}

