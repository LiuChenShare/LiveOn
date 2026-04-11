# LiveOn 项目规范

## 前后端分离

- 前端使用纯静态 HTML，放在 `wwwroot/` 下（login.html、game.html、logs.html）
- 不使用 Razor 视图（.cshtml），不使用 Tag Helper
- C# 后端只提供 API 接口（Controller 返回 Json）
- 前端通过 jQuery AJAX 调用 API，全局 401 自动跳转登录页
- 公共页面结构（head、navbar）直接内联在各 HTML 中

## 数据库

- 使用 Dapper + SQLite，不用 Entity Framework Core
- 连接管理：SQLiteDBContext，手动 GetConn()
- 建表/迁移：SQLiteDBScript 版本化 SQL 脚本（Dictionary<int, string>）
- 版本迁移：DBUpdateHelper.DBUpdate() 自研版本检查（DBVersion 表）
- 数据映射：DBResponse.GetModelFromSql<T>() 反射映射
- CRUD：DBResponse 静态类，原始 SQL，先查后判断 INSERT/UPDATE
- db 文件路径：{BaseDirectory}/Data/LiveOn.db
- 参考项目：D:\工作\GIT\AuraRevival

## 代码注释

- 所有 public 类、方法、属性必须有中文注释（C# 用 `///` XML 文档注释）
- 枚举值用 `[Description("中文")]` 标注
- 字段含义、枚举取值范围需在注释中说明
- 前端 JS 函数、CSS 自定义样式块需有中文注释说明用途

## 架构模式

- 双单例：Grain（世界容器，持有 Blocks/Items 等数据）+ MainGame（事件总线 + 游戏时钟）
- 数据与逻辑分离：MainGame 通过 `Grain.Instance` 读写数据，自身专注 Timer、事件分发、生命周期
- ScriptComd int 枚举统一管理操作码，不用 string 常量

## 认证方式

- Cookie + Authorization header 双重传递
- GlobalActionFilter 全局拦截，未认证返回 401 JSON
- 前端 `ajaxError` 全局处理 401，跳转 `/login.html`

## 前端技术栈

- Bootstrap 5.1 + jQuery
- 暗色森林主题，CSS 变量统一管理（`--bg-primary`、`--accent-green` 等，定义在 `site.css` :root 中）

## 游戏时钟

- System.Timers.Timer 1 秒驱动 Execute()
- 级联事件：秒→分→时→日→月，触发前做 null 检查
- 持久化策略：SaveGame 节流 5 秒，状态变更（开始/暂停/继续）用 `SaveGame(force: true)` 强制立即保存

## Git 分支

- `main` 稳定分支，`dev` 开发分支
