namespace LiveOn.Core
{
    public class VariableUtility
    {
        //1.账号  2.userid  3.apiKey  4.上一次使用apiKey时间
        public static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Tuple<string, int, string, DateTime>> ActiveApiKeys = new System.Collections.Concurrent.ConcurrentDictionary<string, Tuple<string, int, string, DateTime>>();

    }
}
