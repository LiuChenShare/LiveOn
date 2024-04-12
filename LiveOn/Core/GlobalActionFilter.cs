using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using LiveOn.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace LiveOn.Core
{
    public class GlobalActionFilter : ActionFilterAttribute
    {
        private readonly Stopwatch Timmer = new Stopwatch();

        public static ILogger _logger;



        public override void OnActionExecuting(ActionExecutingContext context)
        {


            log4net.ILog log = log4net.LogManager.GetLogger(typeof(GlobalActionFilter));

            Timmer.Restart();


            //跳过权限验证
            var isDefined = false;
            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                var attr = controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit: true);
                isDefined = attr.Any(a => a.GetType().Equals(typeof(SkipMyGlobalActionFilter)));
                if (isDefined)
                {
                    var attribute = attr.FirstOrDefault(a => a.GetType().Equals(typeof(SkipMyGlobalActionFilter))) as SkipMyGlobalActionFilter;
                }
            }

            if (!isDefined)
            {
                _logger.LogInformation("wahahahhahha");
                //Timmer.Start();
                var apiKey2 = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                var apiKey = context.HttpContext.Request.Cookies["Authorization"];//读取cookie用户名
                if (!string.IsNullOrEmpty(apiKey) && apiKey != "[object Object]")
                {
                    //在这边判断时间和当前时间比较是否超过一个小时，没超过更新时间 ，超过去掉该token并返回401
                    if (VariableUtility.ActiveApiKeys.ContainsKey(apiKey))
                    {
                        var val = VariableUtility.ActiveApiKeys[apiKey];
                        if (DateTime.Now.Subtract(val.Item4).Minutes >= 60)
                        {
                            VariableUtility.ActiveApiKeys.TryRemove(apiKey, out val);
                        }
                        else
                        {
                            Tuple<string, int, string, DateTime> newValue = new Tuple<string, int, string, DateTime>(val.Item1, val.Item2, val.Item3, DateTime.Now);
                            VariableUtility.ActiveApiKeys.TryUpdate(apiKey, newValue, val);


                            var user = new CommonUser();

                            user.username = VariableUtility.ActiveApiKeys[apiKey].Item1;
                            var userIp = context.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                            if (string.IsNullOrEmpty(userIp))
                            {
                                userIp = context.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                            }
                            CurrentUser.SetLoadUserToken(apiKey);
                            //设置请求用户的IP
                            CurrentUser.SetLoadUserIP(userIp);
                            CurrentUser.SetLoadUserId(VariableUtility.ActiveApiKeys[apiKey].Item2);
                            CurrentUser.SetLoadUser(user);

                            //base.OnActionExecuting(context);
                        }
                    }
                }


                context.HttpContext.Response.StatusCode = 401;
                context.Result = new ObjectResult(new { status = 401, data = "权限异常" });

                // 指定重定向的页面
                context.Result = new RedirectToRouteResult(new RouteValueDictionary {
                    { "controller", "Home" },
                    { "action", "Login" }
                });
                //context.RedirectToAction("Index", "Home");

                return;
            }
            base.OnActionExecuting(context);

        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            Timmer.Stop();
            var useTime = Timmer.ElapsedMilliseconds;
            
                var type = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
            //_logger.WriteError(type, context.ActionDescriptor.DisplayName + "用时：" + useTime + " ms");
            //_logger.WriteLine();



            base.OnActionExecuted(context);
        }
    }
}
