using log4net.Config;
using log4net.Repository;
using System.Diagnostics;

namespace LiveOn
{
    public class Log4NetHelper
    {
        private static ILoggerRepository _repository;

        /// <summary>
        /// 读取配置文件，并使其生效。如果未找到配置文件，则抛出异常
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="configFilePath">配置文件全路径</param>
        public static void SetConfig(ILoggerRepository repository, string configFilePath)
        {
            _repository = repository;
            var fileInfo = new FileInfo(configFilePath);
            if (!fileInfo.Exists)
            {
                throw new Exception("未找到配置文件" + configFilePath);
            }
            XmlConfigurator.ConfigureAndWatch(_repository, fileInfo);
        }

        /// <summary>
        /// 记录消息日志
        /// </summary>
        /// <param name="t"></param>
        /// <param name="msg"></param>
        public static void WriteInfo(Type t, string msg)
        {
            var log = log4net.LogManager.GetLogger(_repository.Name, "Info");
            var stackTrace = new StackTrace();
            var stackFrame = stackTrace.GetFrame(1);
            var methodBase = stackFrame.GetMethod();
            var message = "方法名称：" + methodBase.Name + "\r\n日志内容：" + msg;
            log.Info(message);
        }
    }
}
