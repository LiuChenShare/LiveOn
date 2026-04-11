using LiveOn.Game.DB;
using LiveOn.Game.DTO;
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
        /// <summary>
        /// 构造函数
        /// </summary>
        public MainGame() { }
        /// <summary>
        /// 主游戏单例实例
        /// </summary>
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
        /// <summary>
        /// 消息事件，当游戏内产生消息时触发
        /// </summary>
        public event MsgHandler MsgEvent;

        /// <summary>
        /// 通用委托，传递对象数组参数
        /// </summary>
        public delegate void AllHandler(object[] objects);
        /// <summary>
        /// 异步通用委托，传递对象数组参数
        /// </summary>
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
                var tree = Entity.Create("0");
                if (tree != null)
                {
                    tree.Init("0");
                    Grain.Instance.Blocks[i].Entity = tree;
                }
            }

            // 预置1颗种子
            var seed = Entity.Create("1");
            if (seed != null)
            {
                seed.Init("1");
                Grain.Instance.Blocks[4].Entity = seed;
            }

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

        /// <summary>
        /// 开始游戏，初始化游戏世界并启动游戏时钟
        /// </summary>
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

        /// <summary>
        /// 暂停游戏，停止游戏时钟
        /// </summary>
        public void PauseGame()
        {
            GameTimer.Enabled = false;
            GameState = GameStateType.Paused;
            SaveGame(force: true);
            AddLog("系统", "游戏已暂停");
        }

        /// <summary>
        /// 继续游戏，恢复游戏时钟
        /// </summary>
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

        /// <summary>
        /// 获取所有物品列表
        /// </summary>
        public List<Item> GetItems() { return Grain.Instance.Items; }

        /// <summary>
        /// 添加物品到背包
        /// </summary>
        public bool AddItems(List<Item> items)
        {
            Grain.Instance.Items.AddRange(items);
            return true;
        }

        /// <summary>
        /// 按物品编码批量移除物品
        /// </summary>
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

        /// <summary>
        /// 获取游戏状态摘要信息
        /// </summary>
        public GameStateVO GetGameStateSummary()
        {
            var grain = Grain.Instance;
            return new GameStateVO
            {
                GameState = GameState.ToString(),
                GameDate = GameDate.ToString("yyyy-MM-dd HH:mm:ss"),
                BlockCount = grain.Blocks.Count,
                ItemCount = grain.Items.Count(x => !x.IsDeleted),
                MaxBlockCount = grain.MaxBlockCount
            };
        }

        /// <summary>
        /// 获取所有区块的概览信息
        /// </summary>
        public List<BlockOverviewVO> GetBlocksOverview()
        {
            return Grain.Instance.Blocks.Select(b =>
            {
                var vo = new BlockOverviewVO
                {
                    Id = b.Id,
                    Stata = b.Stata,
                    HasEntity = b.Entity != null && !b.Entity.IsDeleted,
                    EntityName = b.Entity?.Name,
                    EntityType = b.Entity?.GetType().Name,
                };

                // 树木显示高度
                if (b.Entity is TreeEntity tree)
                {
                    vo.Properties["tree_high"] = tree.TreeHigh;
                }

                return vo;
            }).ToList();
        }

        /// <summary>
        /// 获取指定区块的详细信息
        /// </summary>
        public BlockDetailVO GetBlockDetail(int blockId)
        {
            var block = Grain.Instance.Blocks.FirstOrDefault(b => b.Id == blockId);
            if (block == null) return null;

            EntityDetailVO entityVO = null;
            if (block.Entity != null && !block.Entity.IsDeleted)
            {
                entityVO = new EntityDetailVO
                {
                    Id = block.Entity.Id,
                    Name = block.Entity.Name,
                    Code = block.Entity.Code,
                    Description = block.Entity.Description,
                    Type = block.Entity.GetType().Name,
                    Stage = block.Entity.Stage,
                    LifeTime = block.Entity.LifeTime.ToString("HH:mm:ss"),
                    Properties = new Dictionary<string, object>()
                };

                // 树木特有属性
                if (block.Entity is TreeEntity tree)
                {
                    entityVO.Properties["tree_high"] = tree.TreeHigh;
                    entityVO.Properties["growth_rate"] = tree.GrowthRate;
                }
                // 种子特有属性
                else if (block.Entity is SeedEntity seed)
                {
                    entityVO.Properties["growth_time"] = seed.GrowthTime;
                    entityVO.Properties["to_code"] = seed.ToCode;
                }
            }

            return new BlockDetailVO
            {
                Id = block.Id,
                Stata = block.Stata,
                Entity = entityVO,
                Scripts = block.GetScript().Select(s => new ScriptVO { Name = s.Name, Description = s.Description, ScriptCode = s.ScriptCode }).ToList()
            };
        }

        /// <summary>
        /// 获取物品汇总信息，按编码分组统计数量
        /// </summary>
        public List<ItemSummaryVO> GetItemSummary()
        {
            return Grain.Instance.Items.Where(x => !x.IsDeleted)
                .GroupBy(x => new { x.Code, x.Name })
                .Select(g => new ItemSummaryVO { Code = g.Key.Code, Name = g.Key.Name, Count = g.Count() })
                .ToList();
        }

        #endregion


        #region 日志

        /// <summary>
        /// 获取最近的日志记录
        /// </summary>
        public List<GameLogVO> GetLogs(int count)
        {
            var logs = DBResponse.GetGameLogs(count);
            return logs.Select(l => new GameLogVO
            {
                Id = l.Id, Type = l.Type, Source = l.Source, Content = l.Content,
                GameDate = l.GameDate.ToString("yyyy-MM-dd HH:mm:ss"),
                CreateTime = l.CreateTime.ToString("MM-dd HH:mm:ss")
            }).ToList();
        }

        /// <summary>
        /// 分页获取日志记录
        /// </summary>
        public (List<GameLogVO> logs, int totalCount, int totalPages) GetLogsPaged(int page, int pageSize)
        {
            var totalCount = DBResponse.GetGameLogCount();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var logs = DBResponse.GetGameLogsPaged(page, pageSize);
            var result = logs.Select(l => new GameLogVO
            {
                Id = l.Id, Type = l.Type, Source = l.Source, Content = l.Content,
                GameDate = l.GameDate.ToString("yyyy-MM-dd HH:mm:ss"),
                CreateTime = l.CreateTime.ToString("MM-dd HH:mm:ss")
            }).ToList();
            return (result, totalCount, totalPages);
        }

        /// <summary>
        /// 写入一条游戏日志
        /// </summary>
        private void AddLog(string source, string content, int type = 0)
        {
            DBResponse.AddGameLog(type, source, content, GameDate);
            MsgEvent?.Invoke(type, source, content);
        }

        #endregion


        #region 区块操作

        /// <summary>
        /// 对指定区块执行交互操作
        /// </summary>
        /// <param name="blockId">区块ID</param>
        /// <param name="interactionId">交互标识（如 "plant", "chop", "prune"）</param>
        /// <returns>操作是否成功及消息</returns>
        public (bool success, string message) ExecuteBlockScript(int blockId, string interactionId)
        {
            var block = Grain.Instance.Blocks.FirstOrDefault(b => b.Id == blockId);
            if (block == null) return (false, "区块不存在");

            // 种植（区块操作，不走实体交互）
            if (interactionId == "plant")
            {
                var seedItem = Grain.Instance.Items.FirstOrDefault(x => x.Code == "1" && !x.IsDeleted);
                if (seedItem == null)
                {
                    AddLog("种植", "种植失败，没有种子");
                    return (false, "没有种子可以种植");
                }

                var entity = Entity.Create("1");
                if (entity == null)
                {
                    AddLog("种植", "种植失败，实体创建失败");
                    return (false, "无法创建实体");
                }

                entity.Init("1");
                block.Entity = entity;
                seedItem.Deleted();
                SaveGame();
                AddLog("种植", "成功种植了一颗杂树种子", 1);
                return (true, "成功种植了一颗杂树种子");
            }

            // 实体交互 — 直接多态调用
            if (block.Entity != null && !block.Entity.IsDeleted)
            {
                var (success, drops, destroy) = block.Entity.ExecuteInteraction(interactionId);
                if (!success)
                    return (false, "操作失败");

                if (drops != null && drops.Count > 0)
                    Grain.Instance.Items.AddRange(drops);

                if (destroy)
                    block.Entity = null;

                SaveGame();

                // 获取交互名称用于日志
                var interactionName = block.Entity?.GetInteractions()
                    .FirstOrDefault(i => i.ScriptCode == interactionId)?.Name ?? interactionId;
                AddLog("操作", $"执行了 {interactionName}", 1);
                return (true, "操作成功");
            }

            return (false, "该区块无法执行此操作");
        }

        /// <summary>
        /// 根据实体查找所在区块
        /// </summary>
        public Block FindBlockByEntity(Entity entity)
        {
            return Grain.Instance.Blocks.FirstOrDefault(b => b.Entity == entity);
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

        /// <summary>
        /// 保存游戏状态到数据库，默认每5秒最多保存一次
        /// </summary>
        /// <param name="force">是否强制立即保存，忽略时间间隔限制</param>
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

        /// <summary>
        /// 从数据库加载游戏存档
        /// </summary>
        /// <returns>是否加载成功</returns>
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
                    var entity = Entity.Create(dbEntity.Code);
                    if (entity == null) continue;

                    entity.Id = dbEntity.Id;
                    entity.Code = dbEntity.Code;
                    entity.Description = dbEntity.Description;
                    entity.Stage = dbEntity.Stage;
                    entity.LifeTime = dbEntity.LifeTime;
                    entity.DeserializeProperties(dbEntity.Properties ?? "{}");

                    // Name 可能被子类 InitProperties 覆盖，从数据库直接恢复
                    entity.Name = dbEntity.Name;

                    // 注册秒事件
                    SecondsEvent += entity.OnTick;
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
