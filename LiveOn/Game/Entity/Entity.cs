using LiveOn.Core;

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
        /// 树高（或初始高度）
        /// </summary>
        public double Tree_High { get; set; }

        /// <summary>
        /// 树成长速率
        /// </summary>
        public double Tree_GrowthRate { get; set; }


        #endregion


        #region 种子成长时间

        /// <summary>
        /// 种子成长时间(分钟)
        /// </summary>
        public int SeedGrowthTime { get; set; }

        /// <summary>
        /// 种子成长后的实体编码
        /// </summary>
        public string ToCode { get; set; }

        #endregion

        public bool IsDeleted { get; private set; }


        public bool Init(string code)
        {
            var entityModel = VariableUtility.EntityModel.GetValueOrDefault(code);
            if (entityModel == null)
                return false;

            Name = entityModel.Name;
            Code = entityModel.Code;
            Type = entityModel.Type;
            Id = Guid.NewGuid().ToString();
            LifeTime = new DateTime();
            Description = entityModel.Description;

            #region 树木
            Tree_High = entityModel.Tree_High;
            Tree_GrowthRate = entityModel.Tree_GrowthRate;
            #endregion
            #region 种子
            SeedGrowthTime = entityModel.SeedGrowthTime;
            ToCode = entityModel.ToCode;
            #endregion

            //注册秒事件
            MainGame.Instance.SecondsEvent += SecondsEventExecute;

            return true;
        }
        public bool Deleted()
        {
            IsDeleted = true;
            MainGame.Instance.SecondsEvent -= SecondsEventExecute;
            return true;
        }

        /// <summary>
        /// 获取可执行的操作
        /// </summary>
        /// <returns></returns>
        public List<ScriptItem> GetScript()
        {
            switch (Type)
            {
                case EntityType.Tree:
                    return GetScript_Tree();
                case EntityType.Seed:
                    return GetScript_Seed();
                default:
                    return new List<ScriptItem>();
            }
        }

        /// <summary>
        /// 执行操作
        /// </summary>
        /// <param name="scriptCode"></param>
        /// <returns></returns>
        public bool ExecuteScript(string scriptCode)
        {
            switch (Type)
            {
                case EntityType.Tree:
                    return ExecuteScript_Tree(scriptCode);
                case EntityType.Seed:
                    return ExecuteScript_Seed(scriptCode);
                default:
                    return false;
            }
        }

        private async Task SecondsEventExecute(DateTime time)
        {
            switch (Type)
            {
                case EntityType.Tree:
                    SecondsEventExecute_Tree(time);
                    break;
                default:
                    break;
            }
        }
    }

    public enum EntityType
    {
        /// <summary>
        /// 树木
        /// </summary>
        Tree = 0,
        /// <summary>
        /// 种子
        /// </summary>
        Seed = 1,
    }
}
