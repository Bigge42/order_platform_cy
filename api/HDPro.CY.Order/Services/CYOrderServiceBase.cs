/*
 * CY.Order项目服务基类
 * 所有CY.Order项目的服务类都应继承此基类
 * 可在此处添加CY.Order项目特有的通用业务逻辑
 */
using HDPro.Core.BaseProvider;
using HDPro.Core.Extensions;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.Services;
using HDPro.Core.Utilities;
using HDPro.Entity;
using HDPro.Entity.DomainModels;
using HDPro.Entity.SystemModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace HDPro.CY.Order.Services
{
    /// <summary>
    /// CY.Order项目服务基类
    /// 提供CY.Order项目特有的通用业务逻辑和扩展功能
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <typeparam name="TRepository">仓储接口类型</typeparam>
    public abstract class CYOrderServiceBase<T, TRepository> : ServiceBase<T, TRepository>
        where T : BaseEntity
        where TRepository : IRepository<T>
    {
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public CYOrderServiceBase()
        {
        }

        public CYOrderServiceBase(TRepository repository, IHttpContextAccessor httpContextAccessor = null) 
            : base(repository)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// CY.Order项目通用初始化方法
        /// 可在此处添加项目特有的初始化逻辑
        /// </summary>
        protected override void Init(IRepository<T> repository)
        {
            base.Init(repository);
            // 在此处添加CY.Order项目特有的初始化逻辑
            InitCYOrderSpecific();
        }

        /// <summary>
        /// CY.Order项目特有的初始化逻辑
        /// 子类可重写此方法添加特定的初始化代码
        /// </summary>
        protected virtual void InitCYOrderSpecific()
        {
            // 可在此处添加CY.Order项目特有的初始化逻辑
            // 例如：设置特定的缓存策略、业务规则等
        }

        /// <summary>
        /// 获取当前用户的订单权限范围
        /// 可根据CY.Order项目的业务需求进行扩展
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> GetUserOrderScope()
        {
            // 在此处实现获取用户订单权限范围的逻辑
            // 例如：根据用户角色、部门等获取可访问的订单范围
            return new List<string>();
        }

        /// <summary>
        /// CY.Order项目通用数据验证方法
        /// 可在此处添加项目特有的数据验证逻辑
        /// </summary>
        /// <param name="entity">要验证的实体</param>
        /// <returns>验证结果</returns>
        protected virtual WebResponseContent ValidateCYOrderEntity(T entity)
        {
            var response = new WebResponseContent(true);
            
            // 在此处添加CY.Order项目特有的数据验证逻辑
            // 例如：订单状态验证、业务规则验证等
            
            return response;
        }

        /// <summary>
        /// CY.Order项目通用业务日志记录
        /// </summary>
        /// <param name="action">操作类型</param>
        /// <param name="entity">操作的实体</param>
        /// <param name="message">日志消息</param>
        protected virtual void LogCYOrderOperation(string action, T entity, string message = null)
        {
            try
            {
                // 在此处实现CY.Order项目特有的业务日志记录逻辑
                var entityName = typeof(T).Name;
                var logMessage = $"[CY.Order] {action} - {entityName}";
                if (!string.IsNullOrEmpty(message))
                {
                    logMessage += $" - {message}";
                }
                
                Logger.Info(logMessage);
            }
            catch (Exception ex)
            {
                Logger.Error($"记录CY.Order业务日志失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 重写新增方法，添加CY.Order项目特有的业务逻辑
        /// </summary>
        public override WebResponseContent Add(SaveModel saveDataModel)
        {
            // 在调用基类方法前，可以添加CY.Order项目特有的前置处理
            LogCYOrderOperation("Add", null, "开始新增操作");
            
            var result = base.Add(saveDataModel);
            
            // 在调用基类方法后，可以添加CY.Order项目特有的后置处理
            if (result.Status)
            {
                LogCYOrderOperation("Add", null, "新增操作成功");
            }
            else
            {
                LogCYOrderOperation("Add", null, $"新增操作失败: {result.Message}");
            }
            
            return result;
        }

        /// <summary>
        /// 重写更新方法，添加CY.Order项目特有的业务逻辑
        /// </summary>
        public override WebResponseContent Update(SaveModel saveModel)
        {
            // 在调用基类方法前，可以添加CY.Order项目特有的前置处理
            LogCYOrderOperation("Update", null, "开始更新操作");
            
            var result = base.Update(saveModel);
            
            // 在调用基类方法后，可以添加CY.Order项目特有的后置处理
            if (result.Status)
            {
                LogCYOrderOperation("Update", null, "更新操作成功");
            }
            else
            {
                LogCYOrderOperation("Update", null, $"更新操作失败: {result.Message}");
            }
            
            return result;
        }

        /// <summary>
        /// 重写删除方法，添加CY.Order项目特有的业务逻辑
        /// </summary>
        public override WebResponseContent Del(object[] keys, bool delList = true)
        {
            // 在调用基类方法前，可以添加CY.Order项目特有的前置处理
            LogCYOrderOperation("Delete", null, $"开始删除操作，Keys: {string.Join(",", keys)}");
            
            var result = base.Del(keys, delList);
            
            // 在调用基类方法后，可以添加CY.Order项目特有的后置处理
            if (result.Status)
            {
                LogCYOrderOperation("Delete", null, "删除操作成功");
            }
            else
            {
                LogCYOrderOperation("Delete", null, $"删除操作失败: {result.Message}");
            }
            
            return result;
        }

        /// <summary>
        /// 表体数量汇总至表体
        /// </summary>
        /// <returns></returns>
        public async Task<WebResponseContent> SummaryDetails2Head()
        {
            string procedureName = "P_" + typeof(T).GetEntityTableName() + "_Summary";           
            await repository.DapperContext.ExecuteScalarAsync(procedureName, null, CommandType.StoredProcedure);
            return WebResponseContent.Instance.OK("汇总成功！");
        }
    }
}  