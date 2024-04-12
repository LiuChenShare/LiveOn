namespace LiveOn.Game.Entity
{
    public partial class Entity
    {
        public string Id { get; private set; }
        public string Name { get; set; }
        public string Code { get; set; }

        public string Description { get; set; }

        public EntityType Type { get; set; }

        /// <summary>
        /// 阶段
        /// </summary>
        public int Stage { get; set; }

        /// <summary>
        /// 生命时长
        /// </summary>
        public DateTime LifeTime { get; set; }


        #region 树木
        /// <summary>
        /// 树高
        /// </summary>
        public float Tree_High { get; set; }

        /// <summary>
        /// 树成长速率
        /// </summary>
        public float Tree_GrowthRate { get; set; }


        #endregion



        public bool IsDeleted { get; set; }


        public void Init(string code)
        {
            Id = Guid.NewGuid().ToString();
            LifeTime = new DateTime(0);

            if (code == "0")
            {
                Type = EntityType.Tree;
                Name = "杂树";

            }

            //注册秒事件
            switch (Type)
            {
                case EntityType.Tree:
                    MainGame.Instance.SecondsEvent += SecondsEventExecute_X0;
                    break;

            }
        }

        public List<ScriptItem> GetScript()
        {
            switch (Type)
            {
                case EntityType.Tree:
                    return GetScript_X0();
                default:
                    return new List<ScriptItem>();
            }
        }

    }

    public enum EntityType
    {
        /// <summary>
        /// 树木
        /// </summary>
        Tree = 0,
    }
}
