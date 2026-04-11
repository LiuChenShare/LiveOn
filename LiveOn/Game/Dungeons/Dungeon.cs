namespace LiveOn.Game.Dungeons
{
    /// <summary>
    /// 地下城
    /// </summary>
    public class Dungeon
    {
        /// <summary>
        /// 地下城唯一标识
        /// </summary>
        public string Id {  get; set; }

        /// <summary>
        /// 地下城名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 地下城编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 地下城等级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 关闭时间
        /// </summary>
        public DateTime CloseTime { get; set; }

        /// <summary>
        /// 地下城楼层
        /// </summary>
        public List<Storey> Storeys { get; set; }

        /// <summary>
        /// 根据等级随机生成地下城
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool Init(int level)
        {
            //Random _random = new Random();
            //double randomValue = _random.NextDouble();
            return  true;
        }
    }
}
