using LiveOn.Core;
using LiveOn.Game.DB;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LiveOn
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ////�����첽�¼�����
            //EvTest eventTest = new EvTest();
            //eventTest.XXX();

            var builder = WebApplication.CreateBuilder(args);

            //������־
            builder.Host.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();

                //����Log4net(��ȡ�����ļ�)
                //���滻��������־
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

            #region �ӿ��ж�������
            // Add services to the container.
            //builder.Services.AddSingleton<IActionFilter>(new GlobalActionFilter()); // ��ʼ�� LoggerMonitor
            //builder.Services.AddSingleton<IActionFilter>(new GlobalActionFilter()); // ��ʼ�� LoggerError
            //builder.Services.AddScoped<GlobalActionFilter>(); // ע�� ActionFilter

            //builder.Services.AddControllers(options => {
            //    options.Filters.Add(new GlobalActionFilter());
            //});

            var serviceProvider = builder.Services.BuildServiceProvider();
            GlobalActionFilter._logger = serviceProvider.GetRequiredService<ILogger<GlobalActionFilter>>();
#endregion

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // 初始化数据库
            DBUpdateHelper.DbVersionCheck();

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
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // 默认首页重定向到 login.html
            app.MapGet("/", () => Results.Redirect("/login.html"));

            app.Run();
        }

    }
}