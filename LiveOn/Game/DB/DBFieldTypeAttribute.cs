namespace LiveOn.Game.DB
{
    /// <summary>
    /// 数据库字段类型映射特性，用于标注实体属性对应的数据库字段类型
    /// </summary>
    public class DBFieldTypeAttribute : Attribute
    {
        /// <summary>
        /// 数据库字段类型
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public DBFieldTypeAttribute()
        {
            //Console.WriteLine(nameof(DBFieldTypettribute));
        }

        /// <summary>
        /// 指定字段类型的构造函数
        /// </summary>
        /// <param name="type">数据库字段类型</param>
        public DBFieldTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}
