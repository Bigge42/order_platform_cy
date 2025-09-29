/*
*所有关于Sys_TableFieldDefinition类的业务代码接口应在此处编写
*/
using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace HDPro.Sys.IServices
{
    public partial interface ISys_TableFieldDefinitionService
    {
        Task<WebResponseContent> CreateTable(List<string> tableNames, string dbServiceName);
        Task<WebResponseContent> CreateAllTable(string dbServiceName);
        Task<WebResponseContent> GetAllTable();
        Task<WebResponseContent> GetTables(List<string> tableNames);
        Task<WebResponseContent> AddOrAlterFields(List<int> ids, string dbServiceName);
        Task<WebResponseContent> GetFieldsByTableName(string tableName);
    }
 }
