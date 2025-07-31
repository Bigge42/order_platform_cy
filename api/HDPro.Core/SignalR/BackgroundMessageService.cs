using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HDPro.Core.SignalR
{
    public class BackgroundMessageService : BackgroundService
    {
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly ILogger<BackgroundMessageService> _logger;
        private readonly MessageChannel _channel;
        public BackgroundMessageService(
       IHubContext<MessageHub> hubContext,
             MessageChannel channel,
            ILogger<BackgroundMessageService> logger)
        {
            _hubContext = hubContext;;
            _logger = logger;
            _channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _channel.Run(_hubContext);
        }
    }

}
