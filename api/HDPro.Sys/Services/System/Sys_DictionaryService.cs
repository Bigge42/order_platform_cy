/*
 *Author：jxx
 *Contact：461857658@qq.com
 *Date：2018-07-01
 * 此代码由框架生成，请勿随意更改
 */
using HDPro.Sys.IRepositories;
using HDPro.Sys.IServices;
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.Sys.Services
{
    public partial class Sys_DictionaryService : ServiceBase<Sys_Dictionary, ISys_DictionaryRepository>, ISys_DictionaryService, IDependency
    {
        public Sys_DictionaryService(ISys_DictionaryRepository repository)
             : base(repository) 
        { 
           Init(repository);
        }
        public static ISys_DictionaryService Instance
        {
           get { return AutofacContainerModule.GetService<ISys_DictionaryService>(); }
        }
    }
}

