using LiveOn.Core;
using LiveOn.Game.Entitys;
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

        public Entity Entity { get; set; }


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
                    Name = "种植",
                    ScriptCode = VariableUtility.Script_ZhongZhi,
                    Description = "种点什么"
                };
                result.Add(script_KanShu);
            }

            if (Entity != null && !Entity.IsDeleted)
            {
                result.AddRange(Entity.GetScript());
            }

            return result;
        }
    }
}
