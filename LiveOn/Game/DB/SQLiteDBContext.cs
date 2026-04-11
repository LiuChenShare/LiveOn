using System.Data;
using System.Data.SQLite;

namespace LiveOn.Game.DB
{
    /// <summary>
    /// SQLite 数据库上下文，负责数据库连接的创建和管理
    /// </summary>
    public class SQLiteDBContext
    {
        /// <summary>
        /// 数据库连接对象
        /// </summary>
        public IDbConnection ConnBuilder;

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private string ConnectionString;

        /// <summary>
        /// 构造函数，自动创建数据目录并初始化连接字符串
        /// </summary>
        public SQLiteDBContext()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var path = Path.GetFullPath(Path.Combine(dir, "LiveOn.db"));
            ConnectionString = $"Data Source={path};";
        }

        /// <summary>
        /// 获取并打开一个新的数据库连接
        /// </summary>
        /// <returns>已打开的数据库连接</returns>
        public IDbConnection GetConn()
        {
            ConnBuilder = new SQLiteConnection(ConnectionString);
            ConnBuilder.Open();
            return ConnBuilder;
        }
    }
}
