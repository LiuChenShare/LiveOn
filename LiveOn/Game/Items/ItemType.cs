using System.ComponentModel;

namespace LiveOn.Game.Items
{
    /// <summary>
    /// 物品类型枚举，决定物品在背包中的排序优先级和 UI 展示
    /// </summary>
    public enum ItemType
    {
        /// <summary>种子类 — 有 ToEntityCode，可种植，排序优先级最高</summary>
        [Description("种子")]
        Seed = 0,

        /// <summary>材料类 — 基础资源材料</summary>
        [Description("材料")]
        Material = 1,

        /// <summary>装备类 — 可穿戴的装备</summary>
        [Description("装备")]
        Equipment = 2,

        /// <summary>消耗品类 — 一次性使用的道具</summary>
        [Description("消耗品")]
        Consumable = 3,

        /// <summary>其他类 — 默认类型，排序优先级最低</summary>
        [Description("其他")]
        Other = 9
    }
}
