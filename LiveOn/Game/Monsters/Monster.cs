namespace LiveOn.Game.Monsters
{
    /// <summary>
    /// 怪物实体
    /// </summary>
    public partial class Monster
    {
        /// <summary>
        /// 怪物唯一标识
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// 怪物名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 怪物编码
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// 怪物描述
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// 怪物状态
        /// </summary>
        public int Status {  get; private set; }


        #region 等级相关

        /// <summary>
        /// 怪物等级
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// 当前经验值
        /// </summary>
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
