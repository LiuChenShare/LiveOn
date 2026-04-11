using Dapper;
using LiveOn.Game.DB;

namespace LiveOn.Game
{
    public static class DBResponse
    {
        #region MainGame

        public static DBModels.DbMainGame? GetMainGame()
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                return conn.GetModelFromSql<DBModels.DbMainGame>("SELECT * FROM MainGame;").FirstOrDefault();
            }
            catch { return null; }
        }

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
            catch { return false; }
        }

        #endregion

        #region Block

        public static List<DBModels.DbBlock> GetAllBlocks()
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                return conn.GetModelFromSql<DBModels.DbBlock>("SELECT * FROM Block;");
            }
            catch { return new List<DBModels.DbBlock>(); }
        }

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
            catch { return false; }
        }

        #endregion

        #region Entity

        public static List<DBModels.DbEntity> GetAllEntitys()
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                return conn.GetModelFromSql<DBModels.DbEntity>("SELECT * FROM Entity;");
            }
            catch { return new List<DBModels.DbEntity>(); }
        }

        public static bool SaveEntitys(List<Entitys.Entity> entitys)
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                foreach (var entity in entitys)
                {
                    var exists = conn.GetModelFromSql<DBModels.DbEntity>("SELECT * FROM Entity WHERE Id=@Id;",
                        new { Id = entity.Id }).FirstOrDefault();

                    if (exists == null)
                    {
                        conn.Execute(@"INSERT INTO Entity(Id, Name, Code, Description, Type, Stage, LifeTime, TreeHigh, TreeGrowthRate, SeedGrowthTime, ToCode, IsDeleted)
                                      VALUES (@Id, @Name, @Code, @Description, @Type, @Stage, @LifeTime, @TreeHigh, @TreeGrowthRate, @SeedGrowthTime, @ToCode, @IsDeleted);",
                            new
                            {
                                Id = entity.Id, Name = entity.Name, Code = entity.Code,
                                Description = entity.Description, Type = (int)entity.Type,
                                Stage = entity.Stage, LifeTime = entity.LifeTime,
                                TreeHigh = entity.Tree_High, TreeGrowthRate = entity.Tree_GrowthRate,
                                SeedGrowthTime = entity.SeedGrowthTime, ToCode = entity.ToCode ?? "",
                                IsDeleted = entity.IsDeleted ? 1 : 0
                            });
                    }
                    else
                    {
                        conn.Execute(@"UPDATE Entity SET Name=@Name, Code=@Code, Description=@Description, Type=@Type, Stage=@Stage, LifeTime=@LifeTime,
                                      TreeHigh=@TreeHigh, TreeGrowthRate=@TreeGrowthRate, SeedGrowthTime=@SeedGrowthTime, ToCode=@ToCode, IsDeleted=@IsDeleted WHERE Id=@Id;",
                            new
                            {
                                Id = entity.Id, Name = entity.Name, Code = entity.Code,
                                Description = entity.Description, Type = (int)entity.Type,
                                Stage = entity.Stage, LifeTime = entity.LifeTime,
                                TreeHigh = entity.Tree_High, TreeGrowthRate = entity.Tree_GrowthRate,
                                SeedGrowthTime = entity.SeedGrowthTime, ToCode = entity.ToCode ?? "",
                                IsDeleted = entity.IsDeleted ? 1 : 0
                            });
                    }
                }
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region Item

        public static List<DBModels.DbItem> GetAllItems()
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                return conn.GetModelFromSql<DBModels.DbItem>("SELECT * FROM Item;");
            }
            catch { return new List<DBModels.DbItem>(); }
        }

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
                        conn.Execute(@"INSERT INTO Item(Id, Name, Code, CreateTime, IsDeleted) VALUES (@Id, @Name, @Code, @CreateTime, @IsDeleted);",
                            new
                            {
                                Id = item.Id, Name = item.Name, Code = item.Code,
                                CreateTime = item.CreateTime, IsDeleted = item.IsDeleted ? 1 : 0
                            });
                    }
                    else
                    {
                        conn.Execute(@"UPDATE Item SET Name=@Name, Code=@Code, CreateTime=@CreateTime, IsDeleted=@IsDeleted WHERE Id=@Id;",
                            new
                            {
                                Id = item.Id, Name = item.Name, Code = item.Code,
                                CreateTime = item.CreateTime, IsDeleted = item.IsDeleted ? 1 : 0
                            });
                    }
                }
                return true;
            }
            catch { return false; }
        }

        #endregion

        #region GameLog

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
            catch { }
        }

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
            catch { return new List<DBModels.DbGameLog>(); }
        }

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
            catch { return new List<DBModels.DbGameLog>(); }
        }

        public static int GetGameLogCount()
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            try
            {
                return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM GameLog;");
            }
            catch { return 0; }
        }

        #endregion
    }
}
