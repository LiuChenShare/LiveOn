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

        /// <summary>
        /// Action 执行前拦截（跳过权限验证，直接放行）
        /// </summary>
        /// <param name="context">Action 执行上下文</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            
            base.OnActionExecuting(context);
        }
        /// <summary>
        /// Action 执行后拦截（跳过权限验证，直接放行）
        /// </summary>
        /// <param name="context">Action 已执行上下文</param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
           
            base.OnActionExecuted(context);
        }
    }
}
