using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HDPro.Core.EFDbContext;
using HDPro.Core.Enums;
using HDPro.Core.ManageUser;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;

namespace HDPro.Core.SignalR
{
    public class MessageService : IMessageService
    {
        public ConcurrentDictionary<string, int> Online = new ConcurrentDictionary<string, int>();
        public ConcurrentDictionary<string, string> ConnectionIds = new ConcurrentDictionary<string, string>();

        private readonly IHubContext<MessageHub> _hubContext;
        private readonly MessageChannel _channel;

        public MessageService(IHubContext<MessageHub> hubContext, MessageChannel channel)
        {
            _hubContext = hubContext;
            _channel = channel;
        }
        public async Task SendMessageAsync(MessageChannelData channelData)
        {
            GetChannelConnectionIds(channelData);
            await _channel.WriteMessageAsync(channelData);
        }
        public void SendMessage(MessageChannelData channelData)
        {
            GetChannelConnectionIds(channelData);
            _channel.WriteMessage(channelData);
        }

        public void SendMessage(List<int> toUserIds, string title, string content, NotificationType notificationType = NotificationType.系统, string code = null, string linkUrl = null, string linkType = null)
        {
            MessageChannelData channelData = new MessageChannelData()
            {
                UserIds = toUserIds,
                Code = code,
                MessageNotification = new MessageNotification()
                {
                    NotificationType = notificationType,
                    LinkUrl = linkUrl,
                    LinkType = linkType
                }
            };

            SendMessage(channelData);
        }

        public void SendAllMessage(MessageChannelData channelData)
        {

            channelData.UserName = ConnectionIds.Select(s => s.Value).ToList();
            channelData.ConnectionIds = ConnectionIds.Select(s => s.Key).ToList();
            _channel.WriteMessage(channelData);
        }

        private void GetChannelConnectionIds(MessageChannelData channelData)
        {
            if (channelData.MessageNotification == null)
            {
                channelData.MessageNotification = new MessageNotification();
            }
            if (channelData.MessageNotification.Creator == null && HttpContext.Current != null)
            {
                channelData.MessageNotification.Creator = UserContext.Current.UserTrueName;
                channelData.MessageNotification.CreateID = UserContext.Current.UserId;
            }
            if (channelData.UserIds != null)
            {
                using var context = new SysDbContext();
                channelData.UserName = context.Set<Sys_User>().Where(x => channelData.UserIds.Contains(x.User_Id))
                    .Select(s => s.UserName).ToList();
            }
            if (channelData.UserName != null)
            {
                channelData.ConnectionIds = channelData.UserName.SelectMany(s => GetConnectionIds(s)).ToList();
            }
        }


        /// <summary>
        /// 根据用户名获取所有的客户端
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public IEnumerable<string> GetConnectionIds(string username)
        {
            foreach (var item in ConnectionIds)
            {
                if (item.Value == username)
                {
                    yield return item.Key;
                }
            }
        }

        public int GetOnline(string username)
        {
            if (Online.TryGetValue(username, out int val))
            {
                return val;
            }
            return 0;
        }

        public void Add(HubCallerContext context)
        {
            string userName = context.GetHttpContext().Request.Query["userName"].ToString();
            if (string.IsNullOrEmpty(userName))
            {
                return;
            }
            Online[userName] = 1;
            ConnectionIds[context.ConnectionId] = userName;
        }

        public void RemoveCurrent()
        {
            //   _hubContext.Clients.
            string cid = "";// _hubContext.Clients.All.GetConnectionId();

            //移除缓存
            if (ConnectionIds.TryRemove(cid, out string value))
            {
                Online[value] = 0;
            }
        }
        public void Remove(string userName)
        {
            var list = GetConnectionIds(userName).ToList();

            foreach (var cid in list)
            {
                //移除缓存
                if (ConnectionIds.TryRemove(cid, out string value))
                {
                    Online[value] = 0;
                }
            }
        }
    }
}
