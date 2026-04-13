namespace LiveOn.Game.DB
{
    /// <summary>
    /// SQLite 数据库脚本管理类，维护版本化的建表SQL脚本
    /// </summary>
    public static class SQLiteDBScript
    {
        /// <summary>
        /// 数据库脚本字典，键为版本号，值为对应的SQL脚本
        /// </summary>
        public static Dictionary<int, string> DBScript { get; set; } = new Dictionary<int, string>()
        {
            { 1, @"
                CREATE TABLE IF NOT EXISTS DBVersion (
                    Version integer NOT NULL,
                    CreateTime NVARCHAR NOT NULL
                );
                CREATE TABLE IF NOT EXISTS MainGame (
                    ""Id"" NVARCHAR NOT NULL,
                    ""GameState"" integer,
                    ""GameDate"" TIMESTAMP,
                    ""MaxBlockCount"" integer,
                    PRIMARY KEY (""Id"")
                );
                CREATE TABLE IF NOT EXISTS Block (
                    ""Id"" NVARCHAR NOT NULL,
                    ""Stata"" integer,
                    ""EntityId"" NVARCHAR,
                    PRIMARY KEY (""Id"")
                );
                CREATE TABLE IF NOT EXISTS Entity (
                    ""Id"" NVARCHAR NOT NULL,
                    ""Name"" NVARCHAR,
                    ""Code"" NVARCHAR,
                    ""Description"" NVARCHAR,
                    ""Stage"" integer,
                    ""LifeTime"" TIMESTAMP,
                    ""Properties"" TEXT,
                    ""IsDeleted"" integer,
                    PRIMARY KEY (""Id"")
                );
                CREATE TABLE IF NOT EXISTS Item (
                    ""Id"" NVARCHAR NOT NULL,
                    ""Name"" NVARCHAR,
                    ""Code"" NVARCHAR,
                    ""CreateTime"" TIMESTAMP,
                    ""ToEntityCode"" NVARCHAR,
                    ""IsDeleted"" integer,
                    PRIMARY KEY (""Id"")
                );
                CREATE TABLE IF NOT EXISTS GameLog (
                    ""Id"" NVARCHAR NOT NULL,
                    ""Type"" integer DEFAULT 0,
                    ""Source"" NVARCHAR,
                    ""Content"" NVARCHAR,
                    ""GameDate"" TIMESTAMP,
                    ""CreateTime"" TIMESTAMP,
                    PRIMARY KEY (""Id"")
                );
                CREATE INDEX IF NOT EXISTS idx_gamelog_createtime ON GameLog(CreateTime);
            "},
        };
    }
}
