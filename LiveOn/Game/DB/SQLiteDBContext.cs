using System.Data;
using System.Data.SQLite;

namespace LiveOn.Game.DB
{
    public class SQLiteDBContext
    {
        public IDbConnection ConnBuilder;
        private string ConnectionString;

        public SQLiteDBContext()
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var path = Path.GetFullPath(Path.Combine(dir, "LiveOn.db"));
            ConnectionString = $"Data Source={path};";
        }

        public IDbConnection GetConn()
        {
            ConnBuilder = new SQLiteConnection(ConnectionString);
            ConnBuilder.Open();
            return ConnBuilder;
        }
    }
}
