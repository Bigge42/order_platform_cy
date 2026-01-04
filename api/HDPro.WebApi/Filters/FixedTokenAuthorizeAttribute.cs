using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using HDPro.WebApi.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HDPro.WebApi.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class FixedTokenAuthorizeAttribute : TypeFilterAttribute
    {
        public FixedTokenAuthorizeAttribute() : base(typeof(FixedTokenAuthorizeFilter))
        {
        }
    }

    public sealed class FixedTokenAuthorizeFilter : IAuthorizationFilter
    {
        private readonly FixedTokenAuthOptions _options;
        private readonly ILogger<FixedTokenAuthorizeFilter> _logger;

        public FixedTokenAuthorizeFilter(IOptions<FixedTokenAuthOptions> options, ILogger<FixedTokenAuthorizeFilter> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!_options.Enabled)
            {
                return;
            }

            var expectedToken = _options.Token;
            var headerName = string.IsNullOrWhiteSpace(_options.HeaderName) ? "X-Fixed-Token" : _options.HeaderName;
            var providedToken = context.HttpContext.Request.Headers[headerName].FirstOrDefault();

            if (!IsTokenValid(expectedToken, providedToken))
            {
                context.Result = BuildUnauthorizedResult();
                return;
            }

            if (_options.AllowedCidrs != null && _options.AllowedCidrs.Any())
            {
                var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
                if (!IsIpAllowed(remoteIp, _options.AllowedCidrs))
                {
                    _logger.LogWarning("Fixed token rejected IP: {RemoteIp}", remoteIp);
                    context.Result = BuildUnauthorizedResult();
                }
            }
        }

        private static bool IsTokenValid(string expectedToken, string providedToken)
        {
            if (string.IsNullOrEmpty(expectedToken) || string.IsNullOrEmpty(providedToken))
            {
                return false;
            }

            var expectedBytes = Encoding.UTF8.GetBytes(expectedToken);
            var providedBytes = Encoding.UTF8.GetBytes(providedToken);

            if (expectedBytes.Length != providedBytes.Length)
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
        }

        private static bool IsIpAllowed(IPAddress remoteIp, IEnumerable<string> allowedCidrs)
        {
            if (remoteIp == null)
            {
                return false;
            }

            var ip = remoteIp.AddressFamily == AddressFamily.InterNetworkV6 ? remoteIp.MapToIPv4() : remoteIp;

            foreach (var cidr in allowedCidrs.Where(c => !string.IsNullOrWhiteSpace(c)))
            {
                if (IsInCidr(ip, cidr.Trim()))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsInCidr(IPAddress address, string cidr)
        {
            var parts = cidr.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || !int.TryParse(parts[1], out var prefixLength))
            {
                return false;
            }

            if (!IPAddress.TryParse(parts[0], out var network))
            {
                return false;
            }

            var addressBytes = address.MapToIPv4().GetAddressBytes();
            var networkBytes = network.MapToIPv4().GetAddressBytes();

            if (addressBytes.Length != networkBytes.Length)
            {
                return false;
            }

            var fullBytes = prefixLength / 8;
            var remainingBits = prefixLength % 8;

            for (int i = 0; i < fullBytes; i++)
            {
                if (addressBytes[i] != networkBytes[i])
                {
                    return false;
                }
            }

            if (remainingBits > 0)
            {
                var mask = (byte)~(0xFF >> remainingBits);
                if ((addressBytes[fullBytes] & mask) != (networkBytes[fullBytes] & mask))
                {
                    return false;
                }
            }

            return true;
        }

        private static JsonResult BuildUnauthorizedResult()
        {
            return new JsonResult(new { status = false, message = "固定Token鉴权失败" })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }
    }
}
