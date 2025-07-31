using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HDPro.Core.CacheManager;

namespace HDPro.Core.SignalR
{
    // 实现消息中心
    public class MessageHub : Hub
    {
        private readonly IMessageService _messageService;

        /// <summary>
        /// 构造 注入
        /// </summary>
        public MessageHub(IMessageService messageService)
        {
            _messageService = messageService;   
        }
        /// <summary>
        /// 请求
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            _messageService.Add(Context);
            return base.OnConnectedAsync();
        }
        /// <summary>
        /// 断开
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            _messageService.RemoveCurrent();
            return base.OnDisconnectedAsync(exception);
        }
    }

}
