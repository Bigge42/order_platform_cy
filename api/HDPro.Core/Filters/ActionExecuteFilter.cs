using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using HDPro.Core.Enums;
using HDPro.Core.Extensions;
using HDPro.Core.ObjectActionValidator;
using HDPro.Core.Services;
using HDPro.Core.Utilities;

namespace HDPro.Core.Filters
{
    public class ActionExecuteFilter : IActionFilter
    {

        public void OnActionExecuting(ActionExecutingContext context)
        {
            //验证方法参数
            context.ActionParamsValidator();
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}