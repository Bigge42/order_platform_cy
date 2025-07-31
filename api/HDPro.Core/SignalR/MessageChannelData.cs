using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HDPro.Entity.DomainModels;

namespace HDPro.Core.SignalR
{
    public class MessageChannelData
    {
        public List<string> UserName { get; set; }
        public List<int> UserIds { get; set; }
        public List<string> ConnectionIds { get; set; }
        public bool Status { get; set; }
        public string Code { get; set; }
        public MessageNotification MessageNotification { get; set; }

   
        //public Sys_NotificationLog NotificationLog { get; set; }
    }
    //public class MsgData
    //{
    //    public bool Status { get; set; }
    //    public string Code { get; set; }
    //    public string Message { get; set; }
    //    public string Link { get; set; }
    //    public string Content { get; set; }
    //    public string ExtraData { get; set; }
    //}
}
