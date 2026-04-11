using Dapper;
using System.Data;
using System.Reflection;

namespace LiveOn.Game.DB
{
    public static class DBUpdateHelper
    {
        /// <summary>
        /// 检查并更新数据库
        /// </summary>
        public static void DbVersionCheck()
        {
            var dBContext = new SQLiteDBContext();
            using var conn = dBContext.GetConn();
            DBUpdate(conn);
        }

        /// <summary>
        /// 执行版本迁移
        /// </summary>
        public static void DBUpdate(IDbConnection conn)
        {
            int maxVersion = SQLiteDBScript.DBScript.Keys.Max();
            var sqlSearch = @"SELECT count(*) FROM sqlite_master WHERE type='table' AND name='DBVersion';";
            var sqlGetVersion = @"SELECT Version FROM DBVersion ORDER BY Version DESC;";
            var sqlInsertVersion = @"INSERT INTO DBVersion VALUES (@Version, @CreateTime);";

            int tableExists = conn.QueryFirstOrDefault<int>(sqlSearch);
            int version = -1;
            if (tableExists > 0)
                version = conn.Query<int>(sqlGetVersion).FirstOrDefault();

            if (version < maxVersion)
            {
                for (var v = version + 1; v <= maxVersion; v++)
                {
                    if (SQLiteDBScript.DBScript.ContainsKey(v))
                    {
                        conn.Execute(SQLiteDBScript.DBScript[v]);
                        conn.Execute(sqlInsertVersion, new { Version = v, CreateTime = DateTime.Now.ToString() });
                    }
                }
            }
        }

        /// <summary>
        /// 查询并映射为实体列表
        /// </summary>
        public static List<T> GetModelFromSql<T>(this IDbConnection connection, string sql, object param = null)
        {
            var dataTable = connection.QueryToDataTable(sql, param);
            return GetModelFromDB<T>(dataTable);
        }

        private static List<T> GetModelFromDB<T>(DataTable dt)
        {
            var data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                data.Add(GetItem<T>(row));
            }
            return data;
        }

        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (!pro.Name.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (dr[column.ColumnName] == DBNull.Value)
                    {
                        pro.SetValue(obj, null);
                        break;
                    }

                    string typeName = pro.PropertyType.Name;
                    switch (typeName)
                    {
                        case "Guid":
                            pro.SetValue(obj, Guid.Parse(dr[column.ColumnName].ToString()));
                            break;
                        case "Int32":
                            pro.SetValue(obj, Convert.ToInt32(dr[column.ColumnName]));
                            break;
                        case "Double":
                        case "Single":
                            pro.SetValue(obj, Convert.ToDouble(dr[column.ColumnName]));
                            break;
                        case "String":
                            pro.SetValue(obj, dr[column.ColumnName].ToString());
                            break;
                        case "DateTime":
                            pro.SetValue(obj, Convert.ToDateTime(dr[column.ColumnName]));
                            break;
                        case "Boolean":
                            pro.SetValue(obj, Convert.ToInt32(dr[column.ColumnName]) != 0);
                            break;
                        default:
                            if (pro.PropertyType.BaseType != null && pro.PropertyType.BaseType.Name == "Enum")
                            {
                                pro.SetValue(obj, Convert.ToInt32(dr[column.ColumnName]));
                            }
                            else
                            {
                                pro.SetValue(obj, Convert.ChangeType(dr[column.ColumnName], pro.PropertyType));
                            }
                            break;
                    }
                    break;
                }
            }
            return obj;
        }

        public static DataTable QueryToDataTable(this IDbConnection connection, string sql, object param = null)
        {
            using var reader = connection.ExecuteReader(sql, param);
            var dt = new DataTable();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                dt.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }
            while (reader.Read())
            {
                var row = dt.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = reader[i];
                }
                dt.Rows.Add(row);
            }
            reader.Close();
            return dt;
        }
    }
}
