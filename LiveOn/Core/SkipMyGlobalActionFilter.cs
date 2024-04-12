using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using LiveOn.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace LiveOn.Core
{
    /// <summary>
    /// 无权限验证
    /// </summary>
    public class SkipMyGlobalActionFilter : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            
            base.OnActionExecuting(context);
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
           
            base.OnActionExecuted(context);
        }
    }
}
