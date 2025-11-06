/*
 *Author：hmf
 *Contact：461857658@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下ORDER_NOTE_FLATService与IORDER_NOTE_FLATService中编写
 */
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.IServices;
using HDPro.CY.Order.Services;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Services
{
    public partial class ORDER_NOTE_FLATService : CYOrderServiceBase<ORDER_NOTE_FLAT, IORDER_NOTE_FLATRepository>
    , IORDER_NOTE_FLATService, IDependency
    {
    public static IORDER_NOTE_FLATService Instance
    {
      get { return AutofacContainerModule.GetService<IORDER_NOTE_FLATService>(); } }
    }
 } 