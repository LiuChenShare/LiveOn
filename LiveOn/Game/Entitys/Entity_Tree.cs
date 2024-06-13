using LiveOn.Core;
using LiveOn.Game.Items;
using static LiveOn.Game.MainGame;

namespace LiveOn.Game.Entitys
{
    /// <summary>
    /// 树木实体专用
    /// </summary>
    public partial class Entity
    {

        public virtual async Task SecondsEventExecute_Tree(DateTime time)
        {
            //Thread thread = new Thread(() =>
            //{
            //    LifeTime.AddSeconds(1);

            //});
            //thread.Start();

            Execute_SecondsEvent_Tree();
        }
        public async Task Execute_SecondsEvent_Tree()
        {
            LifeTime.AddSeconds(1);

            if (LifeTime.Second == 0)
            {
                //MinutesEvent?.Invoke(LifeTime);

                if (LifeTime.Minute == 0)
                {
                    //HoursEvent?.Invoke(LifeTime);

                    //每小时成长一次
                    Tree_High = Tree_High * (1 + Tree_GrowthRate);


                    if (LifeTime.Hour == 0)
                    {
                        //DaysEvent?.Invoke(LifeTime);
                        
                        if (LifeTime.Day == 1)
                        {
                            //MonthsEvent?.Invoke(LifeTime);
                            
                        }
                    }
                }
            }
        }
        private List<ScriptItem> GetScript_Tree()
        {
            var result = new List<ScriptItem>();

            //砍树
            ScriptItem script_KanShu = new ScriptItem
            {
                Name = "砍树",
                ScriptCode = VariableUtility.Script_Tree_KanShu,
                Description = "砍掉这棵树，可以收获木料和树枝"
            };
            result.Add(script_KanShu);

            //修剪
            ScriptItem script_XiuJian = new ScriptItem
            {
                Name = "修剪",
                ScriptCode = VariableUtility.Script_Tree_XiuJian,
                Description = "修剪，可以收获树枝"
            };
            result.Add(script_XiuJian);

            return result;
        }


        private bool ExecuteScript_Tree(string scriptCode)
        {
            switch (scriptCode)
            {
                case VariableUtility.Script_Tree_KanShu:
                    //砍树获取木料和树枝
                    int high_integerPart = (int)Tree_High; // 取整数部分
                    double high_decimalPart = Tree_High - high_integerPart; // 取小数部分

                    int item1_quantity = (int)(high_decimalPart * 10);
                    int item2_quantity = high_integerPart;

                    var item1s = new List<Item>();
                    for (int i = 0; i < item1_quantity; i++)
                    {
                        var item = new Item();
                        if (item.Init("1"))             //树枝
                            item1s.Add(item);
                    }
                    for (int i = 0; i < item2_quantity; i++)
                    {
                        var item = new Item();
                        if (item.Init("2"))             //木材
                            item1s.Add(item);
                    }
                    Deleted();
                    return MainGame.Instance.AddItems(item1s);
                case VariableUtility.Script_Tree_XiuJian:
                    return _Script_Tree_XiuJian();
                default:
                    return false;
            }
        }

        /// <summary>
        /// 修剪树枝，减少树木高度，获取树枝
        /// </summary>
        /// <returns></returns>
        private bool _Script_Tree_XiuJian()
        {
            var high = Tree_High * 0.2;
            Tree_High = Tree_High - high;

            int high_integerPart = (int)high; // 取整数部分
            double high_decimalPart = high - high_integerPart; // 取小数部分

            int item1_quantity = (int)(high_decimalPart * 10);
            int item2_quantity = high_integerPart;

            var item1s = new List<Item>();
            for (int i = 0; i < item1_quantity; i++)
            {
                var item = new Item();
                if (item.Init("1"))             //树枝
                    item1s.Add(item);
            }
            for (int i = 0; i < item2_quantity; i++)
            {
                var item = new Item();
                if (item.Init("2"))             //木材
                    item1s.Add(item);
            }
            return MainGame.Instance.AddItems(item1s);
        }
    }

}
