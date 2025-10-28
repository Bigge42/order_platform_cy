/*
 *所有关于MaterialCallBoard类的业务代码应在此处编写
 *可使用repository.调用常用方法，获取EF/Dapper等信息
 *如果需要事务请使用repository.DbContextBeginTransaction
 *也可使用DBServerProvider.手动获取数据库相关信息
 *用户信息、权限、角色等使用UserContext.Current操作
 *MaterialCallBoardService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
 */

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using HDPro.Core.Utilities;
using HDPro.CY.Order.IRepositories;
// 注意：不要在这里 using HDPro.CY.Order.Services（当前文件就处于这个命名空间，会增加歧义）
// 用别名明确实体类型，避免与“MaterialCallBoard”同名命名空间冲突
using MCBEntity = HDPro.Entity.DomainModels.MaterialCallBoard;

namespace HDPro.CY.Order.Services
{
    public partial class MaterialCallBoardService
    {
        private readonly IMaterialCallBoardRepository _repository; // 访问数据库

        [ActivatorUtilitiesConstructor]
        public MaterialCallBoardService(
            IMaterialCallBoardRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
        )
        // ⚠️ 这里要与“主 Service 文件”里基类的构造签名一致
        // 主文件应是：class MaterialCallBoardService 
        //              : CYOrderServiceBase<MCBEntity, IMaterialCallBoardRepository>, ...
        // 所以这里调用 base(repo, httpContextAccessor) 没问题
        : base(dbRepository, httpContextAccessor)
        {
            _repository = dbRepository;
        }

        // 提示：只有当基类真的声明了这个虚方法时，才能 override。
        // 若编译报“没有可覆盖的适用方法”，请删掉本方法或改成你基类实际提供的扩展点（见下方备注）。
        protected override WebResponseContent ValidateCYOrderEntity(MCBEntity entity)
        {
            var response = base.ValidateCYOrderEntity(entity);
            if (!response.Status) return response;

            // 在此处添加 MaterialCallBoard 特有的验证逻辑，例如：
            // if (string.IsNullOrWhiteSpace(entity.WorkOrderNo))
            //     return response.Error("WorkOrderNo 不能为空");

            return response;
        }

        // 如果确实需要一个初始化钩子，而基类没有 InitCYOrderSpecific 这个方法，
        // 请把下面这个方法删除（避免 override 报错），
        // 或者改为你基类真实存在的扩展点（例如 OnModelCreating/Init/ServiceFunFilter 钩子等）。
        // protected override void InitCYOrderSpecific()
        // {
        //     base.InitCYOrderSpecific();
        //     // MaterialCallBoard 特有初始化逻辑
        // }
    }
}
