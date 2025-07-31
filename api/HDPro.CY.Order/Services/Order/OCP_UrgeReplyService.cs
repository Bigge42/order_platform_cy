/*
 *Author：jxx
 *Contact：461857658@qq.com
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下OCP_UrgeReplyService与IOCP_UrgeReplyService中编写
 */
using HDPro.CY.Order.IRepositories;
using HDPro.CY.Order.IServices;
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;

namespace HDPro.CY.Order.Services
{
    public partial class OCP_UrgeReplyService : CYOrderServiceBase<OCP_UrgeReply, IOCP_UrgeReplyRepository>
    , IOCP_UrgeReplyService, IDependency
    {
    public static IOCP_UrgeReplyService Instance
    {
      get { return AutofacContainerModule.GetService<IOCP_UrgeReplyService>(); } }
    }
 }
