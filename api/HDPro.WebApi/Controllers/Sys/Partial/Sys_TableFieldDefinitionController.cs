/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Sys_TableFieldDefinition",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.Sys.IServices;
using Microsoft.AspNetCore.Authorization;

namespace HDPro.Sys.Controllers
{
    public partial class Sys_TableFieldDefinitionController
    {
        private readonly ISys_TableFieldDefinitionService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public Sys_TableFieldDefinitionController(
            ISys_TableFieldDefinitionService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        [Route("CreateAllTable")]
        [HttpPost]
        public async Task<ActionResult> CreateAllTable(string dbServiceName = "ServiceDbContext")
        {
            return Json(await Service.CreateAllTable(dbServiceName));
        }
        [Route("GetAllTable")]
        [HttpPost]
        public async Task<ActionResult> GetAllTable()
        {
            return Json(await Service.GetAllTable());
        }

        [Route("CreateTable")]
        [HttpPost]
        public async Task<ActionResult> CreateTable([FromBody] List<string> tableNames, string dbServiceName = "ServiceDbContext")
        {
            return Json(await Service.CreateTable(tableNames, dbServiceName));
        }
        [Route("GetTable"), AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> GetTables(List<string> tableNames)
        {
            return Json(await Service.GetTables(tableNames));
        }
        [Route("AddOrAlterFields")]
        [HttpPost]
        public async Task<ActionResult> AddOrAlterFields([FromBody] List<int> ids, string dbServiceName = "ServiceDbContext")
        {
            return Json(await Service.AddOrAlterFields(ids, dbServiceName));
        }

        /// <summary>
        /// 根据表名获取字段信息
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>返回格式：[{ key: "字段名", value: "字段中文名"}]</returns>
        [Route("GetFieldsByTableName")]
        [HttpGet]
        public async Task<ActionResult> GetFieldsByTableName(string tableName)
        {
            return Json(await Service.GetFieldsByTableName(tableName));
        }
    }
}
