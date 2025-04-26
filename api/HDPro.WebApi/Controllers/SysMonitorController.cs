using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;
using HDPro.Core.Controllers.Basic;
using HDPro.Core.Extensions.AutofacManager;
using HDPro.Core.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace HDPro.WebApi.Controllers
{
    [Route("api/sysMonitor")]
    public class SysMonitorController: VolController
    {
        private IWebHostEnvironment _hostEnvironment;
        private IHttpContextAccessor _httpContextAccessor;
        public SysMonitorController(IWebHostEnvironment hostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _hostEnvironment = hostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost,Route("getInfo")]
        public object GetInfo()
        {
            string osArch = RuntimeInformation.OSArchitecture.ToString();
            string version = RuntimeInformation.FrameworkDescription;
            string appRAM = ((double)Process.GetCurrentProcess().WorkingSet64 / 1048576).ToString("N2") + " MB";
            string startTime = Process.GetCurrentProcess().StartTime.ToString("yyyy-MM-dd HH:mm:ss");
            string sysRunTime = ComputerHelper.GetRunTime();
            string serverIP = _httpContextAccessor.HttpContext.Connection.LocalIpAddress.MapToIPv4().ToString() + ":" + _httpContextAccessor.HttpContext.Connection.LocalPort;
            var programStartTime = Process.GetCurrentProcess().StartTime;
            string firstPart = (DateTime.Now - programStartTime).TotalMilliseconds.ToString().Split('.')[0];
            string programRunTime = ParseToLong(firstPart).ToString();
            var data = new
            {
                cpu = ComputerHelper.GetComputerInfo(),
                disk = ComputerHelper.GetDiskInfos(),
                sys = new { Environment.MachineName, RuntimeInformation.OSDescription, osArch, serverIP, runTime = sysRunTime },
                app = new
                {
                    name = _hostEnvironment.EnvironmentName,
                    rootPath = _hostEnvironment.ContentRootPath,
                    webRootPath = _hostEnvironment.WebRootPath,
                    version,
                    appRAM,
                    startTime,
                    runTime = programRunTime,
                    host = serverIP
                },
            };

            return data;
        }
        private long ParseToLong( object obj)
        {
            try
            {
                return long.Parse(obj.ToString());
            }
            catch
            {
                return 0L;
            }
        }
    }
}
