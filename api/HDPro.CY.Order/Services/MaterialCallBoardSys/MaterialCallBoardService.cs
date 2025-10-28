/*
 *Author：hmf
 *Contact：461857658@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下MaterialCallBoardService与IMaterialCallBoardService中编写
 */
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.IServices;
// using HDPro.CY.Order.Services; // 当前文件已经在这个命名空间里, 可删以降低歧义
using HDPro.Core.Extensions.AutofacManager;
// using HDPro.Entity.DomainModels; // 避免歧义，改用别名
using MCBEntity = HDPro.Entity.DomainModels.MaterialCallBoard;

namespace HDPro.CY.Order.Services
{
    public partial class MaterialCallBoardService
        : CYOrderServiceBase<MCBEntity, IMaterialCallBoardRepository>, // ★ 关键：T = 同一个实体别名
          IMaterialCallBoardService, IDependency
    {
        public static IMaterialCallBoardService Instance
            => AutofacContainerModule.GetService<IMaterialCallBoardService>();
    }
}
