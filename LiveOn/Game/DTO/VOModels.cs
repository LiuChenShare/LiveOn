namespace LiveOn.Game.DTO
{
    /// <summary>
    /// 游戏状态视图对象
    /// </summary>
    public class GameStateVO
    {
        /// <summary>游戏状态字符串（Init/Load/InGame/Paused）</summary>
        public string GameState { get; set; }
        /// <summary>游戏内时间</summary>
        public string GameDate { get; set; }
        /// <summary>当前区块数量</summary>
        public int BlockCount { get; set; }
        /// <summary>当前物品数量</summary>
        public int ItemCount { get; set; }
        /// <summary>最大区块数量</summary>
        public int MaxBlockCount { get; set; }
    }

    /// <summary>
    /// 区块概览视图对象
    /// </summary>
    public class BlockOverviewVO
    {
        /// <summary>区块ID</summary>
        public int Id { get; set; }
        /// <summary>区块状态</summary>
        public int Stata { get; set; }
        /// <summary>是否有存活实体</summary>
        public bool HasEntity { get; set; }
        /// <summary>实体名称</summary>
        public string EntityName { get; set; }
        /// <summary>实体类型字符串</summary>
        public string EntityType { get; set; }
        /// <summary>树高（仅树木类型）</summary>
        public double? TreeHigh { get; set; }
    }

    /// <summary>
    /// 区块详情视图对象
    /// </summary>
    public class BlockDetailVO
    {
        /// <summary>区块ID</summary>
        public int Id { get; set; }
        /// <summary>区块状态</summary>
        public int Stata { get; set; }
        /// <summary>实体详细信息</summary>
        public EntityDetailVO Entity { get; set; }
        /// <summary>可用脚本列表</summary>
        public List<ScriptVO> Scripts { get; set; }
    }

    /// <summary>
    /// 实体详情视图对象
    /// </summary>
    public class EntityDetailVO
    {
        /// <summary>实体ID</summary>
        public string Id { get; set; }
        /// <summary>实体名称</summary>
        public string Name { get; set; }
        /// <summary>实体编码</summary>
        public string Code { get; set; }
        /// <summary>实体描述</summary>
        public string Description { get; set; }
        /// <summary>实体类型字符串</summary>
        public string Type { get; set; }
        /// <summary>树高</summary>
        public double TreeHigh { get; set; }
        /// <summary>成长阶段</summary>
        public int Stage { get; set; }
        /// <summary>存活时间</summary>
        public string LifeTime { get; set; }
        /// <summary>种子成长所需游戏分钟数</summary>
        public int SeedGrowthTime { get; set; }
        /// <summary>成熟后转化目标编码</summary>
        public string ToCode { get; set; }
    }

    /// <summary>
    /// 脚本视图对象
    /// </summary>
    public class ScriptVO
    {
        /// <summary>脚本名称</summary>
        public string Name { get; set; }
        /// <summary>脚本描述</summary>
        public string Description { get; set; }
        /// <summary>脚本代码</summary>
        public int ScriptCode { get; set; }
    }

    /// <summary>
    /// 物品汇总视图对象
    /// </summary>
    public class ItemSummaryVO
    {
        /// <summary>物品编码</summary>
        public string Code { get; set; }
        /// <summary>物品名称</summary>
        public string Name { get; set; }
        /// <summary>物品数量</summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// 游戏日志视图对象
    /// </summary>
    public class GameLogVO
    {
        /// <summary>日志ID</summary>
        public string Id { get; set; }
        /// <summary>日志类型（0=info, 1=success, 2=error, 3=warning）</summary>
        public int Type { get; set; }
        /// <summary>日志来源</summary>
        public string Source { get; set; }
        /// <summary>日志内容</summary>
        public string Content { get; set; }
        /// <summary>记录时的游戏时间</summary>
        public string GameDate { get; set; }
        /// <summary>记录时间（真实时间）</summary>
        public string CreateTime { get; set; }
    }
}
