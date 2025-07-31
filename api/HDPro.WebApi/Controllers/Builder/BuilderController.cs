using HDPro.Builder.IServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using HDPro.Core.Filters;
using HDPro.Entity.DomainModels;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels.Core;
using Microsoft.AspNetCore.Authorization;

namespace HDPro.WebApi.Controllers.Builder
{
    [JWTAuthorize]
    [Route("/api/Builder")]
    public class BuilderController : Controller
    {
        private ISys_TableInfoService Service;
        public BuilderController(ISys_TableInfoService service)
        {
            Service = service;
        }
        [HttpPost]
        [Route("GetTableTree")]
        //[ApiActionPermission(ActionRolePermission.SuperAdmin)]
        public async Task<ActionResult> GetTableTree()
        {
            try
            {
                (string, string) builderInfo = await Service.GetTableTree();
                return Json(new { list = builderInfo.Item1, nameSpace = builderInfo.Item2 });
            }
            catch (Exception ex)
            {

                return Json(new { list = ex.Message + ex.StackTrace + ex.Source, nameSpace = ex.InnerException?.Message });
            }
        }

        [Route("CreateVuePage")]
        [ApiActionPermission(ActionRolePermission.SuperAdmin)]
        [HttpPost]
        public ActionResult CreateVuePage([FromBody] Sys_TableInfo sysTableInfo, string vuePath)
        {
            return Content(Service.CreateVuePage(sysTableInfo, vuePath));
        }
        [Route("CreateModel")]
        [ApiActionPermission(ActionRolePermission.SuperAdmin)]
        [HttpPost]
        public ActionResult CreateEntityModel([FromBody] Sys_TableInfo tableInfo)
        {
            return Content(Service.CreateEntityModel(tableInfo));
        }
        [Route("Save")]
        [ApiActionPermission(ActionRolePermission.SuperAdmin)]
        [HttpPost]
        public ActionResult SaveEidt([FromBody] Sys_TableInfo tableInfo)
        {
            return Json(Service.SaveEidt(tableInfo));
        }
        [Route("CreateServices")]
        [ApiActionPermission(ActionRolePermission.SuperAdmin)]
        [HttpPost]
        public ActionResult CreateServices(string tableName, string nameSpace, string foldername, bool? partial, bool? api)
        {
            return Content(Service.CreateServices(tableName, nameSpace, foldername, false, true));
        }
        [Route("LoadTableInfo")]
        [HttpPost]
        public ActionResult LoadTable(int parentId, string tableName, string columnCNName, string nameSpace, string foldername, int table_Id, bool isTreeLoad,string dbServer)
        {
            return Json(Service.LoadTable(parentId, tableName, columnCNName, nameSpace, foldername, table_Id, isTreeLoad,dbServer));

        }
        [Route("delTree")]
        [ApiActionPermission(ActionRolePermission.SuperAdmin)]
        [HttpPost]
        public async Task<ActionResult> DelTree(int table_Id)
        {
            return Json(await Service.DelTree(table_Id));
        }
        [Route("syncTable")]
        [ApiActionPermission(ActionRolePermission.SuperAdmin)]
        [HttpPost]
        public async Task<ActionResult> SyncTable(string tableName)
        {
            return Json(await Service.SyncTable(tableName));
        }
        /// <summary>
        /// 根据表字段定义生成代码
        /// </summary>
        /// <param name="param">代码生成参数</param>
        /// <returns></returns>
        [Route("GenerateCodeByFieldDefinition")]
        [HttpPost]
        public async Task<ActionResult> GenerateCodeByFieldDefinition([FromBody] GenerateCodeParam param)
        {
            if (param == null || param.TableNames == null || param.TableNames.Count == 0)
            {
                return Json(new WebResponseContent().Error("表名不能为空"));
            }
            if (param.ParentId <= 0)
            {
                return Json(new WebResponseContent().Error("表的父级ID不能为空"));
            }
            return Json(await Service.GenerateCodeByFieldDefinition(param));
        }
    }
}
