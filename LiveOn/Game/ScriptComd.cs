using System.ComponentModel;

namespace LiveOn.Game
{
    /// <summary>
    /// 统一命令码
    /// </summary>
    public enum ScriptComd
    {
        /// <summary>
        /// 种植
        /// </summary>
        [Description("种植")]
        ZhongZhi = 100001,

        /// <summary>
        /// 砍树
        /// </summary>
        [Description("砍树")]
        KanShu = 100002,

        /// <summary>
        /// 修剪
        /// </summary>
        [Description("修剪")]
        XiuJian = 100003,
    }
}
