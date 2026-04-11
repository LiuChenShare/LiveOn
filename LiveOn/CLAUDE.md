# LiveOn 项目规范

## 数据库

- 使用 Dapper + SQLite，不用 Entity Framework Core
- 连接管理：SQLiteDBContext，手动 GetConn()
- 建表/迁移：SQLiteDBScript 版本化 SQL 脚本（Dictionary<int, string>）
- 版本迁移：DBUpdateHelper.DBUpdate() 自研版本检查（DBVersion 表）
- 数据映射：DBUpdateHelper.GetModelFromSql<T>() 反射映射
- CRUD：DBResponse 静态类，原始 SQL，先查后判断 INSERT/UPDATE
- db 文件路径：{BaseDirectory}/Data/LiveOn.db
- 参考项目：D:\工作\GIT\AuraRevival
