/*
 *Author：jxx
 *Contact：461857658@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下TestDbService与ITestDbService中编写
 */
using HDPro.DbTest.IRepositories;
using HDPro.DbTest.IServices;
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.DbTest.Services
{
    public partial class TestDbService : ServiceBase<TestDb, ITestDbRepository>
    , ITestDbService, IDependency
    {
    public TestDbService(ITestDbRepository repository)
    : base(repository)
    {
    Init(repository);
    }
    public static ITestDbService Instance
    {
      get { return AutofacContainerModule.GetService<ITestDbService>(); } }
    }
 }
