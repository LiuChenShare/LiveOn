using LiveOn.Game.Entitys;

namespace LiveOn.Game
{
    /// <summary>
    /// 区块 — 游戏世界中的一个地块单元，可包含一个实体
    /// </summary>
    public class Block
    {
        /// <summary>
        /// 区块id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 状态    空闲、、、、、
        /// </summary>
        public int Stata { get; set; }

        /// <summary>
        /// 区块上的实体（如树木、种子等），为 null 表示区块空闲
        /// </summary>
        public Entity Entity { get; set; }


        /// <summary>
        /// 获取可执行的操作
        /// </summary>
        public List<ScriptItem> GetScript()
        {
            var result = new List<ScriptItem>();

            if (Entity == null || Entity.IsDeleted)
            {
                // 种植是区块操作，不是实体操作
                // 遍历背包，找出所有有 ToEntityCode 且对应实体可种植的物品
                var plantableItems = Grain.Instance.Items
                    .Where(x => !x.IsDeleted && !string.IsNullOrEmpty(x.ToEntityCode)
                                 && Core.VariableUtility.EntityModel.ContainsKey(x.ToEntityCode))
                    .GroupBy(x => new { x.Code, x.Name, x.ToEntityCode })
                    .Select(g => new { g.Key.Code, g.Key.Name, g.Key.ToEntityCode, Count = g.Count() })
                    .ToList();

                if (plantableItems.Count > 0)
                {
                    var plantScript = new ScriptItem
                    {
                        Name = "种植",
                        ScriptCode = "plant",
                        Description = "种点什么",
                        Items = new List<ScriptItem>()
                    };
                    foreach (var item in plantableItems)
                    {
                        plantScript.Items.Add(new ScriptItem
                        {
                            Name = $"{item.Name} x{item.Count}",
                            ScriptCode = $"plant:{item.ToEntityCode}",
                            Description = item.Name
                        });
                    }
                    result.Add(plantScript);
                }
            }

            if (Entity != null && !Entity.IsDeleted)
            {
                result.AddRange(Entity.GetInteractions());
            }

            return result;
        }
    }
}
