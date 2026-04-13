using log4net.Config;
using log4net.Repository;
using System.Diagnostics;

namespace LiveOn
{
    /// <summary>
    /// Log4Net 日志辅助类，提供不同等级的日志写入方法
    /// 日志文件按等级分目录：log4net/error/、log4net/warn/、log4net/info/、log4net/debug/
    /// </summary>
    public class Log4NetHelper
    {
        /// <summary>
        /// Log4Net 日志仓储实例
        /// </summary>
        private static ILoggerRepository _repository;

        /// <summary>
        /// 读取配置文件，并使其生效
        /// </summary>
        /// <param name="repository">日志仓储</param>
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
        /// 写入 DEBUG 级别日志
        /// </summary>
        /// <param name="source">日志来源类型</param>
        /// <param name="msg">日志内容</param>
        public static void WriteDebug(Type source, string msg)
        {
            var log = log4net.LogManager.GetLogger(_repository.Name, source);
            var method = new StackTrace().GetFrame(1).GetMethod();
            log.Debug($"[{method.Name}] {msg}");
        }

        /// <summary>
        /// 写入 INFO 级别日志
        /// </summary>
        /// <param name="source">日志来源类型</param>
        /// <param name="msg">日志内容</param>
        public static void WriteInfo(Type source, string msg)
        {
            var log = log4net.LogManager.GetLogger(_repository.Name, source);
            var method = new StackTrace().GetFrame(1).GetMethod();
            log.Info($"[{method.Name}] {msg}");
        }

        /// <summary>
        /// 写入 WARN 级别日志
        /// </summary>
        /// <param name="source">日志来源类型</param>
        /// <param name="msg">日志内容</param>
        public static void WriteWarn(Type source, string msg)
        {
            var log = log4net.LogManager.GetLogger(_repository.Name, source);
            var method = new StackTrace().GetFrame(1).GetMethod();
            log.Warn($"[{method.Name}] {msg}");
        }

        /// <summary>
        /// 写入 ERROR 级别日志
        /// </summary>
        /// <param name="source">日志来源类型</param>
        /// <param name="msg">日志内容</param>
        public static void WriteError(Type source, string msg)
        {
            var log = log4net.LogManager.GetLogger(_repository.Name, source);
            var method = new StackTrace().GetFrame(1).GetMethod();
            log.Error($"[{method.Name}] {msg}");
        }

        /// <summary>
        /// 写入 FATAL 级别日志
        /// </summary>
        /// <param name="source">日志来源类型</param>
        /// <param name="msg">日志内容</param>
        public static void WriteFatal(Type source, string msg)
        {
            var log = log4net.LogManager.GetLogger(_repository.Name, source);
            var method = new StackTrace().GetFrame(1).GetMethod();
            log.Fatal($"[{method.Name}] {msg}");
        }
    }
}
