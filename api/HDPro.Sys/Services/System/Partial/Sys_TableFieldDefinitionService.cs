/*
 *所有关于Sys_TableFieldDefinition类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Sys_TableFieldDefinitionService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Entity.DomainModels;
using System.Linq;
using HDPro.Core.Utilities;
using System.Linq.Expressions;
using HDPro.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Sys.IRepositories;
using Dapper;
using HDPro.Core.Configuration;
using HDPro.Core.DBManager;
using HDPro.Utilities.Extensions;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System;
using HDPro.Utilities;
using OfficeOpenXml;

namespace HDPro.Sys.Services
{
    public partial class Sys_TableFieldDefinitionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISys_TableFieldDefinitionRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public Sys_TableFieldDefinitionService(
            ISys_TableFieldDefinitionRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }
        public async Task<WebResponseContent> CreateAllTable(string dbServiceName)
        {
            var tables = await _repository.DbContext.Set<Sys_TableFieldDefinition>().ToListAsync();
            if (tables?.Count > 0)
            {
                var result = await CreateTable(tables, dbServiceName);
                return result;
            }
            else
            {
                return new WebResponseContent().Error("不存在相应表的定义");
            }
        }
        public async Task<WebResponseContent> GetAllTable()
        {
            var tables = await _repository.DbContext.Set<Sys_TableFieldDefinition>().Select(p => new { p.TableName, p.TableCName }).Distinct().ToListAsync();
            if (tables?.Count > 0)
            {

                return new WebResponseContent().OKData(tables);
            }
            else
            {
                return new WebResponseContent().Error("不存在相应表的定义");
            }
        }

        public async Task<WebResponseContent> CreateTable(List<string> tableNames, string dbServiceName)
        {
            var tables = await _repository.FindAsync(p => tableNames.Contains(p.TableName));
            if (tables?.Count > 0)
            {
                var result = await CreateTable(tables, dbServiceName);
                return result;
            }
            else
            {
                return new WebResponseContent().Error("不存在相应表的定义");
            }
        }
        public async Task<WebResponseContent> AddOrAlterFields(List<int> ids, string dbServiceName)
        {
            var result = new WebResponseContent();
            var fields = await _repository.FindAsync(p => ids.Contains(p.Id));
            StringBuilder strb = new StringBuilder();
            StringBuilder strbDescription = new StringBuilder();
            foreach (var field in fields)
            {
                if (field.IsKey == 1) continue;
                var fieldNullable = field.IsNullable == 1 ? "NULL" : "NOT NULL";
                var fieldDefaultValue = GetDefaultValueByFieldType(field.DataType, field.DefaultValue);
                bool isExist = await IsExistField(field, dbServiceName);
                if (!isExist)
                {
                    strb.AppendLine($" alter table {field.TableName} add  {field.FieldName} {field.DataType} {fieldNullable}  {fieldDefaultValue};");
                    strbDescription.AppendLine($"EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'{field.FieldCName}' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{field.TableName}', @level2type=N'COLUMN',@level2name=N'{field.FieldName}';");
                }
                else
                {

                    strb.AppendLine($" alter table {field.TableName} alter column  {field.FieldName} {field.DataType} {fieldNullable} ;");
                    await DropExistsDefaultConstraint(field, dbServiceName);
                    if (!fieldDefaultValue.IsNullOrEmptyOrWhiteSpace())
                    {
                        //strb.AppendLine($"alter table {field.TableName} add constraint df_{field.FieldName}_{_idWorker.GenerateId()} {fieldDefaultValue} for {field.FieldName};");
                    }
                }
            }
            HDLogHelper.Log("CreateTable", strb.ToString());
            HDLogHelper.Log("FieldDescription", strbDescription.ToString());
            try
            {
                var dapper = DBServerProvider.GetSqlDapperWidthDbService(dbServiceName);
                await dapper.ExcuteNonQueryAsync(strb.ToString(), null);
                if (strbDescription.Length > 0)
                {
                    await dapper.ExcuteNonQueryAsync(strbDescription.ToString(), null);
                }
            }
            catch (Exception ex)
            {
                HDLogHelper.Log(fileName: "error", ex.Message + ex.StackTrace);
                return result.Error($"字段创建或修改失败：{ex.Message}");
            }
            return result.OK("字段创建或修改成功");
        }

        private async Task DropExistsDefaultConstraint(Sys_TableFieldDefinition field, string dbServiceName)
        {
            //判断表字段是否存在
            string strSql = @$"
SELECT  dc.name as default_constraint_name
FROM    sys.tables t
        INNER JOIN sys.columns c ON t.object_id = c.object_id
        INNER JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
WHERE   t.name = '{field.TableName}'
        AND c.name = '{field.FieldName}'
";
            var dapper = DBServerProvider.GetSqlDapperWidthDbService(dbServiceName);
            var constraintName = await dapper.ExecuteScalarAsync(strSql, null);
            if (!Convert.ToString(constraintName).IsNullOrEmptyOrWhiteSpace())
            {
                var constriantNameStr = Convert.ToString(constraintName);
                HDLogHelper.Log("DefaultConstraint", constriantNameStr);
                await dapper.ExcuteNonQueryAsync($"alter table {field.TableName} drop constraint {constriantNameStr}", null);
            }
        }

        private async Task<bool> IsExistField(Sys_TableFieldDefinition field, string dbServiceName)
        {
            //判断表字段是否存在
            string strSql = @"select 1 from syscolumns where id=object_id(@tableName) and name=@fieldName";
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@tableName", field.TableName);
            parameters.Add("@fieldName", field.FieldName);
            var dapper = DBServerProvider.GetSqlDapperWidthDbService(dbServiceName);
            var a = await dapper.ExecuteScalarAsync(strSql, parameters);
            bool isExist = Convert.ToInt32(a) > 0;
            return isExist;
        }

        private async Task<WebResponseContent> CreateTable(List<Sys_TableFieldDefinition> tables, string dbServiceName)
        {
            var result = new WebResponseContent();
            StringBuilder strb = new StringBuilder();
            StringBuilder strbDescription = new StringBuilder();
            foreach (var tablefields in tables.GroupBy(p => p.TableName))
            {
                bool isExist = await IsExistTable(tablefields.Key, dbServiceName);
                if (!isExist)
                {
                    strb.AppendLine($"CREATE TABLE {tablefields.Key} (");
                    if (!tablefields.Any(p => p.IsKey == 1))
                    {
                        strb.AppendLine($" Id  IDENTITY(1,1)  PRIMARY KEY NOT NULL,");
                    }

                    foreach (var field in tablefields)
                    {
                        if (field.IsKey == 1)
                        {
                            strb.AppendLine($" {field.FieldName} {field.DataType} IDENTITY(1,1)  PRIMARY KEY NOT NULL,");
                        }
                        else
                        {
                            var fieldNullable = field.IsNullable == 1 ? "NULL" : "NOT NULL";
                            var fieldDefaultValue = GetDefaultValueByFieldType(field.DataType, field.DefaultValue);
                            strb.AppendLine($" {field.FieldName} {field.DataType} {fieldNullable}  {fieldDefaultValue},");
                        }
                        strbDescription.AppendLine($"EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'{field.FieldCName}' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{field.TableName}', @level2type=N'COLUMN',@level2name=N'{field.FieldName}';");
                    }
                    CreateDefaultMemberField(tablefields.ToList(), strb);
                    strb.AppendLine($");");
                }
            }
            if (strb.Length > 0)
            {
                HDLogHelper.Log("CreateTable", strb.ToString());
                HDLogHelper.Log("FieldDescription", strbDescription.ToString());
                try
                {
                    var dapper = DBServerProvider.GetSqlDapperWidthDbService(dbServiceName);
                    await dapper.ExcuteNonQueryAsync(strb.ToString(), null);
                    await dapper.ExcuteNonQueryAsync(strbDescription.ToString(), null);
                }
                catch (Exception ex)
                {
                    HDLogHelper.Log(fileName: "error", ex.Message + ex.StackTrace);
                    return result.Error($"表创建失败：{ex.Message}");
                }
                return result.OK("表创建成功");
            }
            return result.OK("表已存在无需创建");
        }

        private string GetDefaultValueByFieldType(string dataType, string defaultValue)
        {
            string result = "";
            if (defaultValue.IsNullOrEmptyOrWhiteSpace())
            {
                if (dataType.Contains("decimal", StringComparison.OrdinalIgnoreCase) || dataType.Contains("int", StringComparison.OrdinalIgnoreCase))
                {
                    result = "default(0)";
                }
            }
            else
            {
                if (dataType.Contains("nvarchar", StringComparison.OrdinalIgnoreCase))
                {
                    defaultValue = defaultValue.Replace("'", "").Replace('"', ' ').Trim();
                    result = $"default('{defaultValue}')";
                }
                if (dataType.Contains("decimal", StringComparison.OrdinalIgnoreCase) || dataType.Contains("int", StringComparison.OrdinalIgnoreCase))
                {
                    defaultValue = defaultValue.Replace("'", "").Replace('"', ' ').Trim();
                    result = $"default({defaultValue})";
                }
            }

            return result;
        }

        private void CreateDefaultMemberField(List<Sys_TableFieldDefinition> sys_TableFieldDefinitions, StringBuilder strb)
        {
            if (!sys_TableFieldDefinitions.Any(p => p.FieldName.Equals(AppSetting.CreateMember.UserNameField, StringComparison.OrdinalIgnoreCase)))
            {
                strb.AppendLine($" {AppSetting.CreateMember.UserNameField} nvarchar(200),");
            }
            if (!sys_TableFieldDefinitions.Any(p => p.FieldName.Equals(AppSetting.CreateMember.UserIdField, StringComparison.OrdinalIgnoreCase)))
            {
                strb.AppendLine($" {AppSetting.CreateMember.UserIdField} int,");
            }
            if (!sys_TableFieldDefinitions.Any(p => p.FieldName.Equals(AppSetting.CreateMember.DateField, StringComparison.OrdinalIgnoreCase)))
            {
                strb.AppendLine($" {AppSetting.CreateMember.DateField} datetime,");
            }
            if (!sys_TableFieldDefinitions.Any(p => p.FieldName.Equals(AppSetting.ModifyMember.UserNameField, StringComparison.OrdinalIgnoreCase)))
            {
                strb.AppendLine($" {AppSetting.ModifyMember.UserNameField} nvarchar(200),");
            }
            if (!sys_TableFieldDefinitions.Any(p => p.FieldName.Equals(AppSetting.ModifyMember.UserIdField, StringComparison.OrdinalIgnoreCase)))
            {
                strb.AppendLine($" {AppSetting.ModifyMember.UserIdField} int,");
            }
            if (!sys_TableFieldDefinitions.Any(p => p.FieldName.Equals(AppSetting.ModifyMember.DateField, StringComparison.OrdinalIgnoreCase)))
            {
                strb.AppendLine($" {AppSetting.ModifyMember.DateField} datetime,");
            }
        }

        private async Task<bool> IsExistTable(string tableName, string dbServiceName)
        {
            var dapper = DBServerProvider.GetSqlDapperWidthDbService(dbServiceName);
            //判断表是否存在，不存在则生成相应的创建语句
            string strSql = @"select  count(*)  from  dbo.sysobjects where name=@tableName;";
            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@tableName", tableName);
            var a = await dapper.ExecuteScalarAsync(strSql, parameters);
            bool isExist = Convert.ToInt32(a) > 0;
            return isExist;
        }


       
        //
        public async Task<WebResponseContent> GetTables(List<string> tableNames)
        {
            var tables = await _repository.FindAsync(p => tableNames.Contains(p.TableName));
            return new WebResponseContent().OK("OK", tables);
        }

        /// <summary>
        /// 根据表名获取字段信息，返回前端需要的格式
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>返回格式：[{ key: "字段名", value: "字段中文名"}]</returns>
        public async Task<WebResponseContent> GetFieldsByTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return new WebResponseContent().Error("表名不能为空");
            }

            var fields = await _repository.FindAsync(p => p.TableName == tableName);
            
            if (fields == null || !fields.Any())
            {
                return new WebResponseContent().Error($"未找到表 {tableName} 的字段定义");
            }

            var result = fields.Select(f => new 
            { 
                key = f.FieldName, 
                value = f.FieldCName 
            }).ToList();

            return new WebResponseContent().OK("获取成功", result);
        }
        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public override WebResponseContent Import(List<IFormFile> files)
        {
            
            //导入保存前处理(可以对list设置新的值)
            ImportOnExecuting = (List<Sys_TableFieldDefinition> list) =>
            {
                // 校验导入数据中的重复性：表名TableName+字段名FieldName
                var duplicateItems = list
                    .GroupBy(x => new { x.TableName, x.FieldName })
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateItems.Any())
                {
                    var duplicateInfo = string.Join("；", duplicateItems.Select(d => $"表名：{d.TableName}，字段名：{d.FieldName}"));
                    return WebResponseContent.Instance.Error($"导入数据中存在重复的表名和字段名组合：{duplicateInfo}");
                }

                return WebResponseContent.Instance.OK();
            };

         
            return base.Import(files);
        }
        protected override void SaveImportData(List<Sys_TableFieldDefinition> list)
        {
            List<Sys_TableFieldDefinition> updateList = new List<Sys_TableFieldDefinition>();
            List<Sys_TableFieldDefinition> addList = new List<Sys_TableFieldDefinition>();
            foreach (var item in list)
            {
                var dbItem = _repository.Find(p => p.TableName == item.TableName && p.FieldName == item.FieldName).FirstOrDefault();
                if (dbItem != null)
                {
                    dbItem.TableCName = item.TableCName;
                    dbItem.FieldCName = item.FieldCName;
                    dbItem.DefaultValue = item.DefaultValue;
                    dbItem.DataType = item.DataType;
                    dbItem.Bak = item.Bak;
                    dbItem.IsKey = item.IsKey;
                    dbItem.IsNullable = item.IsNullable;
              
                    updateList.Add(dbItem);
                }
                else
                {
                    addList.Add(item);
                }
            }
            if (updateList.Count > 0)
            {
                _repository.UpdateRange(updateList, true);
            }
            if (addList.Count > 0)
            {
                _repository.AddRange(addList, true);
            }
        }
    }
}
