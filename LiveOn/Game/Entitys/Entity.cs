using System.Text.Json;

namespace LiveOn.Game.Entitys
{
    /// <summary>
    /// 实体基类，表示游戏中的各种可交互对象
    /// 子类通过静态构造函数调用 Register 注册自己，新增实体只需新建子类文件
    /// </summary>
    public abstract class Entity
    {
        #region 静态注册

        /// <summary>
        /// 实体注册表，Key 为编码，Value 为工厂方法
        /// </summary>
        private static readonly Dictionary<string, Func<Entity>> _registry = new();

        /// <summary>
        /// 子类在静态构造函数中调用，将自身注册到工厂
        /// </summary>
        /// <typeparam name="T">子类类型，必须有 public 无参构造函数</typeparam>
        /// <param name="code">实体编码</param>
        protected static void Register<T>(string code) where T : Entity, new()
        {
            _registry[code] = () => new T();
        }

        /// <summary>
        /// 根据编码创建对应子类实例，未注册的编码返回 null
        /// </summary>
        public static Entity Create(string code)
        {
            return _registry.TryGetValue(code, out var factory) ? factory() : null;
        }

        #endregion


        #region 公共属性

        /// <summary>实体唯一标识</summary>
        public string Id { get; internal set; }

        /// <summary>实体编码（对应注册时的 code）</summary>
        public string Code { get; set; }

        /// <summary>实体名称</summary>
        public string Name { get; set; }

        /// <summary>实体描述</summary>
        public string Description { get; set; }

        /// <summary>成长阶段</summary>
        public int Stage { get; set; }

        /// <summary>生命时长</summary>
        public DateTime LifeTime { get; set; }

        /// <summary>是否已删除</summary>
        public bool IsDeleted { get; private set; }

        #endregion


        #region 公共方法

        /// <summary>
        /// 根据编码初始化实体：创建实例 → 设置公共字段 → 初始化子类属性 → 注册秒事件
        /// </summary>
        public bool Init(string code)
        {
            // 如果当前实例已经是正确的类型（由 Entity.Create 创建），直接初始化
            Code = code;
            Name = GetType().Name; // 默认用类名，子类可在 InitProperties 中覆盖
            Description = "";
            Id = Guid.NewGuid().ToString();
            LifeTime = DateTime.MinValue;
            Stage = 0;

            InitProperties();

            // 注册秒事件
            MainGame.Instance.SecondsEvent += OnTick;

            return true;
        }

        /// <summary>
        /// 删除实体，标记为已删除并注销秒事件
        /// </summary>
        public bool Deleted()
        {
            IsDeleted = true;
            MainGame.Instance.SecondsEvent -= OnTick;
            return true;
        }

        #endregion


        #region 子类必须实现的抽象方法

        /// <summary>
        /// 初始化子类特有属性（在 Init 中调用）
        /// </summary>
        public abstract void InitProperties();

        /// <summary>
        /// 获取可执行的交互列表
        /// </summary>
        public abstract List<ScriptItem> GetInteractions();

        /// <summary>
        /// 执行指定交互
        /// </summary>
        /// <param name="interactionId">交互 ID</param>
        /// <returns>(是否成功, 掉落物品列表, 是否销毁实体)</returns>
        public abstract (bool success, List<Items.Item> drops, bool destroy) ExecuteInteraction(string interactionId);

        /// <summary>
        /// 秒事件处理
        /// </summary>
        public abstract Task OnTick(DateTime time);

        #endregion


        #region 序列化（子类重写）

        /// <summary>
        /// 将子类特有属性序列化为 JSON 字符串，用于数据库持久化
        /// </summary>
        public virtual string SerializeProperties() => "{}";

        /// <summary>
        /// 从 JSON 字符串恢复子类特有属性，用于数据库加载
        /// </summary>
        public virtual void DeserializeProperties(string json) { }

        #endregion
    }
}
