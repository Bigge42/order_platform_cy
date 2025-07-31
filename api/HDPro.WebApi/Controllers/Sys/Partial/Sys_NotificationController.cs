/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Sys_Notification",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using HDPro.Entity.DomainModels;
using HDPro.Sys.IServices;
using HDPro.Sys.IRepositories;
using Microsoft.AspNetCore.SignalR;
using HDPro.WebApi.Controllers.Hubs;
using HDPro.Core;
using HDPro.Core.SignalR;
using HDPro.Core.ManageUser;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using HDPro.Core.Enums;
using static Dapper.SqlMapper;
using System.Linq.Expressions;
using HDPro.Core.Extensions;

namespace HDPro.Sys.Controllers
{
    public partial class Sys_NotificationController
    {
        private readonly ISys_NotificationService _service;//访问业务代码
        private readonly ISys_NotificationRepository _repository;//访问业务代码
        private readonly ISys_NotificationLogRepository _logRepository;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMessageService _messageService;

        [ActivatorUtilitiesConstructor]
        public Sys_NotificationController(
            ISys_NotificationService service,
            ISys_NotificationRepository repository,
            ISys_NotificationLogRepository logRepository,
            IHttpContextAccessor httpContextAccessor,
            IMessageService messageService
        )
        : base(service)
        {
            _messageService = messageService;
            _service = service;
            _repository = repository;
            _logRepository = logRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost, Route("send")]
        public IActionResult Send([FromBody] Sys_Notification notification)
        {
            if (notification.PublishStatus.Value == 0)
            {
                notification.PublishStatus = 1;
                _repository.Update(notification, x => new { x.PublishStatus }, true);
            }
            if (notification.NotificationType == null)
            {
                notification.NotificationType = NotificationType.系统.ToString();
            }
            var data = new MessageChannelData()
            {
                UserName = null,
                MessageNotification = new MessageNotification()
                {
                    NotificationType = (NotificationType)Enum.Parse(typeof(NotificationType), notification.NotificationType),
                    NotificationId = notification.NotificationId,
                    Title = notification.NotificationTitle,
                    Creator = UserContext.Current.UserTrueName,
                    BusinessFunction = notification.BusinessFunction,
                    LinkType = notification.LinkType,
                    Content = notification.NotificationContent,
                    LinkUrl = notification.LinkUrl,
                    Level = notification.LinkUrl,
                    Remark = notification.Remark,
                    TableKey = notification.TableKey,
                    TableName = notification.TableName,
                }
            };
            if (notification.TargetObjectValue != null)
            {
                if (notification.TargetObjectType == ((int)NotificationTarget.用户).ToString())
                {
                    data.UserIds = notification.TargetObjectValue.Split(',').Select(s => s.GetInt()).ToList();
                }
                else if (notification.TargetObjectType == ((int)NotificationTarget.角色).ToString())
                {
                    var roleIds = notification.TargetObjectValue.Split(',').Select(s => s.GetInt()).ToList();
                    data.UserIds = _repository.DbContext.Set<Sys_UserRole>().Where(x => roleIds.Contains(x.RoleId) && x.Enable == 1)
                        .Select(s => s.UserId).Distinct().Take(500).ToList();
                }
                else if (notification.TargetObjectType == ((int)NotificationTarget.部门).ToString())
                {
                    var deptIds = notification.TargetObjectValue.Split(',').Select(s => s.GetGuid()).ToList();
                    data.UserIds = _repository.DbContext.Set<Sys_UserDepartment>().Where(x => deptIds.Contains(x.DepartmentId) && x.Enable == 1)
                        .Select(s => s.UserId).Distinct().Take(500).ToList();
                }
                else if (notification.TargetObjectType == ((int)NotificationTarget.岗位).ToString())
                {
                    var postIds = notification.TargetObjectValue.Split(',').Select(s => s.GetGuid()).ToList();
                    data.UserIds = _repository.DbContext.Set<Sys_UserPost>().Where(x => postIds.Contains(x.PostId) && x.Enable == 1)
                        .Select(s => s.UserId).Distinct().Take(500).ToList();
                }
            }
            _messageService.SendMessage(data);
            return Content("发送成功");
        }
        /// <summary>
        /// 获取全部消息
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("getList")]
        public async Task<IActionResult> GetList()
        {
            return Json(await GetData(x => true, true));
        }
        /// <summary>
        /// 获取未读消息
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("getUnreadList")]
        public async Task<IActionResult> GetUnreadList()
        {
            return Json(await GetData(x => x.IsRead == 0));
        }
        /// <summary>
        /// 获取全部审批消息
        /// </summary>
        /// <returns></returns>
        [HttpPost, Route("getAuditList")]
        public async Task<IActionResult> GetAuditList()
        {
            return Json(await GetData(x => x.NotificationType == NotificationType.审批.ToString(), true));
        }

        private async Task<object> GetData(Expression<Func<Sys_NotificationLog, bool>> predicate, bool total = false)
        {
            //NotificationType.审批
            var query = _logRepository.FindAsIQueryable(x => x.ReceiveUserId == UserContext.Current.UserId&&x.NotificationType==NotificationType.通知.ToString());
            var data = new
            {
                total = total ? await query.Where(x => x.IsRead == 0).CountAsync() : 0,
                auditTotal = total ? await query.Where(x => x.NotificationType == NotificationType.审批.ToString()).CountAsync() : 0,
                list = await query.Where(predicate)
                .OrderByDescending(x => x.CreateDate).Take(50).Select(s => new
                {
                    id = s.NotificationLogId,
                    s.IsRead,
                    s.TableName,
                    s.TableKey,
                    s.NotificationTitle,
                    s.NotificationType,
                    s.NotificationContent,
                    s.NotificationLevel,
                    s.LinkType,
                    s.LinkUrl,
                    s.Creator,
                    s.CreateDate
                }).ToListAsync(),
            };
            return data;
        }


        [HttpGet, Route("read")]
        public async Task<IActionResult> Read(Guid id)
        {
            await _logRepository.FindAsIQueryable(x => x.NotificationLogId == id)
                 .ExecuteUpdateAsync(x => x.SetProperty(c => c.IsRead, 1).SetProperty(c => c.ReadDate, DateTime.Now));
            return Content("ok");
        }
    }
}
