using LiveOn.Game.Dungeons;
using LiveOn.Game.Entitys;
using LiveOn.Game.Items;
using log4net.Core;

namespace LiveOn.Core
{
    public class VariableUtility
    {
        //1.账号  2.userid  3.apiKey  4.上一次使用apiKey时间
        public static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Tuple<string, int, string, DateTime>> ActiveApiKeys = new System.Collections.Concurrent.ConcurrentDictionary<string, Tuple<string, int, string, DateTime>>();

        #region 操作

        /// <summary>
        /// 树-砍树
        /// </summary>
        public const string Script_Tree_KanShu = "Script_Tree_KanShu";
        /// <summary>
        /// 树-修剪
        /// </summary>
        public const string Script_Tree_XiuJian = "Script_Tree_XiuJian";

        /// <summary>
        /// 种植
        /// </summary>
        public const string Script_ZhongZhi = "Script_ZhongZhi";
        #endregion

        #region  实体
        public static Dictionary<string, Entity> EntityModel = new Dictionary<string, Entity>
            {
                { "0", new Entity() { Code = "0",Type = EntityType.Tree,Name = "杂树", Description="这是一颗不知名的树木。", Tree_High = 0.3, Tree_GrowthRate = 0.3} },
                { "1", new Entity() { Code = "1",Type = EntityType.Tree,Name = "杂树种子", Description="这是一颗平平无奇的种子，有可能种出一颗成材的大树。", SeedGrowthTime = 10, ToCode="0"} },
            };
        #endregion

        #region  物品
        public static Dictionary<string, Item> ItemModel = new Dictionary<string, Item>
            {
                { "1", new Item() { Code = "1", Name = "树枝" } },
                { "2", new Item() { Code = "2", Name = "木材" } }
            };
        #endregion

        #region 地下城
        public static Dictionary<string, Dungeon> Dungeon = new Dictionary<string, Dungeon>
        {
            { "1", new Dungeon() { Code = "1", Name = "初始地下城",Level = 1, 
                    Storeys = new List<Storey>(){ 
                        new Storey() { Index =1, MonsterPool = new List<ProbabilityModel>(){ 
                            new ProbabilityModel(){ //TODO 这里需要定义一些怪物和概率
                                                    }
                            } 
                        }, 
                    }
                } 
            },
        };
        #endregion
    }

}
