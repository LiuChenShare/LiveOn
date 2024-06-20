using LiveOn.Core;

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
        /// 怪物概率 
        /// </summary>
        //public List<Tuple<string, double,int>> MonsterPool {  get; set; }
        public List<ProbabilityModel> MonsterPool { get; set; }
    }

    public enum StoreyStatus
    {
        /// <summary>
        /// 未初始化
        /// </summary>
        Unknown =0,
        /// <summary>
        /// 已开启
        /// </summary>
        Opening = 1,
        /// <summary>
        /// 已清理
        /// </summary>
        Cleared = 2,
    }
}
