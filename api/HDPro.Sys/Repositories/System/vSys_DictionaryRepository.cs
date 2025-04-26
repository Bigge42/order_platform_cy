/*
 *Author：jxx
 *Contact：461857658@qq.com
 *Date：2018-07-01
 * 此代码由框架生成，请勿随意更改
 */
using HDPro.Sys.IRepositories;
using HDPro.Core.BaseProvider;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.Sys.Repositories
{
    public partial class vSys_DictionaryRepository : RepositoryBase<vSys_Dictionary>, IvSys_DictionaryRepository
    {
        public vSys_DictionaryRepository(SysDbContext dbContext)
        : base(dbContext)
        {

        }
        public static IvSys_DictionaryRepository Instance
        {
            get { return AutofacContainerModule.GetService<IvSys_DictionaryRepository>(); }
        }
    }
}

