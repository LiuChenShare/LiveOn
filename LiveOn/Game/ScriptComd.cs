using System.ComponentModel;

namespace LiveOn.Game
{
    /// <summary>
    /// 统一命令码（仅保留区块级操作，实体级操作由子类自行定义）
    /// </summary>
    public enum ScriptComd
    {
        /// <summary>
        /// 种植
        /// </summary>
        [Description("种植")]
        ZhongZhi = 100001,
    }
}
