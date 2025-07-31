using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions;
using HDPro.Core.Services;
using HDPro.Core.Utilities;
using HDPro.Entity.DomainModels;

namespace HDPro.Core.SignalR
{
    public class MessageChannel
    {
        private readonly Channel<MessageChannelData> _channel = Channel.CreateUnbounded<MessageChannelData>();


        public async Task Run(IHubContext<MessageHub> hubContext)
        {
            Console.WriteLine("消息推送服务已启动");
            while (await _channel.Reader.WaitToReadAsync())
            {
                if (_channel.Reader.TryRead(out MessageChannelData channelData))
                {
                    try
                    {
                        var client = hubContext.Clients.Clients(channelData.ConnectionIds);
                        if (string.IsNullOrEmpty(channelData.MessageNotification.Title))
                        {
                            channelData.MessageNotification.Title = channelData.MessageNotification.Content;
                        }
                        string message = channelData.MessageNotification.NotificationType == Enums.NotificationType.审批
                            && !string.IsNullOrEmpty(channelData.MessageNotification.Content) ?
                           channelData.MessageNotification.Content : channelData.MessageNotification.Title;
                        await client.SendAsync("ReceiveHomePageMessage", new
                        {
                            code = channelData.Code,
                            message,
                            //string.IsNullOrEmpty(channelData.MessageNotification.Title) ? channelData.MessageNotification.Content : channelData.MessageNotification.Title,
                            channelData.MessageNotification.NotificationType,
                            channelData.MessageNotification.BusinessFunction,
                            Title = message,
                            // channelData.MessageNotification.Title,
                            Date = DateTime.Now,
                            creator = channelData.MessageNotification.Creator
                        });
                        using var context = new SysDbContext();


                        var users = context.Set<Sys_User>().Where(x => channelData.UserName.Contains(x.UserName))
                              .Select(s => new { s.User_Id, s.UserName, s.UserTrueName }).ToList();

                        var list = channelData.UserName.Select(c => new Sys_NotificationLog()
                        {
                            NotificationLogId = Guid.NewGuid(),
                            BusinessFunction = channelData.MessageNotification.BusinessFunction ?? "系统",
                            NotificationId = channelData.MessageNotification.NotificationId,
                            NotificationContent = channelData.MessageNotification.Content,
                            NotificationTitle = channelData.MessageNotification.Title,
                            IsRead = 0,
                            LinkType = channelData.MessageNotification.LinkType,
                            LinkUrl = channelData.MessageNotification.LinkUrl,
                            NotificationLevel = channelData.MessageNotification.Level ?? "info",
                            NotificationType = channelData.MessageNotification.NotificationType.ToString(),
                            ReceiveUserId = users.Where(x => x.UserName == c).Select(x => x.User_Id).FirstOrDefault(),
                            //channelData.MessageNotification.ReceiveUserId,
                            ReceiveUserName = c,
                            ReceiveUserTrueName = users.Where(x => x.UserName == c).Select(x => x.UserTrueName).FirstOrDefault(),
                            //channelData.MessageNotification.ReceiveUserName,
                            CreateDate = DateTime.Now,
                            TableKey = channelData.MessageNotification.TableKey,
                            TableName = channelData.MessageNotification.TableName,
                            CreateID = channelData.MessageNotification.CreateID,
                            Creator = channelData.MessageNotification.Creator
                        }).ToList();
                        await context.AddRangeAsync(list);
                        await context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Logger.AddAsync($"Channel消息发送异常,data:{channelData.Serialize()},ex:{ex.Message + ex.StackTrace}");
                        Console.WriteLine("发送异常");
                    }
                }
            }
        }


        public void WriteMessage(MessageChannelData channelData)
        {
            _channel.Writer.TryWrite(channelData);
        }
        public async Task WriteMessageAsync(MessageChannelData channelData)
        {
            await _channel.Writer.WriteAsync(channelData);
        }
    }
}
