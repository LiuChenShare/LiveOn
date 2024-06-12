using LiveOn.Core;
using static LiveOn.Game.MainGame;

namespace LiveOn.Game.Entity
{
    /// <summary>
    /// 种子实体专用
    /// </summary>
    public partial class Entity
    {

        public virtual async Task SecondsEventExecute_Seed(DateTime time)
        {
            //Thread thread = new Thread(() =>
            //{
            //    LifeTime.AddSeconds(1);

            //});
            //thread.Start();

            Execute_SecondsEvent_Seed();
        }
        public async Task Execute_SecondsEvent_Seed()
        {
            LifeTime.AddSeconds(1);
            
            if (LifeTime.Second == 0)
            {
                //MinutesEvent?.Invoke(LifeTime);
                TimeSpan timeOfDay = LifeTime.TimeOfDay;
                double totalMinutes = timeOfDay.TotalMinutes; // 将时间部分转换为总分钟数
                if (totalMinutes > SeedGrowthTime)      // 成长为指定实体
                {
                    // 成长为指定实体
                    MainGame.Instance.SecondsEvent -= SecondsEventExecute;
                    Init(ToCode);
                }
            }
        }
        private List<ScriptItem> GetScript_Seed()
        {
            var result = new List<ScriptItem>();

            return result;
        }


        private bool ExecuteScript_Seed(string scriptCode)
        {
            switch (scriptCode)
            {
                default:
                    return false;
            }
        }

    }

}
