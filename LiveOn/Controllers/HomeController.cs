using LiveOn.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LiveOn.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 登录 API
        /// </summary>
        [HttpPost]
        [SkipMyGlobalActionFilter]
        public IActionResult Login(string userName, string passWord)
        {
            if (userName == "admin" && passWord == "123456")
            {
                int userId = 576;
                string authorization = "11111111-1111-1111-1111-111111111111";

                var data = new Tuple<string, int, string, DateTime>(userName, userId, authorization, DateTime.Now);
                VariableUtility.ActiveApiKeys.AddOrUpdate(authorization, data, new Func<string, Tuple<string, int, string, DateTime>, Tuple<string, int, string, DateTime>>((oldkey, oldvalue) =>
                    data
                ));

                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddMinutes(60);
                Response.Cookies.Append("Authorization", authorization, options);
                Response.Headers.Append("Authorization", authorization);

                return Json(new { success = true });
            }

            Response.StatusCode = 401;
            return Json(new { success = false, message = "账号或密码错误" });
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        [HttpGet]
        public IActionResult Logout()
        {
            var apiKey = HttpContext.Request.Cookies["Authorization"];
            if (!string.IsNullOrEmpty(apiKey))
            {
                VariableUtility.ActiveApiKeys.TryRemove(apiKey, out _);
                Response.Cookies.Delete("Authorization");
            }
            return Redirect("/login.html");
        }
    }
}
