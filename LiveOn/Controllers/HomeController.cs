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
            string uPass = passWord;                    //����ٶ��û���=admin ����=123456
            if (uName == "admin" && uPass == "123456")
            {
                int userId = 576;
                string Authorization = "11111111-1111-1111-1111-111111111111";
                var apiKey = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

                #region ����ʾ��
                ////1.��򵥵�Cookie�÷�(����ȫ��)��
                //CookieOptions options = new CookieOptions();
                //options.Expires = DateTime.Now.AddMinutes(10);//10���Ӻ����
                //Response.Cookies.Append("cookie_UserName", uName, options);//������
                //Response.Cookies.Append("cookie_UserPass", uPass, options);//��������
                //string _userName = Request.Cookies["cookie_UserName"];//��ȡcookie�û���
                //string _userPass = Request.Cookies["cookie_UserPass"];//��ȡcookie����
                //ViewData["_userName"] = _userName;
                //ViewData["_UserPass"] = _userPass;
                #endregion


                //VariableUtility.ActiveApiKeys.TryAdd(Authorization, new Tuple<string, int, string, DateTime>(userName, userId, Authorization, DateTime.Now));
                var data = new Tuple<string, int, string, DateTime>(userName, userId, Authorization, DateTime.Now);
                VariableUtility.ActiveApiKeys.AddOrUpdate(Authorization, data, new Func<string, Tuple<string, int, string, DateTime>, Tuple<string, int, string, DateTime>>((oldkey, oldvalue) =>
                    data
                ));
                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddMinutes(60);//60���Ӻ����
                Response.Cookies.Append("Authorization", Authorization, options);//������
                Response.Headers.Append("Authorization", Authorization);


                _logger.LogInformation("{time}���ǵ�{one}����־,һ��{two}��",DateTime.Now.ToString("F"), 1, 2);

                return RedirectToAction("Index", "Home");//��ͬController����תд��
                                                         //return Redirect("Privacy");//ͬһ��Controller�µ�д��
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }
}