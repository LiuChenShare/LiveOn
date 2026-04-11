using LiveOn.Game.DB;
using LiveOn.Game.Entitys;
using LiveOn.Game.Items;
using System.ComponentModel;

namespace LiveOn.Game
{
    /// <summary>
    /// 主程序 — 事件总线 + 游戏时钟 + 游戏生命周期
    /// </summary>
    public class MainGame
    {
        #region 单例
        private static volatile MainGame instance;
        private static object syncRoot = new Object();
        public MainGame() { }
        public static MainGame Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MainGame();
                    }
                }
                return instance;
            }
        }
        #endregion


        #region 事件
        public delegate Task TimeHandler(DateTime time);
        /// <summary>秒事件</summary>
        public event TimeHandler SecondsEvent;
        /// <summary>分事件</summary>
        public event TimeHandler MinutesEvent;
        /// <summary>时事件</summary>
        public event TimeHandler HoursEvent;
        /// <summary>日事件</summary>
        public event TimeHandler DaysEvent;
        /// <summary>月事件</summary>
        public event TimeHandler MonthsEvent;

        /// <summary>
        /// 消息委托
        /// </summary>
        public delegate void MsgHandler(int type, string source, string content);
        public event MsgHandler MsgEvent;

        public delegate void AllHandler(object[] objects);
        public delegate void AllTaskHandler(object[] objects);
        #endregion


        /// <summary>
        /// 游戏状态
        /// </summary>
        [DBFieldType(typeof(Int32))]
        public GameStateType GameState { get; private set; } = GameStateType.Init;

        private readonly System.Timers.Timer GameTimer = new System.Timers.Timer(1000);
        private bool _timerRegistered = false;

        /// <summary>
        /// 游戏时间
        /// </summary>
        public DateTime GameDate { get; private set; } = new DateTime();


        #region 游戏控制

        /// <summary>
        /// 初始化游戏世界
        /// </summary>
        public void InitGame()
        {
            if (GameState != GameStateType.Init) return;

            // 尝试从数据库加载存档
            if (LoadGame())
            {
                // 恢复 InGame 状态时自动启动时钟
                if (GameState == GameStateType.InGame)
                {
                    EnsureTimerRegistered();
                    GameTimer.Enabled = true;
                }
                return;
            }

            GameDate = new DateTime(1, 1, 1, 6, 0, 0);

            // 创建12个区块
            for (int i = 0; i < 12; i++)
            {
                Grain.Instance.Blocks.Add(new Block { Id = i, Stata = 0, Entity = null });
            }
            Grain.Instance.MaxBlockCount = Grain.Instance.Blocks.Count;

            // 预置4棵杂树
            for (int i = 0; i < 4; i++)
            {
                var tree = new Entity();
                tree.Init("0");
                Grain.Instance.Blocks[i].Entity = tree;
            }

            // 预置1颗种子
            var seed = new Entity();
            seed.Init("1");
            Grain.Instance.Blocks[4].Entity = seed;

            // 初始物品
            var initItems = new List<Item>();
            for (int i = 0; i < 3; i++)
            {
                var item1 = new Item(); item1.Init("1"); initItems.Add(item1);
                var item2 = new Item(); item2.Init("2"); initItems.Add(item2);
            }
            Grain.Instance.Items.AddRange(initItems);

            GameState = GameStateType.Load;
            SaveGame();
            AddLog("系统", "新的世界已创建");
        }

        public void GameStart()
        {
            if (GameState == GameStateType.Init)
                InitGame();

            EnsureTimerRegistered();
            GameTimer.Enabled = true;
            GameTimer.Start();
            GameState = GameStateType.InGame;
            SaveGame(force: true);
            AddLog("系统", "游戏开始");
        }

        public void PauseGame()
        {
            GameTimer.Enabled = false;
            GameState = GameStateType.Paused;
            SaveGame(force: true);
            AddLog("系统", "游戏已暂停");
        }

        public void ProceedGame()
        {
            EnsureTimerRegistered();
            GameTimer.Enabled = true;
            GameState = GameStateType.InGame;
            SaveGame(force: true);
            AddLog("系统", "游戏继续");
        }

        private void EnsureTimerRegistered()
        {
            if (_timerRegistered) return;
            GameTimer.Elapsed += new System.Timers.ElapsedEventHandler(Execute);
            GameTimer.AutoReset = true;
            _timerRegistered = true;
        }

        #endregion


        #region 物品操作

        public List<Item> GetItems() { return Grain.Instance.Items; }

        public bool AddItems(List<Item> items)
        {
            Grain.Instance.Items.AddRange(items);
            return true;
        }

        public bool RemoveItems(Dictionary<string, int> code_numbers)
        {
            var removeItems = new List<Item>();
            foreach (var item in code_numbers)
            {
                var items = Grain.Instance.Items.Where(x => x.Code == item.Key && !x.IsDeleted).OrderBy(x => x.CreateTime).Take(item.Value).ToList();
                if (items.Count == item.Value)
                    return false;
                removeItems.AddRange(items);
            }
            foreach (var item in removeItems)
                item.Deleted();
            return true;
        }

        #endregion


        #region 数据查询

        public object GetGameStateSummary()
        {
            var grain = Grain.Instance;
            return new
            {
                GameState = GameState.ToString(),
                GameDate = GameDate.ToString("yyyy-MM-dd HH:mm:ss"),
                BlockCount = grain.Blocks.Count,
                ItemCount = grain.Items.Count(x => !x.IsDeleted),
                MaxBlockCount = grain.MaxBlockCount
            };
        }

        public List<object> GetBlocksOverview()
        {
            return Grain.Instance.Blocks.Select(b => new
            {
                b.Id,
                b.Stata,
                HasEntity = b.Entity != null && !b.Entity.IsDeleted,
                EntityName = b.Entity?.Name,
                EntityType = b.Entity?.Type.ToString(),
                TreeHigh = b.Entity?.Tree_High
            }).ToList<object>();
        }

        public object GetBlockDetail(int blockId)
        {
            var block = Grain.Instance.Blocks.FirstOrDefault(b => b.Id == blockId);
            if (block == null) return null;

            return new
            {
                block.Id,
                block.Stata,
                Entity = block.Entity == null || block.Entity.IsDeleted ? null : new
                {
                    block.Entity.Id,
                    block.Entity.Name,
                    block.Entity.Code,
                    block.Entity.Description,
                    block.Entity.Type,
                    block.Entity.Tree_High,
                    block.Entity.Stage,
                    LifeTime = block.Entity.LifeTime.ToString("HH:mm:ss"),
                    block.Entity.SeedGrowthTime,
                    block.Entity.ToCode
                },
                Scripts = block.GetScript().Select(s => new { s.Name, s.Description, s.ScriptCode })
            };
        }

        public List<object> GetItemSummary()
        {
            return Grain.Instance.Items.Where(x => !x.IsDeleted)
                .GroupBy(x => new { x.Code, x.Name })
                .Select(g => new { Code = g.Key.Code, Name = g.Key.Name, Count = g.Count() })
                .ToList<object>();
        }

        #endregion


        #region 日志

        public List<object> GetLogs(int count)
        {
            var logs = DBResponse.GetGameLogs(count);
            return logs.Select(l => new
            {
                l.Id, l.Type, l.Source, l.Content,
                GameDate = l.GameDate.ToString("yyyy-MM-dd HH:mm:ss"),
                CreateTime = l.CreateTime.ToString("MM-dd HH:mm:ss")
            }).ToList<object>();
        }

        public (List<object> logs, int totalCount, int totalPages) GetLogsPaged(int page, int pageSize)
        {
            var totalCount = DBResponse.GetGameLogCount();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var logs = DBResponse.GetGameLogsPaged(page, pageSize);
            var result = logs.Select(l => new
            {
                l.Id, l.Type, l.Source, l.Content,
                GameDate = l.GameDate.ToString("yyyy-MM-dd HH:mm:ss"),
                CreateTime = l.CreateTime.ToString("MM-dd HH:mm:ss")
            }).ToList<object>();
            return (result, totalCount, totalPages);
        }

        private void AddLog(string source, string content, int type = 0)
        {
            DBResponse.AddGameLog(type, source, content, GameDate);
            MsgEvent?.Invoke(type, source, content);
        }

        #endregion


        #region 区块操作

        public (bool success, string message) ExecuteBlockScript(int blockId, int scriptCode)
        {
            var block = Grain.Instance.Blocks.FirstOrDefault(b => b.Id == blockId);
            if (block == null) return (false, "区块不存在");

            if (scriptCode == (int)ScriptComd.ZhongZhi)
            {
                var seedItem = Grain.Instance.Items.FirstOrDefault(x => x.Code == "1" && !x.IsDeleted);
                if (seedItem == null)
                {
                    AddLog("种植", "种植失败，没有种子");
                    return (false, "没有种子可以种植");
                }

                var entity = new Entity();
                entity.Init("1");
                block.Entity = entity;
                seedItem.Deleted();
                SaveGame();
                AddLog("种植", "成功种植了一颗杂树种子", 1);
                return (true, "成功种植了一颗杂树种子");
            }

            if (block.Entity != null && !block.Entity.IsDeleted)
            {
                if (scriptCode == (int)ScriptComd.KanShu)
                {
                    block.Entity.ExecuteScript(scriptCode);
                    block.Entity = null;
                    SaveGame();
                    AddLog("伐木", "砍树成功，获得了木材和树枝", 1);
                    return (true, "砍树成功，获得了木材和树枝");
                }

                var result = block.Entity.ExecuteScript(scriptCode);
                if (result && scriptCode == (int)ScriptComd.XiuJian)
                {
                    SaveGame();
                    AddLog("修剪", "修剪成功，获得了树枝", 1);
                    return (true, "修剪成功，获得了树枝");
                }
                AddLog("操作", result ? "操作成功" : "操作失败", result ? 1 : 2);
                return (result, result ? "操作成功" : "操作失败");
            }

            return (false, "该区块无法执行此操作");
        }

        #endregion


        private void Execute(object source, System.Timers.ElapsedEventArgs e)
        {
            GameDate = GameDate.AddSeconds(1);
            SaveGame();

            if (SecondsEvent != null)
            {
                Task[] tasksSeconds = SecondsEvent.GetInvocationList().Cast<TimeHandler>()
                                           .Select(handler => Task.Run(() => handler(GameDate))).ToArray();
            }
            if (GameDate.Second == 0 && MinutesEvent != null)
            {
                Task[] tasksMinutes = MinutesEvent.GetInvocationList().Cast<TimeHandler>()
                                           .Select(handler => Task.Run(() => handler(GameDate))).ToArray();
                if (GameDate.Minute == 0 && HoursEvent != null)
                {
                    Task[] tasksHours = HoursEvent.GetInvocationList().Cast<TimeHandler>()
                                               .Select(handler => Task.Run(() => handler(GameDate))).ToArray();
                    if (GameDate.Hour == 0 && DaysEvent != null)
                    {
                        Task[] tasksDays = DaysEvent.GetInvocationList().Cast<TimeHandler>()
                                                   .Select(handler => Task.Run(() => handler(GameDate))).ToArray();
                        if (GameDate.Day == 1 && MonthsEvent != null)
                        {
                            Task[] tasksMonths = MonthsEvent.GetInvocationList().Cast<TimeHandler>()
                                                       .Select(handler => Task.Run(() => handler(GameDate))).ToArray();
                        }
                    }
                }
            }
        }


        #region 持久化

        private DateTime _lastSaveTime = DateTime.MinValue;

        public void SaveGame(bool force = false)
        {
            var now = DateTime.Now;
            if (!force && (now - _lastSaveTime).TotalSeconds < 5) return;
            _lastSaveTime = now;

            try
            {
                var grain = Grain.Instance;
                DBResponse.SaveMainGame((int)GameState, GameDate, grain.MaxBlockCount);
                DBResponse.SaveBlocks(grain.Blocks);
                DBResponse.SaveEntitys(grain.Blocks.Where(b => b.Entity != null).Select(b => b.Entity).ToList());
                DBResponse.SaveItems(grain.Items);
            }
            catch { }
        }

        public bool LoadGame()
        {
            try
            {
                var mainGameData = DBResponse.GetMainGame();
                if (mainGameData == null) return false;

                var grain = Grain.Instance;

                GameDate = mainGameData.GameDate;
                grain.MaxBlockCount = mainGameData.MaxBlockCount;

                // 加载所有实体
                var dbEntitys = DBResponse.GetAllEntitys();
                var entityMap = new Dictionary<string, Entity>();
                foreach (var dbEntity in dbEntitys)
                {
                    var entity = new Entity();
                    entity.Id = dbEntity.Id;
                    entity.Name = dbEntity.Name;
                    entity.Code = dbEntity.Code;
                    entity.Description = dbEntity.Description;
                    entity.Type = (EntityType)dbEntity.Type;
                    entity.Stage = dbEntity.Stage;
                    entity.LifeTime = dbEntity.LifeTime;
                    entity.Tree_High = dbEntity.TreeHigh;
                    entity.Tree_GrowthRate = dbEntity.TreeGrowthRate;
                    entity.SeedGrowthTime = dbEntity.SeedGrowthTime;
                    entity.ToCode = dbEntity.ToCode;

                    SecondsEvent += entity.SecondsEventExecute;
                    entityMap[entity.Id] = entity;
                }

                // 加载区块
                var dbBlocks = DBResponse.GetAllBlocks();
                grain.Blocks.Clear();
                foreach (var dbBlock in dbBlocks)
                {
                    grain.Blocks.Add(new Block
                    {
                        Id = int.Parse(dbBlock.Id),
                        Stata = dbBlock.Stata,
                        Entity = !string.IsNullOrEmpty(dbBlock.EntityId) && entityMap.ContainsKey(dbBlock.EntityId)
                            ? entityMap[dbBlock.EntityId] : null
                    });
                }

                // 加载物品
                var dbItems = DBResponse.GetAllItems();
                grain.Items.Clear();
                foreach (var dbItem in dbItems)
                {
                    if (!dbItem.IsDeleted)
                    {
                        var item = new Item();
                        item.Id = dbItem.Id;
                        item.Name = dbItem.Name;
                        item.Code = dbItem.Code;
                        item.CreateTime = dbItem.CreateTime;
                        grain.Items.Add(item);
                    }
                }

                GameState = (GameStateType)mainGameData.GameState;
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }



    /// <summary>
    /// 游戏状态
    /// </summary>
    public enum GameStateType
    {
        [Description("GameOver")]
        GameOver = -1,

        [Description("初始化")]
        Init = 0,

        [Description("加载中")]
        Load = 1,

        [Description("游戏中")]
        InGame = 2,

        [Description("暂停")]
        Paused = 3,
    }
}
