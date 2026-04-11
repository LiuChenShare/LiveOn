namespace LiveOn.Game
{
    /// <summary>
    /// 世界容器 — 持有游戏世界的所有数据
    /// </summary>
    public class Grain
    {
        #region 单例
        private static volatile Grain instance;
        private static object syncRoot = new Object();
        public Grain() { }
        public static Grain Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Grain();
                    }
                }
                return instance;
            }
        }
        #endregion

        /// <summary>
        /// 主游戏控制器
        /// </summary>
        public MainGame MainGame { get; set; } = MainGame.Instance;

        /// <summary>
        /// 区块列表
        /// </summary>
        public List<Block> Blocks { get; set; } = new List<Block>();

        /// <summary>
        /// 物品列表
        /// </summary>
        public List<Items.Item> Items { get; set; } = new List<Items.Item>();

        /// <summary>
        /// 最大区块数
        /// </summary>
        public int MaxBlockCount { get; set; } = 0;
    }
}
