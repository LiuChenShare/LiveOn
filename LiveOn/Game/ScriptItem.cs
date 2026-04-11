namespace LiveOn.Game
{
    /// <summary>
    /// 脚本操作项 — 描述一个可执行的脚本操作及其子操作
    /// </summary>
    public class ScriptItem
    {
        /// <summary>
        /// 操作名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 操作描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 交互标识码（如 "chop", "prune", "plant"）
        /// </summary>
        public string ScriptCode { get; set; }

        /// <summary>
        /// 子操作列表
        /// </summary>
        public List<ScriptItem> Items { get; set;}
    }
}
