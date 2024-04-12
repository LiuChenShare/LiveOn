namespace LiveOn.Core
{
    public class CurrentUser
    {   /// <summary>
        /// 当前用户ID
        /// </summary>
        private static AsyncLocal<int> currentUserId;

        private static Object currentUserIdLock = new object();
        /// <summary>
        /// 当前用户Seesion
        /// </summary>
        private static AsyncLocal<String> currentUserToken;
        private static Object currentTokenLock = new object();
        /// <summary>
        /// 当前操作Seesion
        /// </summary>
        private static AsyncLocal<CommonUser> currentUser;
        private static Object currentUsersLock = new object();


        private static AsyncLocal<String> currentUserIP;
        private static Object currentUsersIPLock = new object();
        /// <summary>
        /// 获取请求的Token(非缓存)
        /// </summary>
        /// <returns></returns>
        public static CommonUser GetLoadUser()
        {
            if (null != currentUser && null != (currentUser.Value))
            {
                return currentUser.Value;
            }
            return null;
        }
        /// <summary>
        /// 设置请求用户
        /// </summary>
        /// <param name="userId"></param>
        public static void SetLoadUser(CommonUser user)
        {
            if (null == currentUser)
            {
                lock (currentUsersLock)
                {
                    if (null == currentUser)
                    {
                        currentUser = new AsyncLocal<CommonUser>() { Value = user };
                    }
                    else
                    {
                        currentUser.Value = user;
                    }
                }
            }
            else
            {
                currentUser.Value = user;
            }
        }



        /// <summary>
        /// 获取请求的Token(非缓存)
        /// </summary>
        /// <returns></returns>
        public static string GetLoadUserToken()
        {
            if (null != currentUserToken && !string.IsNullOrEmpty(currentUserToken.Value))
            {
                return currentUserToken.Value;
            }
            return string.Empty;
        }

        /// <summary>
        /// 设置登入ID
        /// </summary>
        /// <param name="userId"></param>
        public static void SetLoadUserToken(string userToken)
        {
            if (null == currentUserId)
            {
                lock (currentTokenLock)
                {
                    if (null == currentUserId)
                    {
                        currentUserToken = new AsyncLocal<string>() { Value = userToken };
                    }
                    else
                    {
                        currentUserToken.Value = userToken;
                    }
                }
            }
            else
            {
                currentUserToken.Value = userToken;
            }
        }


        /// <summary>
        /// 设置登入IP
        /// </summary>
        /// <param name="userId"></param>
        public static void SetLoadUserIP(string userip)
        {
            if (null == currentUserIP)
            {
                lock (currentUsersIPLock)
                {
                    if (null == currentUserIP)
                    {
                        currentUserIP = new AsyncLocal<string>() { Value = userip };
                    }
                    else
                    {
                        currentUserIP.Value = userip;
                    }
                }
            }
            else
            {
                currentUserIP.Value = userip;
            }
        }
        /// <summary>
        /// 获取请求的ID(非缓存)
        /// </summary>
        /// <returns></returns>
        public static int GetLoadUserId()
        {
            if (null != currentUserId && currentUserId.Value > 0)
            {
                return currentUserId.Value;
            }
            return 0;
        }



        public static string GetLoadUserIP()
        {
            if (null != currentUserIP && currentUserIP.Value != string.Empty)
            {
                return currentUserIP.Value;
            }
            return string.Empty;
        }

        /// <summary>
        /// 设置登入ID
        /// </summary>
        /// <param name="userId"></param>
        public static void SetLoadUserId(int userId)
        {
            if (null == currentUserId)
            {
                lock (currentUserIdLock)
                {
                    if (null == currentUserId)
                    {
                        currentUserId = new AsyncLocal<int>() { Value = userId };
                    }
                    else
                    {
                        currentUserId.Value = userId;
                    }
                }
            }
            else
            {
                currentUserId.Value = userId;
            }
        }
    }
}
