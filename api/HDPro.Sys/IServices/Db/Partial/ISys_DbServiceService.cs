/*
*所有关于Sys_DbService类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using System;

namespace HDPro.Sys.IServices
{
    public partial interface ISys_DbServiceService
    {
        WebResponseContent CreateDb(Guid id);
    }
 }
