/*
 *Author：jxx
 *Contact：283591387@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Sys_NotificationTemplateService与ISys_NotificationTemplateService中编写
 */
using HDPro.Sys.IRepositories;
using HDPro.Sys.IServices;
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.Sys.Services
{
    public partial class Sys_NotificationTemplateService : ServiceBase<Sys_NotificationTemplate, ISys_NotificationTemplateRepository>
    , ISys_NotificationTemplateService, IDependency
    {
    public static ISys_NotificationTemplateService Instance
    {
      get { return AutofacContainerModule.GetService<ISys_NotificationTemplateService>(); } }
    }
 }
