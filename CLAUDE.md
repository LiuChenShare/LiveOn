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
