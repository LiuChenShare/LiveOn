namespace LiveOn.Game.Items
{
    /// <summary>
    /// 物品 — 游戏中的可收集/可使用的物品
    /// </summary>
    public class Item
    {
        /// <summary>
        /// 物品唯一标识
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// 物品名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 物品编码，对应物品模板
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 物品创建时间（游戏内时间）
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 种植后生成的实体编码（仅种子类物品有值，如 "1" 表示种下后变成 code=1 的种子实体）
        /// </summary>
        public string ToEntityCode { get; set; }

        /// <summary>
        /// 物品类型，影响背包排序和 UI 展示
        /// </summary>
        public ItemType ItemType { get; set; } = ItemType.Other;

        /// <summary>
        /// 是否已被删除
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// 根据编码创建物品实例并初始化
        /// </summary>
        /// <param name="code">物品编码</param>
        /// <returns>创建成功返回实例，编码不存在返回 null</returns>
        public static Item Create(string code)
        {
            var template = Core.VariableUtility.ItemModel.GetValueOrDefault(code);
            if (template == null) return null;

            return new Item
            {
                Code = template.Code,
                Name = template.Name,
                ToEntityCode = template.ToEntityCode,
                ItemType = template.ItemType,
                Id = Guid.NewGuid().ToString(),
                CreateTime = MainGame.Instance.GameDate
            };
        }

        /// <summary>
        /// 根据编码初始化物品（兼容旧调用方式）
        /// </summary>
        public bool Init(string code)
        {
            var item = Create(code);
            if (item == null) return false;

            Name = item.Name;
            Code = item.Code;
            ToEntityCode = item.ToEntityCode;
            ItemType = item.ItemType;
            Id = item.Id;
            CreateTime = item.CreateTime;
            return true;
        }

        /// <summary>
        /// 标记物品为已删除
        /// </summary>
        /// <returns>是否删除成功（已删除的物品返回 false）</returns>
        public bool Deleted()
        {
            if (IsDeleted) return false;

            IsDeleted = true;
            return true;
        }
    }
}
