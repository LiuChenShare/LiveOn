namespace LiveOn.Game.Dungeons
{
    /// <summary>
    /// 地下城
    /// </summary>
    public class Dungeon
    {
        public string Id {  get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int Level { get; set; }

        public DateTime CreateTime { get; set; }
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
