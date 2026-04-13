using LiveOn.Game.Items;
using System.Text.Json;
using log4net;

namespace LiveOn.Game.Entitys
{
    /// <summary>
    /// 树木实体，具有生长、砍树、修剪行为
    /// </summary>
    public class TreeEntity : Entity
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TreeEntity));

        static TreeEntity()
        {
            Register<TreeEntity>("0");
        }

        /// <summary>树高</summary>
        public double TreeHigh { get; set; }

        /// <summary>生长速率</summary>
        public double GrowthRate { get; set; }


        /// <summary>
        /// 初始化树木属性
        /// </summary>
        public override void InitProperties()
        {
            Name = "杂树";
            Description = "这是一颗不知名的树木。";
            TreeHigh = 0.3;
            GrowthRate = 0.3;
        }

        /// <summary>
        /// 获取树木可执行的交互列表（砍树、修剪）
        /// </summary>
        public override List<ScriptItem> GetInteractions()
        {
            return new List<ScriptItem>
            {
                new ScriptItem
                {
                    Name = "砍树",
                    ScriptCode = "chop",
                    Description = "砍掉这棵树，可以收获木料和树枝"
                },
                new ScriptItem
                {
                    Name = "修剪",
                    ScriptCode = "prune",
                    Description = "修剪，可以收获树枝"
                },
            };
        }

        /// <summary>
        /// 执行树木交互（砍树或修剪）
        /// </summary>
        public override (bool success, List<Item> drops, bool destroy) ExecuteInteraction(string interactionId)
        {
            if (interactionId == "chop") return Chop();
            if (interactionId == "prune") return Prune();
            return (false, null, false);
        }

        /// <summary>
        /// 树木秒事件：每小时按生长速率增长树高
        /// </summary>
        public override Task OnTick(DateTime time)
        {
            LifeTime = LifeTime.AddSeconds(1);

            if (LifeTime.Second == 0 && LifeTime.Minute == 0)
            {
                // 每小时成长一次
                TreeHigh = TreeHigh * (1 + GrowthRate);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 砍树：计算掉落物品并销毁实体
        /// </summary>
        private (bool success, List<Item> drops, bool destroy) Chop()
        {
            var drops = CalculateDrops(TreeHigh);
            return (true, drops, true);
        }

        /// <summary>
        /// 修剪：减少树高并计算掉落物品
        /// </summary>
        private (bool success, List<Item> drops, bool destroy) Prune()
        {
            var cutHigh = TreeHigh * 0.2;
            TreeHigh = TreeHigh - cutHigh;

            var drops = CalculateDrops(cutHigh);
            return (true, drops, false);
        }

        /// <summary>
        /// 根据高度计算掉落物品：整数部分=木材，小数部分*10=树枝
        /// </summary>
        private List<Item> CalculateDrops(double height)
        {
            var items = new List<Item>();
            int intPart = (int)height;
            double decPart = height - intPart;

            for (int i = 0; i < (int)(decPart * 10); i++)
            {
                var item = new Item();
                if (item.Init("1")) items.Add(item);
            }
            for (int i = 0; i < intPart; i++)
            {
                var item = new Item();
                if (item.Init("2")) items.Add(item);
            }
            return items;
        }

        /// <summary>
        /// 序列化树木属性
        /// </summary>
        public override string SerializeProperties()
        {
            return JsonSerializer.Serialize(new { tree_high = TreeHigh, growth_rate = GrowthRate });
        }

        /// <summary>
        /// 反序列化树木属性
        /// </summary>
        public override void DeserializeProperties(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("tree_high", out var h)) TreeHigh = h.GetDouble();
                if (doc.RootElement.TryGetProperty("growth_rate", out var r)) GrowthRate = r.GetDouble();
            }
            catch (Exception ex)
            {
                Log.Error($"反序列化树木属性失败: {json}", ex);
            }
        }
    }
}
