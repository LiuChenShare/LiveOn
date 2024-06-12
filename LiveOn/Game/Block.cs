using LiveOn.Core;
using LiveOn.Game.Entity;
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


        /// <summary>
        /// 获取可执行的操作
        /// </summary>
        /// <returns></returns>
        public List<ScriptItem> GetScript()
        {
            var result = new List<ScriptItem>();

            if (Entity == null || Entity.IsDeleted) {
                //种植
                ScriptItem script_KanShu = new ScriptItem
                {
                    Name = "砍树",
                    ScriptCode = VariableUtility.Script_Tree_KanShu,
                    Description = "砍掉这棵树，可以收获木料和树枝"
                };
                result.Add(script_KanShu);
            }

            switch (Type)
            {
                case EntityType.Tree:
                    return GetScript_Tree();
                default:
                    return new List<ScriptItem>();
            }
        }
    }
}
