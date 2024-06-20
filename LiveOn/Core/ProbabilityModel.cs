namespace LiveOn.Core
{
    /// <summary>
    /// 概率模型
    /// </summary>
    public class ProbabilityModel
    {
        /// <summary>
        /// 类型编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 概率0-1
        /// </summary>
        public double Probability {  get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
    }
}
