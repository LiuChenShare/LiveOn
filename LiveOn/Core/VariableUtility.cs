using LiveOn.Game.Dungeons;
using LiveOn.Game.Items;
using log4net.Core;

namespace LiveOn.Core
{
    /// <summary>
    /// 全局变量工具类，存储活跃的 API 密钥、物品模型和地下城数据
    /// </summary>
    public class VariableUtility
    {
        /// <summary>
        /// 活跃的 API 密钥字典，Key 为 apiKey，Value 为元组（账号、用户ID、apiKey、上次使用时间）
        /// </summary>
        public static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Tuple<string, int, string, DateTime>> ActiveApiKeys = new System.Collections.Concurrent.ConcurrentDictionary<string, Tuple<string, int, string, DateTime>>();

        #region  物品
        /// <summary>
        /// 物品模型字典，Key 为编码，Value 为物品定义
        /// </summary>
        public static Dictionary<string, Item> ItemModel = new Dictionary<string, Item>
            {
                { "1", new Item() { Code = "1", Name = "树枝" } },
                { "2", new Item() { Code = "2", Name = "木材" } }
            };
        #endregion

        #region 地下城
        /// <summary>
        /// 地下城字典，Key 为编码，Value 为地下城定义
        /// </summary>
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
