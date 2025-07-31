using HDPro.Core.BaseProvider;
using HDPro.Entity.DomainModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels.Core;

namespace HDPro.Builder.IServices
{
    public partial interface ISys_TableInfoService
    {
        Task<(string, string)> GetTableTree();
         
        string CreateEntityModel(Sys_TableInfo tableInfo);
         
        WebResponseContent SaveEidt(Sys_TableInfo sysTableInfo);  

        string CreateServices(string tableName, string nameSpace, string foldername, bool webController, bool apiController);


        string CreateVuePage(Sys_TableInfo sysTableInfo, string vuePath);

        object LoadTable(int parentId, string tableName, string columnCNName, string nameSpace, string foldername, int table_Id, bool isTreeLoad,string dbServer);
        Task<WebResponseContent> SyncTable(string tableName);
        Task<WebResponseContent> DelTree(int table_Id);

        /// <summary>
        /// 根据表字段定义生成代码
        /// </summary>
        /// <param name="param">代码生成参数</param>
        /// <returns>生成结果</returns>
        Task<WebResponseContent> GenerateCodeByFieldDefinition(GenerateCodeParam param);
    }
}
