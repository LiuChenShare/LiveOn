using System.Drawing;

namespace LiveOn.Game
{
    public class Block
    {
        /// <summary>
        /// 区块id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 状态    空闲、、、、、
        /// </summary>
        public int Stata { get; set; }

        //public string Item { get; set; }

        public Game.Entity.Entity Entity { get; set; }
    }
}
