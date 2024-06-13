using LiveOn.Core;
using LiveOn.Game.Entitys;
using System.Xml.Linq;

namespace LiveOn.Game.Items
{
    public partial class Item
    {
        public string Id { get; private set; }
        public string Name { get; set; }

        public string Code { get; set; }

        public DateTime  CreateTime { get; set; }



        public bool IsDeleted { get; private set; }


        public bool Init(string code)
        {
            var itemModel = VariableUtility.ItemModel.GetValueOrDefault(code);

            if (itemModel == null)
                return false;

            Name = itemModel.Name;
            Code = itemModel.Code;
            Id = Guid.NewGuid().ToString();
            CreateTime = MainGame.Instance.GameDate;

            return true;
        }

        public bool Deleted()
        {
            if(IsDeleted) return false;

            IsDeleted = true;
            return true;
        }
    }
}
