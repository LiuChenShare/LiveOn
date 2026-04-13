using Dapper;
using LiveOn.Game.DB;
using log4net;

namespace LiveOn.Game
{
    /// <summary>
    /// 数据库响应工具类，提供所有实体的增删改查静态方法
    /// </summary>
    public static class DBResponse
    {
        /// <summary>
        /// Log4Net 日志实例
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(DBResponse));

        #region MainGame

        /// <summary>
        /// 获取主游戏存档数据
        /// </summary>
        /// <returns>主游戏数据模型，查询失败返回 null</returns>
        public static DBModels.DbMainGame? GetMainGame()
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                return conn.GetModelFromSql<DBModels.DbMainGame>("SELECT * FROM MainGame;").FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error("查询主游戏存档失败", ex);
                return null;
            }
        }

        /// <summary>
        /// 保存主游戏存档数据，存在则更新，不存在则插入
        /// </summary>
        public static bool SaveMainGame(int gameState, DateTime gameDate, int maxBlockCount)
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                var exists = conn.GetModelFromSql<DBModels.DbMainGame>("SELECT * FROM MainGame WHERE Id='1';").FirstOrDefault();
                if (exists == null)
                {
                    conn.Execute("INSERT INTO MainGame(Id, GameState, GameDate, MaxBlockCount) VALUES ('1', @GameState, @GameDate, @MaxBlockCount);",
                        new { GameState = gameState, GameDate = gameDate, MaxBlockCount = maxBlockCount });
                }
                else
                {
                    conn.Execute("UPDATE MainGame SET GameState=@GameState, GameDate=@GameDate, MaxBlockCount=@MaxBlockCount WHERE Id='1';",
                        new { GameState = gameState, GameDate = gameDate, MaxBlockCount = maxBlockCount });
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("保存主游戏存档失败", ex);
                return false;
            }
        }

        #endregion

        #region Block

        /// <summary>
        /// 获取所有地块数据
        /// </summary>
        public static List<DBModels.DbBlock> GetAllBlocks()
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                return conn.GetModelFromSql<DBModels.DbBlock>("SELECT * FROM Block;");
            }
            catch (Exception ex)
            {
                Log.Error("查询所有地块失败", ex);
                return new List<DBModels.DbBlock>();
            }
        }

        /// <summary>
        /// 批量保存地块数据
        /// </summary>
        public static bool SaveBlocks(List<Block> blocks)
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                foreach (var block in blocks)
                {
                    var exists = conn.GetModelFromSql<DBModels.DbBlock>("SELECT * FROM Block WHERE Id=@Id;",
                        new { Id = block.Id.ToString() }).FirstOrDefault();

                    if (exists == null)
                    {
                        conn.Execute(@"INSERT INTO Block(Id, Stata, EntityId) VALUES (@Id, @Stata, @EntityId);",
                            new { Id = block.Id.ToString(), Stata = block.Stata, EntityId = block.Entity?.Id });
                    }
                    else
                    {
                        conn.Execute(@"UPDATE Block SET Stata=@Stata, EntityId=@EntityId WHERE Id=@Id;",
                            new { Id = block.Id.ToString(), Stata = block.Stata, EntityId = block.Entity?.Id });
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("保存地块数据失败", ex);
                return false;
            }
        }

        #endregion

        #region Entity

        /// <summary>
        /// 获取所有实体数据
        /// </summary>
        public static List<DBModels.DbEntity> GetAllEntitys()
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                return conn.GetModelFromSql<DBModels.DbEntity>("SELECT * FROM Entity;");
            }
            catch (Exception ex)
            {
                Log.Error("查询所有实体失败", ex);
                return new List<DBModels.DbEntity>();
            }
        }

        /// <summary>
        /// 批量保存实体数据
        /// </summary>
        public static bool SaveEntitys(List<Entitys.Entity> entitys)
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                foreach (var entity in entitys)
                {
                    var propertiesJson = entity.SerializeProperties();
                    var exists = conn.GetModelFromSql<DBModels.DbEntity>("SELECT * FROM Entity WHERE Id=@Id;",
                        new { Id = entity.Id }).FirstOrDefault();

                    if (exists == null)
                    {
                        conn.Execute(@"INSERT INTO Entity(Id, Name, Code, Description, Stage, LifeTime, Properties, IsDeleted)
                                      VALUES (@Id, @Name, @Code, @Description, @Stage, @LifeTime, @Properties, @IsDeleted);",
                            new
                            {
                                Id = entity.Id, Name = entity.Name, Code = entity.Code,
                                Description = entity.Description,
                                Stage = entity.Stage, LifeTime = entity.LifeTime,
                                Properties = propertiesJson,
                                IsDeleted = entity.IsDeleted ? 1 : 0
                            });
                    }
                    else
                    {
                        conn.Execute(@"UPDATE Entity SET Name=@Name, Code=@Code, Description=@Description,
                                      Stage=@Stage, LifeTime=@LifeTime, Properties=@Properties, IsDeleted=@IsDeleted WHERE Id=@Id;",
                            new
                            {
                                Id = entity.Id, Name = entity.Name, Code = entity.Code,
                                Description = entity.Description,
                                Stage = entity.Stage, LifeTime = entity.LifeTime,
                                Properties = propertiesJson,
                                IsDeleted = entity.IsDeleted ? 1 : 0
                            });
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("保存实体数据失败", ex);
                return false;
            }
        }

        #endregion

        #region Item

        /// <summary>
        /// 获取所有物品数据
        /// </summary>
        public static List<DBModels.DbItem> GetAllItems()
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                return conn.GetModelFromSql<DBModels.DbItem>("SELECT * FROM Item;");
            }
            catch (Exception ex)
            {
                Log.Error("查询所有物品失败", ex);
                return new List<DBModels.DbItem>();
            }
        }

        /// <summary>
        /// 批量保存物品数据
        /// </summary>
        public static bool SaveItems(List<Items.Item> items)
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                foreach (var item in items)
                {
                    var exists = conn.GetModelFromSql<DBModels.DbItem>("SELECT * FROM Item WHERE Id=@Id;",
                        new { Id = item.Id }).FirstOrDefault();

                    if (exists == null)
                    {
                        conn.Execute(@"INSERT INTO Item(Id, Name, Code, CreateTime, ToEntityCode, IsDeleted) VALUES (@Id, @Name, @Code, @CreateTime, @ToEntityCode, @IsDeleted);",
                            new
                            {
                                Id = item.Id, Name = item.Name, Code = item.Code,
                                CreateTime = item.CreateTime, ToEntityCode = item.ToEntityCode,
                                IsDeleted = item.IsDeleted ? 1 : 0
                            });
                    }
                    else
                    {
                        conn.Execute(@"UPDATE Item SET Name=@Name, Code=@Code, CreateTime=@CreateTime, ToEntityCode=@ToEntityCode, IsDeleted=@IsDeleted WHERE Id=@Id;",
                            new
                            {
                                Id = item.Id, Name = item.Name, Code = item.Code,
                                CreateTime = item.CreateTime, ToEntityCode = item.ToEntityCode,
                                IsDeleted = item.IsDeleted ? 1 : 0
                            });
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("保存物品数据失败", ex);
                return false;
            }
        }

        #endregion

        #region GameLog

        /// <summary>
        /// 添加一条游戏日志
        /// </summary>
        public static void AddGameLog(int type, string source, string content, DateTime gameDate)
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                conn.Execute(@"INSERT INTO GameLog(Id, Type, Source, Content, GameDate, CreateTime)
                              VALUES (@Id, @Type, @Source, @Content, @GameDate, @CreateTime);",
                    new
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = type,
                        Source = source,
                        Content = content,
                        GameDate = gameDate,
                        CreateTime = DateTime.Now
                    });
            }
            catch (Exception ex)
            {
                Log.Error("写入游戏日志失败", ex);
            }
        }

        /// <summary>
        /// 获取最近指定数量的游戏日志
        /// </summary>
        public static List<DBModels.DbGameLog> GetGameLogs(int count)
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                return conn.GetModelFromSql<DBModels.DbGameLog>(
                    "SELECT * FROM GameLog ORDER BY CreateTime DESC LIMIT @Count;",
                    new { Count = count });
            }
            catch (Exception ex)
            {
                Log.Error("查询游戏日志失败", ex);
                return new List<DBModels.DbGameLog>();
            }
        }

        /// <summary>
        /// 分页获取游戏日志
        /// </summary>
        public static List<DBModels.DbGameLog> GetGameLogsPaged(int page, int pageSize)
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                var offset = (page - 1) * pageSize;
                return conn.GetModelFromSql<DBModels.DbGameLog>(
                    "SELECT * FROM GameLog ORDER BY CreateTime DESC LIMIT @PageSize OFFSET @Offset;",
                    new { PageSize = pageSize, Offset = offset });
            }
            catch (Exception ex)
            {
                Log.Error("分页查询游戏日志失败", ex);
                return new List<DBModels.DbGameLog>();
            }
        }

        /// <summary>
        /// 获取游戏日志总数
        /// </summary>
        public static int GetGameLogCount()
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM GameLog;");
            }
            catch (Exception ex)
            {
                Log.Error("查询游戏日志总数失败", ex);
                return 0;
            }
        }

        #endregion
    }
}
