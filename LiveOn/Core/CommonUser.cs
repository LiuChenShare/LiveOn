namespace LiveOn.Core
{
    /// <summary>
    /// 通用用户
    /// </summary>
    public class CommonUser
    {
        /// <summary>
        /// 主键标识id
        /// </summary>
        public Guid UserGuid { get; set; }
        /// <summary>
        /// 主键标识id
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string account { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string password { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }


        /// <summary>
        /// 请求用户的IPV4
        /// </summary>
        public string IP { get; set; }

    }
}
