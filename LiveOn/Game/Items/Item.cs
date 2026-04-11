using LiveOn.Core;
using LiveOn.Game.Entitys;
using System.Xml.Linq;

namespace LiveOn.Game.Items
{
    /// <summary>
    /// 物品 — 游戏中的可收集/可使用的物品
    /// </summary>
    public partial class Item
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
        public DateTime  CreateTime { get; set; }



        /// <summary>
        /// 是否已被删除
        /// </summary>
        public bool IsDeleted { get; private set; }


        /// <summary>
        /// 根据物品编码初始化物品
        /// </summary>
        /// <param name="code">物品编码</param>
        /// <returns>是否初始化成功</returns>
        public bool Init(string code)
        {
            var itemModel = VariableUtility.ItemModel.GetValueOrDefault(code);

            if (itemModel == null)
                return false;

            Name = itemModel.Name;
            Code = itemModel.Code;
            Id = Guid.NewGuid().ToString();
            CreateTime = MainGame.Instance.GameDate;

            return true;
        }

        /// <summary>
        /// 标记物品为已删除
        /// </summary>
        /// <returns>是否删除成功（已删除的物品返回 false）</returns>
        public bool Deleted()
        {
            if(IsDeleted) return false;

            IsDeleted = true;
            return true;
        }
    }
}
