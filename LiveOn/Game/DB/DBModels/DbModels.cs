namespace LiveOn.Game.DBModels
{
    /// <summary>
    /// 游戏主存档
    /// </summary>
    public class DbMainGame
    {
        public string Id { get; set; }
        /// <summary>游戏状态（0=Init, 1=Load, 2=InGame, 3=Paused）</summary>
        public int GameState { get; set; }
        /// <summary>游戏内时间</summary>
        public DateTime GameDate { get; set; }
        /// <summary>最大区块数</summary>
        public int MaxBlockCount { get; set; }
    }

    /// <summary>
    /// 区块（地块）
    /// </summary>
    public class DbBlock
    {
        public string Id { get; set; }
        /// <summary>区块状态</summary>
        public int Stata { get; set; }
        /// <summary>关联实体ID</summary>
        public string EntityId { get; set; }
    }

    /// <summary>
    /// 实体（树、种子等）— 子类特有属性通过 Properties JSON 列存储
    /// </summary>
    public class DbEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        /// <summary>成长阶段</summary>
        public int Stage { get; set; }
        /// <summary>存活时间</summary>
        public DateTime LifeTime { get; set; }
        /// <summary>子类特有属性 JSON</summary>
        public string Properties { get; set; }
        /// <summary>是否已删除</summary>
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// 物品
    /// </summary>
    public class DbItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        /// <summary>创建时间</summary>
        public DateTime CreateTime { get; set; }
        /// <summary>种植后生成的实体编码</summary>
        public string ToEntityCode { get; set; }
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// 操作日志
    /// </summary>
    public class DbGameLog
    {
        public string Id { get; set; }
        /// <summary>类型（0=info, 1=success, 2=error, 3=warning）</summary>
        public int Type { get; set; }
        /// <summary>日志来源（系统、种植、伐木等）</summary>
        public string Source { get; set; }
        /// <summary>日志内容</summary>
        public string Content { get; set; }
        /// <summary>记录时的游戏时间</summary>
        public DateTime GameDate { get; set; }
        /// <summary>记录时间（真实时间）</summary>
        public DateTime CreateTime { get; set; }
    }
}
