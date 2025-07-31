using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.Core.SignalR
{
    public class MessagePushOptions
    {
        public bool EnableBackgroundPush { get; set; } = true;
        public int PushIntervalSeconds { get; set; } = 30;
    }
}
