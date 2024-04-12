using LiveOn.Core;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LiveOn
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ////测试异步事件调用
            //EvTest eventTest = new EvTest();
            //eventTest.XXX();

            var builder = WebApplication.CreateBuilder(args);

            //添加日志
            builder.Host.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();

                //配置Log4net(读取配置文件)
                //会替换掉内置日志
                logging.AddLog4Net("log4net.config");
            });

            //var app = builder.Build();

            //app.MapGet("/", () => "Hello World!");

            //app.MapGet("/Test", async (ILogger<Program> logger, HttpResponse response) =>
            //{
            //    logger.LogInformation("Testing logging in Program.cs");
            //    await response.WriteAsync("Testing");
            //});

            builder.Host.ConfigureServices(services => { 
            
                services.AddMvc(option => {
                    option.Filters.Add(new GlobalActionFilter());
                });
            });

            #region 接口行动过滤器
            // Add services to the container.
            //builder.Services.AddSingleton<IActionFilter>(new GlobalActionFilter()); // 初始化 LoggerMonitor
            //builder.Services.AddSingleton<IActionFilter>(new GlobalActionFilter()); // 初始化 LoggerError
            //builder.Services.AddScoped<GlobalActionFilter>(); // 注册 ActionFilter

            //builder.Services.AddControllers(options => {
            //    options.Filters.Add(new GlobalActionFilter());
            //});

            var serviceProvider = builder.Services.BuildServiceProvider();
            GlobalActionFilter._logger = serviceProvider.GetRequiredService<ILogger<GlobalActionFilter>>();
#endregion

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                //pattern: "{controller=Home}/{action=Index}/{id?}");
                pattern: "{controller=Home}/{action=Login}/{id?}");

            app.Run();
        }

    }
}