using LiveOn.Game.Items;

namespace LiveOn.Game.Entitys
{
    /// <summary>
    /// 树木实体专用逻辑
    /// </summary>
    public partial class Entity
    {

        /// <summary>
        /// 树木秒事件执行入口
        /// </summary>
        /// <param name="time">当前时间</param>
        public virtual async Task SecondsEventExecute_Tree(DateTime time)
        {
            Execute_SecondsEvent_Tree();
        }

        /// <summary>
        /// 树木秒事件具体执行逻辑，每小时按成长速率增长树高
        /// </summary>
        public async Task Execute_SecondsEvent_Tree()
        {
            LifeTime.AddSeconds(1);

            if (LifeTime.Second == 0)
            {
                if (LifeTime.Minute == 0)
                {
                    //每小时成长一次
                    Tree_High = Tree_High * (1 + Tree_GrowthRate);

                    if (LifeTime.Hour == 0)
                    {
                        if (LifeTime.Day == 1)
                        {
                            //MonthsEvent?.Invoke(LifeTime);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取树木可执行的操作列表（砍树、修剪）
        /// </summary>
        /// <returns>操作列表</returns>
        private List<ScriptItem> GetScript_Tree()
        {
            var result = new List<ScriptItem>();

            result.Add(new ScriptItem
            {
                Name = "砍树",
                ScriptCode = (int)ScriptComd.KanShu,
                Description = "砍掉这棵树，可以收获木料和树枝"
            });

            result.Add(new ScriptItem
            {
                Name = "修剪",
                ScriptCode = (int)ScriptComd.XiuJian,
                Description = "修剪，可以收获树枝"
            });

            return result;
        }


        /// <summary>
        /// 执行树木指定的操作脚本（砍树或修剪）
        /// </summary>
        /// <param name="scriptCode">脚本编码</param>
        /// <returns>执行成功返回 true</returns>
        private bool ExecuteScript_Tree(int scriptCode)
        {
            switch (scriptCode)
            {
                case (int)ScriptComd.KanShu:
                    int high_integerPart = (int)Tree_High;
                    double high_decimalPart = Tree_High - high_integerPart;

                    int item1_quantity = (int)(high_decimalPart * 10);
                    int item2_quantity = high_integerPart;

                    var item1s = new List<Item>();
                    for (int i = 0; i < item1_quantity; i++)
                    {
                        var item = new Item();
                        if (item.Init("1"))
                            item1s.Add(item);
                    }
                    for (int i = 0; i < item2_quantity; i++)
                    {
                        var item = new Item();
                        if (item.Init("2"))
                            item1s.Add(item);
                    }
                    Deleted();
                    Grain.Instance.Items.AddRange(item1s);
                    return true;
                case (int)ScriptComd.XiuJian:
                    return _Script_Tree_XiuJian();
                default:
                    return false;
            }
        }

        /// <summary>
        /// 修剪树枝，减少树木高度，获取树枝
        /// </summary>
        private bool _Script_Tree_XiuJian()
        {
            var high = Tree_High * 0.2;
            Tree_High = Tree_High - high;

            int high_integerPart = (int)high;
            double high_decimalPart = high - high_integerPart;

            int item1_quantity = (int)(high_decimalPart * 10);
            int item2_quantity = high_integerPart;

            var item1s = new List<Item>();
            for (int i = 0; i < item1_quantity; i++)
            {
                var item = new Item();
                if (item.Init("1"))
                    item1s.Add(item);
            }
            for (int i = 0; i < item2_quantity; i++)
            {
                var item = new Item();
                if (item.Init("2"))
                    item1s.Add(item);
            }
            Grain.Instance.Items.AddRange(item1s);
            return true;
        }
    }
}
