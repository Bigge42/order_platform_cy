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
    public partial class vSys_DictionaryService : ServiceBase<vSys_Dictionary, IvSys_DictionaryRepository>, IvSys_DictionaryService, IDependency
    {
        public vSys_DictionaryService(IvSys_DictionaryRepository repository)
             : base(repository) 
        { 
           Init(repository);
        }
        public static IvSys_DictionaryService Instance
        {
           get { return AutofacContainerModule.GetService<IvSys_DictionaryService>(); }
        }
    }
}

