using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HDPro.Core.Enums;

namespace HDPro.Core.SignalR
{
    public interface IMessageService
    {
        Task SendMessageAsync(MessageChannelData channelData);
        void SendMessage(MessageChannelData channelData);
        void SendMessage(List<int> toUserIds, string title, string content, NotificationType notificationType = NotificationType.系统,string code=null, string linkUrl = null, string linkType = null);
        /// <summary>
        /// 根据用户名获取所有的客户端
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        IEnumerable<string> GetConnectionIds(string username);

        int GetOnline(string username);

        void Add(HubCallerContext context);

        void RemoveCurrent();
        void Remove(string userName);
    }
}
