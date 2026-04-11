using LiveOn.Core;
using System.ComponentModel;

namespace LiveOn.Game.Dungeons
{
    /// <summary>
    /// 地下城楼层
    /// </summary>
    public class Storey
    {
        /// <summary>
        /// 楼层号
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 楼层状态
        /// </summary>
        public StoreyStatus Status { get; set; }

        /// <summary>
        /// 怪物概率池
        /// </summary>
        //public List<Tuple<string, double,int>> MonsterPool {  get; set; }
        public List<ProbabilityModel> MonsterPool { get; set; }
    }

    /// <summary>
    /// 地下城楼层状态枚举
    /// </summary>
    public enum StoreyStatus
    {
        [Description("未初始化")]
        Unknown =0,
        [Description("已开启")]
        Opening = 1,
        [Description("已清理")]
        Cleared = 2,
    }
}
