using HDPro.Builder.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using HDPro.Core.Const;
using HDPro.Core.DBManager;
using HDPro.Core.Enums;
using HDPro.Core.Extensions;
using HDPro.Core.ManageUser;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;
using HDPro.Entity.DomainModels.Sys;
using HDPro.Entity.SystemModels;
using HDPro.Core.EFDbContext;
using HDPro.Core.Configuration;
using HDPro.Entity.DomainModels.Core;
using HDPro.Utilities.Extensions;

namespace HDPro.Builder.Services
{
    public partial class Sys_TableInfoService
    {

        /// <summary>
        /// 根据表字段定义生成代码
        /// </summary>
        /// <param name="param">代码生成参数实体</param>
        /// <returns>生成结果</returns>
        public async Task<WebResponseContent> GenerateCodeByFieldDefinition(GenerateCodeParam param)
        {
            if (param == null)
            {
                return new WebResponseContent().Error("参数不能为空");
            }

            if (param.TableNames == null || param.TableNames.Count == 0)
            {
                return new WebResponseContent().Error("表名列表不能为空");
            }

            if (string.IsNullOrEmpty(param.DBServer))
            {
                return new WebResponseContent().Error("数据库服务名不能为空");
            }

            if (string.IsNullOrEmpty(param.BaseClassName))
            {
                param.BaseClassName = "ServiceBase";
            }

            if (string.IsNullOrEmpty(param.NameSpace) || string.IsNullOrEmpty(param.FolderName))
            {
                return new WebResponseContent().Error("命名空间和文件夹名不能为空");
            }

            if (string.IsNullOrEmpty(param.VuePath))
            {
                return new WebResponseContent().Error("Vue路径不能为空");
            }

            if (string.IsNullOrEmpty(param.SortName))
            {
                param.SortName = "Id";
            }

            List<(string tableName, string message)> resultList = new List<(string, string)>();

            // 获取Sys_TableFieldDefinition表中的数据
            var tableFieldDefinitions = await repository.DbContext.Set<Sys_TableFieldDefinition>()
                .Where(p => param.TableNames.Contains(p.TableName))
                .ToListAsync();

            if (tableFieldDefinitions.Count == 0)
            {
                return new WebResponseContent().Error("未找到指定表的字段定义");
            }

            // 按表名分组
            foreach (var tableGroup in tableFieldDefinitions.GroupBy(p => p.TableName))
            {
                string tableName = tableGroup.Key;
                List<string> messages = new List<string>();
                string auth = JsonConvert.SerializeObject(GetDefaultAuth(tableName));

                try
                {
                    // 检查表是否已存在
                    var existingTable = await repository.FindAsIQueryable(x => x.TableName == tableName)
                         .Include(c => c.TableColumns)
                        .FirstOrDefaultAsync();

                    Sys_TableInfo tableInfo;
                    if (existingTable != null)
                    {
                        tableInfo = existingTable;
                        messages.Add($"表{tableName}的定义已存在,请到代码生成页面进行操作。");
                        resultList.Add((tableName, string.Join(";", messages)));
                        continue;
                    }
                    else
                    {
                        // 创建TableInfo
                        tableInfo = new Sys_TableInfo()
                        {
                            ParentId = param.ParentId,
                            ColumnCNName = tableGroup.First().TableCName,
                            CnName = tableGroup.First().TableCName,
                            TableName = tableName,
                            TableTrueName = tableName,
                            Namespace = param.NameSpace,
                            FolderName = param.FolderName,
                            Enable = 1,
                            DBServer = param.DBServer,
                            SortName = param.SortName,
                        };

                        // 保存TableInfo
                        await repository.AddAsync(tableInfo);
                        await repository.SaveChangesAsync();
                        messages.Add($"成功创建表{tableName}的基本信息");

                        // 调用SyncTable方法同步表结构
                        var syncResult = await SyncTable(tableName);
                        if (syncResult.Status)
                        {
                            messages.Add($"成功同步表{tableName}的结构信息");
                        }
                        else
                        {
                            messages.Add($"同步表{tableName}的结构信息失败：{syncResult.Message}");
                            resultList.Add((tableName, string.Join(";", messages)));
                            continue;
                        }

                        // 重新获取包含列信息的完整表信息
                        tableInfo = await repository.FindAsIQueryable(x => x.TableName == tableName)
                            .Include(c => c.TableColumns)
                            .FirstOrDefaultAsync();

                        if (tableInfo == null || tableInfo.TableColumns == null || tableInfo.TableColumns.Count == 0)
                        {
                            messages.Add($"未能获取到表{tableName}的列信息");
                            resultList.Add((tableName, string.Join(";", messages)));
                            continue;
                        }

                        // 设置列的显示顺序和编辑、查询行列
                        int orderNo = (tableInfo.TableColumns.Count + 10) * 50;

                        // 忽略一些系统字段
                        List<string> ignoreFields = new List<string>() { "Id",
                            AppSetting.CreateMember.DateField, AppSetting.CreateMember.UserIdField, AppSetting.CreateMember.UserNameField,
                            AppSetting.ModifyMember.DateField, AppSetting.ModifyMember.UserIdField, AppSetting.ModifyMember.UserNameField,
                        };

                        // 计算有效列数（排除ignoreFields）
                        int effectiveColumnCount = tableInfo.TableColumns.Count(c => !ignoreFields.Contains(c.ColumnName));
                        
                        // 确定每行显示的列数
                        int colsPerRow = effectiveColumnCount > 10 ? 3 : 1;
                        int colsPerSearchRow =4;
                        // 用于跟踪行号和列号
                        int currentEditRowNo = 0;
                        int currentEditColNo = 0;
                        int currentSearchRowNo = 0;
                        int currentSearchColNo = 0;

                        foreach (var column in tableInfo.TableColumns)
                        {
                            column.OrderNo = orderNo;
                            orderNo = orderNo - 50;
                            if (column.ColumnCnName.IsNullOrEmptyOrWhiteSpace())
                            {
                                switch (column.ColumnName)
                                {
                                    case "Creator":
                                        column.ColumnCnName = "创建人";
                                        break;
                                    case "CreateId":
                                        column.ColumnCnName = "创建人内码";
                                        break;
                                    case "CreateDate":
                                        column.ColumnCnName = "创建日期";
                                        break;
                                    case "Modifier":
                                        column.ColumnCnName = "修改人";
                                        break;
                                    case "ModifyID":
                                        column.ColumnCnName = "修改人内码";
                                        break;
                                    case "ModifyDate":
                                        column.ColumnCnName = "修改时间";
                                        break;
                                    default:
                                        break;
                                }
                            }

                            // 忽略编辑、查询列
                            if (ignoreFields.Contains(column.ColumnName))
                            {
                                column.EditRowNo = null;
                                column.EditColNo = null;
                                column.SearchRowNo = null;
                                column.SearchColNo = null;
                            }
                            else
                            {
                                // 设置行列号
                                column.EditRowNo = currentEditRowNo;
                                column.EditColNo = currentEditColNo;
                                column.SearchRowNo = currentSearchRowNo;
                                column.SearchColNo = currentSearchColNo;
                                
                                // 更新列号，如果达到每行列数上限，则换行
                                currentEditColNo++;
                                currentSearchColNo++;
                                if (currentEditColNo >= colsPerRow)
                                {
                                    currentEditColNo = 0;
                                    currentEditRowNo+=10;
                                }
                                if (currentSearchColNo >= colsPerSearchRow)
                                {
                                    currentSearchColNo = 0;
                                    currentSearchRowNo+=10;
                                }
                                
                                // 设置编辑类型和查询类型
                                if (column.ColumnType.ToLower() == "datetime")
                                {
                                    column.SearchType = "datetime";
                                    column.EditType = "datetime";
                                }
                                else
                                {
                                    column.SearchType = "like";
                                }

                                // 设置sorttable
                                column.Sortable = 1;
                            }

                            // 根据Sys_TableFieldDefinition更新列信息
                            var fieldDef = tableGroup.FirstOrDefault(f => f.FieldName == column.ColumnName);
                            if (fieldDef != null)
                            {
                                column.ColumnCnName = fieldDef.FieldCName;
                            }
                        }

                        // 更新列信息
                        repository.UpdateRange(tableInfo.TableColumns);
                        await repository.SaveChangesAsync();
                        foreach (var item in tableInfo.TableColumns)
                        {
                            repository.DbContext.Entry(item).State = EntityState.Detached;
                        }
                    }

                    // 生成实体模型
                    string entityResult = CreateEntityModel(tableInfo);
                    messages.Add($"实体模型生成结果：{entityResult}");

                    // 生成服务
                    string serviceResult = CreateServices(tableName, param.NameSpace, param.FolderName, false, true);
                    messages.Add($"服务生成结果：{serviceResult}");

                    // 生成Vue页面
                    string vueResult = CreateVuePage(tableInfo, param.VuePath);
                    messages.Add($"Vue页面生成结果：{vueResult}");

                    // 创建菜单
                    var sysMenu = await repository.DbContext.Set<Sys_Menu>()
                        .FirstOrDefaultAsync(p => p.TableName == tableName);

                    if (sysMenu == null)
                    {
                        sysMenu = new Sys_Menu()
                        {
                            TableName = tableName,
                            Url = $"/{tableName}",
                            MenuName = tableInfo.CnName,
                            MenuType = 0,
                            Menu_Id = 0,
                            ParentId = param.ParentMenuId ?? 0, // 使用传入的父菜单ID
                            Auth = auth,
                            Enable = 1,
                            OrderNo = 0
                        };
                        repository.DbContext.Add(sysMenu);
                        await repository.DbContext.SaveChangesAsync();
                        messages.Add("成功创建菜单");
                    }
                    else
                    {
                        messages.Add($"菜单已存在");
                    }
                }
                catch (Exception ex)
                {
                    messages.Add($"处理表{tableName}时出错：{ex.Message}");
                }

                resultList.Add((tableName, string.Join(";", messages)));
            }

            return new WebResponseContent().OK("代码生成完成", resultList);
        }
            
        private List<object> GetDefaultAuth(string tableName = null)
        {
            List<object> result = new List<object>();
            result.Add(new { text = "查询", value = "Search" });
            result.Add(new { text = "新建", value = "Add" });
            result.Add(new { text = "删除", value = "Delete" });
            result.Add(new { text = "复制", value = "CopyData" });
            result.Add(new { text = "编辑", value = "Update" });
            result.Add(new { text = "导入", value = "Import" });
            result.Add(new { text = "导出", value = "Export" });
            return result;
        }
    }
   
}

