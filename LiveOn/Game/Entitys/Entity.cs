using LiveOn.Core;
using System.ComponentModel;

namespace LiveOn.Game.Entitys
{
    /// <summary>
    /// 实体基类，表示游戏中的各种可交互对象（树木、种子等）
    /// </summary>
    public partial class Entity
    {
        /// <summary>
        /// 实体唯一标识
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// 实体名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 实体编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 实体描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 实体类型
        /// </summary>
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

        /// <summary>
        /// 是否已删除
        /// </summary>
        public bool IsDeleted { get; private set; }


        /// <summary>
        /// 根据编码初始化实体，加载模板数据并注册秒事件
        /// </summary>
        /// <param name="code">实体编码</param>
        /// <returns>初始化成功返回 true，编码不存在返回 false</returns>
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
        /// <summary>
        /// 删除实体，标记为已删除并注销秒事件
        /// </summary>
        /// <returns>删除成功返回 true</returns>
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
        public bool ExecuteScript(int scriptCode)
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

        /// <summary>
        /// 秒事件执行入口，根据实体类型分发到对应的秒事件处理方法
        /// </summary>
        /// <param name="time">当前时间</param>
        internal async Task SecondsEventExecute(DateTime time)
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

    /// <summary>
    /// 实体类型枚举
    /// </summary>
    public enum EntityType
    {
        [Description("树木")]
        Tree = 0,
        [Description("种子")]
        Seed = 1,
    }
}
