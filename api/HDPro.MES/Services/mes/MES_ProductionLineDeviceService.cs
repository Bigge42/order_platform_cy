/*
 *Author：jxx
 *Contact：461857658@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下MES_ProductionLineDeviceService与IMES_ProductionLineDeviceService中编写
 */
using HDPro.MES.IRepositories;
using HDPro.MES.IServices;
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.MES.Services
{
    public partial class MES_ProductionLineDeviceService : ServiceBase<MES_ProductionLineDevice, IMES_ProductionLineDeviceRepository>
    , IMES_ProductionLineDeviceService, IDependency
    {
    public static IMES_ProductionLineDeviceService Instance
    {
      get { return AutofacContainerModule.GetService<IMES_ProductionLineDeviceService>(); } }
    }
 }
