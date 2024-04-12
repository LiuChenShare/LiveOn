using LiveOn.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using LiveOn.Core;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;

namespace LiveOn.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var user = CurrentUser.GetLoadUser();

            return View(user);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpGet]
        [SkipMyGlobalActionFilter]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [SkipMyGlobalActionFilter]
        public IActionResult Login(string userName, string passWord)
        {
            string uName = userName;
            string uPass = passWord;                    //代码假定用户名=admin 密码=123456
            if (uName == "admin" && uPass == "123456")
            {
                int userId = 576;
                string Authorization = "11111111-1111-1111-1111-111111111111";
                var apiKey = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

                #region 简易示例
                ////1.最简单的Cookie用法(不安全的)：
                //CookieOptions options = new CookieOptions();
                //options.Expires = DateTime.Now.AddMinutes(10);//10分钟后过期
                //Response.Cookies.Append("cookie_UserName", uName, options);//设置用
                //Response.Cookies.Append("cookie_UserPass", uPass, options);//设置密码
                //string _userName = Request.Cookies["cookie_UserName"];//读取cookie用户名
                //string _userPass = Request.Cookies["cookie_UserPass"];//读取cookie密码
                //ViewData["_userName"] = _userName;
                //ViewData["_UserPass"] = _userPass;
                #endregion


                //VariableUtility.ActiveApiKeys.TryAdd(Authorization, new Tuple<string, int, string, DateTime>(userName, userId, Authorization, DateTime.Now));
                var data = new Tuple<string, int, string, DateTime>(userName, userId, Authorization, DateTime.Now);
                VariableUtility.ActiveApiKeys.AddOrUpdate(Authorization, data, new Func<string, Tuple<string, int, string, DateTime>, Tuple<string, int, string, DateTime>>((oldkey, oldvalue) =>
                    data
                ));
                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddMinutes(60);//60分钟后过期
                Response.Cookies.Append("Authorization", Authorization, options);//设置用
                Response.Headers.Append("Authorization", Authorization);


                _logger.LogInformation("{time}这是第{one}个日志,一共{two}个",DateTime.Now.ToString("F"), 1, 2);

                return RedirectToAction("Index", "Home");//不同Controller的跳转写法
                                                         //return Redirect("Privacy");//同一个Controller下的写法
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }
}