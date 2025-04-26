using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http.Features;

namespace HDPro.Core.Middleware
{
    public class HttpRequestMiddleware
    {
        public static Func<RequestDelegate, RequestDelegate> Context
        {
            get
            {
                return next => async context =>
                {
                    //context.Response.Headers.Add("Access-Control-Expose-Headers", "vol_exp");
                    context.Response.Headers["Access-Control-Expose-Headers"]="vol_exp";
                    await next(context);
                };

            }
        }
    }

}
