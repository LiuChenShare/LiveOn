namespace LiveOn.Game.Monsters
{
    /// <summary>
    /// 怪物
    /// </summary>
    public partial class Monster
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Code { get; private set; }

        public string Description { get; private set; }

        public int Status {  get; private set; }


        #region 等级相关

        public int Level { get; private set; }

        public int Exp { get; private set; }
        /// <summary>
        /// 经验值（上限）
        /// </summary>
        public int ExpMax { get; private set; }
        #endregion


        #region 战斗相关
        /// <summary>
        /// 力量
        /// </summary>
        public int Power { get; private set; }
        /// <summary>
        /// 敏捷
        /// </summary>
        public int Agile { get; private set; }
        /// <summary>
        /// 生命值
        /// </summary>
        public int HP { get; private set; }
        /// <summary>
        /// 生命值（最大值）
        /// </summary>
        public int HPMax
        {
            get
            {
                var a = 1.55;
                return (int)(Power * a);
            }
            private set { HPMax = value; }
        }
        #endregion
    }
}
