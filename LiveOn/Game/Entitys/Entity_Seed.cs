using LiveOn.Core;
using static LiveOn.Game.MainGame;

namespace LiveOn.Game.Entitys
{
    /// <summary>
    /// 种子实体专用逻辑
    /// </summary>
    public partial class Entity
    {

        /// <summary>
        /// 种子秒事件执行入口
        /// </summary>
        /// <param name="time">当前时间</param>
        public virtual async Task SecondsEventExecute_Seed(DateTime time)
        {
            //Thread thread = new Thread(() =>
            //{
            //    LifeTime.AddSeconds(1);

            //});
            //thread.Start();

            Execute_SecondsEvent_Seed();
        }
        /// <summary>
        /// 种子秒事件具体执行逻辑，累计生命时长，到达成长时间后转化为目标实体
        /// </summary>
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
        /// <summary>
        /// 获取种子可执行的操作列表
        /// </summary>
        /// <returns>操作列表</returns>
        private List<ScriptItem> GetScript_Seed()
        {
            var result = new List<ScriptItem>();

            return result;
        }


        /// <summary>
        /// 执行种子指定的操作脚本
        /// </summary>
        /// <param name="scriptCode">脚本编码</param>
        /// <returns>执行成功返回 true</returns>
        private bool ExecuteScript_Seed(int scriptCode)
        {
            switch (scriptCode)
            {
                default:
                    return false;
            }
        }

    }

}
