namespace LiveOn.Game.DB
{
    public class DBFieldTypeAttribute : Attribute
    {
        public Type Type { get; set; }

        public DBFieldTypeAttribute()
        {
            //Console.WriteLine(nameof(DBFieldTypettribute));
        }
        public DBFieldTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}
