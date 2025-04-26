/*
 *所有关于Sys_Department类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Sys_DepartmentService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using System.Linq;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using HDPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Sys.IRepositories;
using System.Collections.Generic;
using HDPro.Core.ManageUser;
using HDPro.Core.UserManager;
using System;
using HDPro.Sys.Repositories;

namespace HDPro.Sys.Services
{
    public partial class Sys_DepartmentService
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISys_DepartmentRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public Sys_DepartmentService(
            ISys_DepartmentRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
        }

        public override PageGridData<Sys_Department> GetPageData(PageDataOptions options)
        {
            FilterData();
            return base.GetPageData(options);
        }

        private void FilterData()
        {
            //限制 只能看自己部门及下级组织的数据
            QueryRelativeExpression = (IQueryable<Sys_Department> queryable) =>
            {
                if (UserContext.Current.IsSuperAdmin)
                {
                    return queryable;
                }
                var deptIds = UserContext.Current.GetAllChildrenDeptIds();
                return queryable.Where(x => deptIds.Contains(x.DepartmentId)); 
            };
        }
        public override WebResponseContent Export(PageDataOptions pageData)
        {
            FilterData();
            return base.Export(pageData);
        }

        WebResponseContent webResponse = new WebResponseContent();
        public override WebResponseContent Add(SaveModel saveDataModel)
        {
            AddOnExecuting = (Sys_Department dept, object list) =>
            {
                dept.DbServiceId = UserContext.CurrentServiceId;
                return webResponse.OK();
            };

            AddOnExecuted = (Sys_Department dept, object list) =>
            {
                //2023.12.01增加非理管员创建组织部门后台自动有此部门数据
                if (!UserContext.Current.IsSuperAdmin && dept.ParentId == null)
                {
                    Sys_UserDepartment userDepartment = new Sys_UserDepartment()
                    {
                        Id = Guid.NewGuid(),
                        DepartmentId = dept.DepartmentId,
                        CreateDate = DateTime.Now,
                        UserId = UserContext.Current.UserId,
                        Enable = 1,
                        Creator = UserContext.Current.UserTrueName
                    };
                    repository.DbContext.Add(userDepartment);
                    //2023.12.27修复非管理员添加一级节点时界面不显示的问题

                    var userRepsitory = Sys_UserRepository.Instance;
                    var user = userRepsitory.FindAsIQueryable(x => x.User_Id == UserContext.Current.UserId)
                      .AsNoTracking().FirstOrDefault();
                    List<Guid> guids = new List<Guid>() { dept.DepartmentId };
                    if (user != null && !string.IsNullOrEmpty(user.DeptIds))
                    {
                        guids.AddRange(user.DeptIds.Split(",").Select(c => (Guid)c.GetGuid()));
                    }
                    user.DeptIds = string.Join(",", guids.Distinct());

                    userRepsitory.Update(user, x => new { x.DeptIds });
                    userRepsitory.SaveChanges();
                    userRepsitory.Detached(user);
                  
                    UserContext.Current.LogOut(UserContext.Current.UserId);
                }
                return webResponse.OK();
            };
            return base.Add(saveDataModel).Reload();
        }
        public override WebResponseContent Update(SaveModel saveModel)
        {
            if (!saveModel.MainData.ContainsKey("DbServiceId"))
            {
                saveModel.MainData["DbServiceId"] = UserContext.CurrentServiceId;
            }
            UpdateOnExecuting = (Sys_Department dept, object addList, object updateList, List<object> delKeys) =>
            {
                if (dept.ParentId == dept.DepartmentId)
                {
                    return webResponse.Error("上级组织不能选择自己");
                }
                if (_repository.Exists(x => x.DepartmentId == dept.ParentId && x.ParentId == dept.DepartmentId))
                {
                    return webResponse.Error("不能选择此上级组织");
                }
                return webResponse.OK();
            };
            return base.Update(saveModel).Reload();
        }

        public override WebResponseContent Del(object[] keys, bool delList = true)
        {
            return base.Del(keys, delList).Reload();
        }
    }

}
