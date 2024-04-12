using LiveOn.Game.DB;
using System.ComponentModel;
using System.Drawing;
using System.Security.Principal;
using static LiveOn.EvTest;

namespace LiveOn.Game
{
    /// <summary>
    /// 主程序
    /// </summary>
    public class MainGame
    {
        #region 单例
        private static volatile MainGame instance;
        private static object syncRoot = new Object();
        public MainGame() { }
        public static MainGame Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MainGame();
                    }
                }
                return instance;
            }
        }
        #endregion



        #region 事件
        public delegate Task TimeHandler(DateTime time);//声明委托
        /// <summary>秒事件 </summary>
        public event TimeHandler SecondsEvent;
        /// <summary>分事件 </summary>
        public event TimeHandler MinutesEvent;
        /// <summary>时事件 </summary>
        public event TimeHandler HoursEvent;
        /// <summary>日事件 </summary>
        public event TimeHandler DaysEvent;
        /// <summary>月事件 </summary>
        public event TimeHandler MonthsEvent;

        /// <summary>
        /// 消息委托
        /// </summary>
        /// <param name="type">来源类型:0-游戏公告，1-区块，2-建筑，3-实体</param>
        /// <param name="source">来源名称</param>
        /// <param name="content">消息内容</param>
        public delegate void MsgHandler(int type, string source, string content);
        public event MsgHandler MsgEvent;

        /// <summary>
        /// 万能委托
        /// </summary>
        public delegate void AllHandler(object[] objects);
        /// <summary>
        /// 万能委托异步
        /// </summary>
        public delegate void AllTaskHandler(object[] objects);

        #endregion


        /// <summary>
        /// 游戏状态
        /// </summary>
        [DBFieldType(typeof(Int32))]
        public GameStateType GameState { get; private set; } = GameStateType.Init;


        //实例化Timer类，设置间隔时间为1秒；
        private readonly System.Timers.Timer GameTimer = new System.Timers.Timer(1000);
        /// <summary>
        /// 游戏时间
        /// </summary>
        public DateTime GameDate { get; private set; } = new DateTime();


        /// <summary>
        /// 区块
        /// </summary>
        public List<Block> Blocks { get; set; } = new List<Block>();




        public void GameStart()
        {
            //SecondsEvent += ShowMsg;

            GameTimer.Elapsed += new System.Timers.ElapsedEventHandler(Execute);//到达时间的时候执行事件；
            GameTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
            GameTimer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；

            GameTimer.Start(); //启动定时器
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            GameTimer.Enabled = false;
            GameState = GameStateType.Paused;
        }
        /// <summary>
        /// 继续游戏
        /// </summary>
        public void ProceedGame()
        {
            GameTimer.Enabled = true;
            GameState = GameStateType.InGame;
        }

        private void Execute(object source, System.Timers.ElapsedEventArgs e)
        {
            GameDate = GameDate.AddSeconds(1);
            //SecondsEvent?.Invoke(GameDate);
            Task[] tasksSeconds = SecondsEvent.GetInvocationList().Cast<TimeHandler>()
                                       .Select(handler => Task.Run(() => handler(GameDate))).ToArray();
            if (GameDate.Second == 0)
            {
                //MinutesEvent?.Invoke(GameDate);
                Task[] tasksMinutes = MinutesEvent.GetInvocationList().Cast<TimeHandler>()
                                           .Select(handler => Task.Run(() => handler(GameDate))).ToArray();
                if (GameDate.Minute == 0)
                {
                    //HoursEvent?.Invoke(GameDate);
                    Task[] tasksHours = HoursEvent.GetInvocationList().Cast<TimeHandler>()
                                               .Select(handler => Task.Run(() => handler(GameDate))).ToArray();
                    if (GameDate.Hour == 0)
                    {
                        //DaysEvent?.Invoke(GameDate);
                        Task[] tasksDays = DaysEvent.GetInvocationList().Cast<TimeHandler>()
                                                   .Select(handler => Task.Run(() => handler(GameDate))).ToArray();
                        if (GameDate.Day == 1)
                        {
                            //MonthsEvent?.Invoke(GameDate);
                            Task[] tasksMonths = MonthsEvent.GetInvocationList().Cast<TimeHandler>()
                                                       .Select(handler => Task.Run(() => handler(GameDate))).ToArray();
                        }
                    }
                }
            }
        }
    }




    /// <summary>
    /// 游戏状态
    /// </summary>
    public enum GameStateType
    {
        /// <summary>
        /// GameOver
        /// </summary>
        [Description("GameOver")]
        GameOver = -1,



        /// <summary>
        /// 初始化
        /// </summary>
        [Description("初始化")]
        Init = 0,
        /// <summary>
        /// 加载中
        /// </summary>
        [Description("加载中")]
        Load = 1,
        /// <summary>
        /// 游戏中
        /// </summary>
        [Description("游戏中")]
        InGame = 2,
        /// <summary>
        /// 暂停
        /// </summary>
        [Description("暂停")]
        Paused = 3,
    }
}
