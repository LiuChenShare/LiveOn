using LiveOn.Game;
using static LiveOn.EvTest;

namespace LiveOn
{
    public class EvTest
    {


        public delegate Task XXXHandler(string time);//声明委托
        /// <summary>秒事件 </summary>
        public event XXXHandler XXXEvent;

        public delegate void BXXHandler(string time);//声明委托
        /// <summary>秒事件 </summary>
        public event BXXHandler BXXEvent;

        public  void XXX()
        {
            XXXEvent += RoundEventExecute2;
            XXXEvent += RoundEventExecute;


            //XXXEvent?.Invoke("3");

            //XXXEvent.BeginInvoke("3", null, null);

            //Task.Run(() => XXXEvent?.BeginInvoke("3", null, null));

            //有效，但是同一个委托绑定的事件还是按照顺序执行的
            //Task.Run(() => XXXEvent?.Invoke("3"));

            // 完美实现
            // 创建一个任务数组，包含所有事件处理器的异步操作
            Task[] tasks = XXXEvent.GetInvocationList().Cast<XXXHandler>()
                                       .Select(handler => Task.Run(() => handler("3"))).ToArray();

            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(100);
                Console.WriteLine(999);
            }
        }

        public  void XXX1(string time)
        {

        }
        public async Task RoundEventExecute(string time)
        {

            for (int i = 0; i < 30; i++)
            {
                Thread.Sleep(100);
                Console.WriteLine(1);
            }
        }
        public async Task RoundEventExecute2(string time)
        {

            for (int i = 0; i < 30; i++)
            {
                Thread.Sleep(100);
                Console.WriteLine(2);
            }
        }
    }
}
