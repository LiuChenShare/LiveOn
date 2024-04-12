using static LiveOn.Game.MainGame;

namespace LiveOn.Game.Entity
{
    /// <summary>
    /// 树木实体专用
    /// </summary>
    public partial class Entity
    {

        public virtual async Task SecondsEventExecute_X0(DateTime time)
        {
            //Thread thread = new Thread(() =>
            //{
            //    LifeTime.AddSeconds(1);

            //});
            //thread.Start();

            xxxxxxx_X0();
        }
        public async Task xxxxxxx_X0()
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
        private List<ScriptItem> GetScript_X0()
        {
            var result = new List<ScriptItem>();

            //砍树
            
            throw new NotImplementedException();
        }
    }

}
