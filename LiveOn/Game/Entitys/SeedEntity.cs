using System.Text.Json;
using log4net;

namespace LiveOn.Game.Entitys
{
    /// <summary>
    /// 种子实体，到达成长时间后转化为目标实体
    /// </summary>
    public class SeedEntity : Entity
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SeedEntity));

        static SeedEntity()
        {
            Register<SeedEntity>("1");
        }

        /// <summary>成长所需游戏分钟数</summary>
        public int GrowthTime { get; set; }

        /// <summary>成熟后转化目标实体编码</summary>
        public string ToCode { get; set; }


        /// <summary>
        /// 初始化种子属性
        /// </summary>
        public override void InitProperties()
        {
            Name = "杂树种子";
            Description = "这是一颗平平无奇的种子，有可能种出一颗成材的大树。";
            GrowthTime = 10;
            ToCode = "0";
        }

        /// <summary>
        /// 种子暂无可执行交互
        /// </summary>
        public override List<ScriptItem> GetInteractions()
        {
            return new List<ScriptItem>();
        }

        /// <summary>
        /// 种子暂无可执行交互
        /// </summary>
        public override (bool success, List<Items.Item> drops, bool destroy) ExecuteInteraction(string interactionId)
        {
            return (false, null, false);
        }

        /// <summary>
        /// 种子秒事件：累计生命时长，到达成长时间后转化为目标实体
        /// </summary>
        public override Task OnTick(DateTime time)
        {
            LifeTime = LifeTime.AddSeconds(1);

            if (LifeTime.Second == 0)
            {
                double totalMinutes = LifeTime.TimeOfDay.TotalMinutes;
                if (totalMinutes > GrowthTime)
                {
                    // 注销当前秒事件
                    MainGame.Instance.SecondsEvent -= OnTick;

                    // 创建目标实体并重新初始化
                    var targetEntity = Entity.Create(ToCode);
                    if (targetEntity != null)
                    {
                        targetEntity.Init(ToCode);

                        // 将自己替换为新生成的实体（通过 Block 设置）
                        // 需要外部（MainGame）处理 Block.Entity 的替换
                        // 这里通过一个事件或返回值通知调用方
                        // 简化处理：直接在种子所在的 Block 上替换
                        var block = MainGame.Instance.FindBlockByEntity(this);
                        if (block != null)
                        {
                            block.Entity = targetEntity;
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 序列化种子属性
        /// </summary>
        public override string SerializeProperties()
        {
            return JsonSerializer.Serialize(new { growth_time = GrowthTime, to_code = ToCode });
        }

        /// <summary>
        /// 反序列化种子属性
        /// </summary>
        public override void DeserializeProperties(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("growth_time", out var g)) GrowthTime = g.GetInt32();
                if (doc.RootElement.TryGetProperty("to_code", out var t)) ToCode = t.GetString() ?? "";
            }
            catch (Exception ex)
            {
                Log.Error($"反序列化种子属性失败: {json}", ex);
            }
        }
    }
}
