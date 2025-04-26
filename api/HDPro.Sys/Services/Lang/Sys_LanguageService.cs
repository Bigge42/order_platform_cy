/*
 *Author：jxx
 *Contact：461857658@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Sys_LanguageService与ISys_LanguageService中编写
 */
using HDPro.Sys.IRepositories;
using HDPro.Sys.IServices;
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.Sys.Services
{
    public partial class Sys_LanguageService : ServiceBase<Sys_Language, ISys_LanguageRepository>
    , ISys_LanguageService, IDependency
    {
    public Sys_LanguageService(ISys_LanguageRepository repository)
    : base(repository)
    {
    Init(repository);
    }
    public static ISys_LanguageService Instance
    {
      get { return AutofacContainerModule.GetService<ISys_LanguageService>(); } }
    }
 }
