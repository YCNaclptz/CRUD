
using CRUD_Project.EntityAttributes;
using CRUD_Project.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace CRUD_Project.DataAccessLayer
{
    public class SqlDbHelper: IDbHelper
    {
        private SqlConnection m_oConn;
        private string m_szConn = "Data Source=127.0.0.1,1439;Database=northwind;User ID=admin;Password=admin123;TrustServerCertificate=true;";
        private bool m_bIsTransaction = false;
        private SqlTransaction m_oTransaction;
        Dictionary<Type, SqlDbType> typeToSqlDbTypeMapping = new Dictionary<Type, SqlDbType>
        {
            { typeof(int), SqlDbType.Int },
            { typeof(int?), SqlDbType.Int },
            { typeof(long), SqlDbType.BigInt },
            { typeof(short), SqlDbType.SmallInt },
            { typeof(byte), SqlDbType.TinyInt },
            { typeof(float), SqlDbType.Real },
            { typeof(double), SqlDbType.Float },
            { typeof(decimal), SqlDbType.Decimal },
            { typeof(string), SqlDbType.NVarChar },
            { typeof(char), SqlDbType.NChar },
            { typeof(bool), SqlDbType.Bit },
            { typeof(DateTime), SqlDbType.DateTime },
            { typeof(DateTime?), SqlDbType.DateTime },
            { typeof(Guid), SqlDbType.UniqueIdentifier },
            { typeof(byte[]), SqlDbType.Image },
        };
        public SqlDbHelper(string connString)
        {
            m_szConn = connString;
        }
        private void OpenDbResource(bool IsTransaction = false)
        {
            m_oConn = new SqlConnection(m_szConn);
            m_oConn.Open();
            if (IsTransaction)
            {
                m_oTransaction = m_oConn.BeginTransaction();
            }
        }
        private void CloseDbResource()
        {
            if (m_oConn != null && m_oConn.State == ConnectionState.Open)
            {
                m_oConn.Close();
            }
        }
        private SqlCommand CreateDbCommand()
        {
            SqlCommand cmd = m_oConn.CreateCommand();
            if (m_bIsTransaction)
            {
                cmd.Transaction = m_oTransaction;
            }
            return cmd;
        }

        private void Commit()
        {
            if (m_bIsTransaction == false)
            {
                throw new Exception("No transaction is started.");
            }
            else
            {
                m_oTransaction.Commit();
            }
        }
        private void Rollback()
        {
            if (m_bIsTransaction == false)
            {
                throw new Exception("No transaction is started.");
            }
            else
            {
                m_oTransaction.Rollback();
            }
        }
        public T Add<T>(T entity) where T : class
        {
            Type type = typeof(T);
            PropertyInfo[] aProp = type.GetProperties();
            bool hasAttribute = Attribute.IsDefined(type, typeof(TableNameAttribute));
            string szTableName = string.Empty;
            if (hasAttribute)
            {
                TableNameAttribute attribute = (TableNameAttribute)type.GetCustomAttribute(typeof(TableNameAttribute));
                szTableName = attribute.Name;
            }
            else
            {
                szTableName = type.Name;
            }
            var oPKey = GetPrimaryKeyPF<T>();
            //INSERTED.ColumnName: 回傳插入後的欄位值
            //組合資料表中所有的主鍵欄位
            string szSqlOutputVar = oPKey.Select(p => $"INSERTED.{p.Name}").Aggregate((pre, curr) => $"{pre}, {curr}");
            string szSQL = string.Empty;
            string[] aKeys = aProp.Where(p => IsPrimaryKey(p) == false).Select(p => p.Name).ToArray();
            string[] aValues = aProp.Where(p => IsPrimaryKey(p) == false).Select(p => "@" + p.Name).ToArray();
            if (oPKey is null)
            {
                throw new NullReferenceException("主鍵為空！");
            }
            szSQL = $"INSERT {szTableName} ({string.Join(",", aKeys)}) OUTPUT {szSqlOutputVar} VALUES ({string.Join(",", aValues)})";
            try
            {
                OpenDbResource();
                using (var oCmd = CreateDbCommand())
                {
                    oCmd.CommandText = szSQL;

                    foreach (var prop in aProp.Where(p => IsPrimaryKey(p) == false))
                    {
                        object propValue = prop.GetValue(entity);
                        if (propValue == null)
                        {
                            propValue = DBNull.Value;
                        }
                        oCmd.Parameters.Add("@" + prop.Name, typeToSqlDbTypeMapping[prop.PropertyType]).Value = propValue;
                    }

                    using (var reader = oCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);
                                object value = reader.GetValue(i);
                                PropertyInfo oProp = oPKey.Where(p => p.Name == columnName).First();
                                oProp.SetValue(entity, value);
                            }
                        }
                    }
                    return entity;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                throw ex;
            }
            finally
            {
                CloseDbResource();
            }
        }

        public bool Delete<T>(T entity) where T : class
        {
            var type = typeof(T);
            var aPrimaryKey = GetPrimaryKeyPF<T>().ToArray();
            string szTableName = GetTableNameByType<T>();
            string szSQL = string.Empty;
            string szFilter = aPrimaryKey
                .Select(p => $"{p.Name} = @{p.Name}")
                .Aggregate((kv1, kv2) => $"{kv1} and {kv2}");
            szSQL = $"DELETE FROM {szTableName} WHERE {szFilter}";
            try
            {
                OpenDbResource();
                using (var oCmd = CreateDbCommand())
                {
                    oCmd.CommandText = szSQL;
                    foreach (var prop in aPrimaryKey)
                    {
                        oCmd.Parameters.Add("@" + prop.Name, typeToSqlDbTypeMapping[prop.PropertyType]).Value = prop.GetValue(entity);
                    }
                    int iResult = oCmd.ExecuteNonQuery();
                    if (iResult > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                throw ex;
            }
            finally
            {
                CloseDbResource();
            }

        }


        public T Find<T>(params object[] keyValues) where T : class
        {
            try
            {
                string szTableName = GetTableNameByType<T>();
                var aPrimaryKey = GetPrimaryKeyPF<T>().ToArray();
                if (aPrimaryKey.Count() != keyValues.Count())
                {
                    throw new ArgumentException("參數與主鍵數量不一致！");
                }

                OpenDbResource();
                using(var oCmd = CreateDbCommand())
                {
                    string szFilter = aPrimaryKey
                    .Select(p => $"{p.Name} = @{p.Name}")
                    .Aggregate((kv1, kv2) => $"{kv1} and {kv2}");
                    oCmd.CommandText = $"select * from {szTableName} where {szFilter}";

                    for (int i = 0; i < aPrimaryKey.Length; i++)
                    {
                        oCmd.Parameters.Add("@" + aPrimaryKey[i].Name, typeToSqlDbTypeMapping[aPrimaryKey[i].PropertyType]).Value = keyValues[i];
                    }

                    var oTable = new DataTable();
                    oTable.Load(oCmd.ExecuteReader());

                    if (oTable.Rows.Count > 0)
                    {
                        return ToModel<T>(oTable.Rows[0]);
                    }
                    return null;
                }
            }
            catch (ArgumentException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                throw ex;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                throw ex;
            }
            finally
            {
                CloseDbResource();
            }

        }

        
        public IEnumerable<T> Query<T>() where T : class
        {
            try
            {
                string szTableName = GetTableNameByType<T>();

                OpenDbResource();
                using(var oCmd = CreateDbCommand())
                {
                    oCmd.CommandText = $"SELECT TOP 10 * from {szTableName}";
                    var oTable = new DataTable();
                    oTable.Load(oCmd.ExecuteReader());

                    if (oTable.Rows.Count > 0)
                    {
                        return ToModelList<T>(oTable);
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                throw ex;
            }
            finally
            {
                CloseDbResource();
            }
        }

        public bool Update<T>(T entity) where T : class
        {
            try
            {
                var type = typeof(T);
                var aProp = type.GetProperties();
                var aPrimaryKey = GetPrimaryKeyPF<T>().ToArray();
                string szTableName = GetTableNameByType<T>();
                string szSQL = string.Empty;
                string szFilter = aPrimaryKey
                    .Select(p => $"{p.Name} = @{p.Name}")
                    .Aggregate((kv1, kv2) => $"{kv1} and {kv2}");
                //更新除了主鍵以外的欄位
                var aKeyValues = aProp.Where(p => IsPrimaryKey(p) == false).Select(p => $"{p.Name} = @{p.Name}");
                szSQL = $"UPDATE {szTableName} SET {string.Join(",", aKeyValues)} WHERE {szFilter}";
                System.Diagnostics.Debug.WriteLine(szSQL);
                OpenDbResource();
                using (var oCmd = CreateDbCommand())
                {
                    oCmd.CommandText = szSQL;
                    //參數包含主鍵及其他所有欄位
                    foreach (var prop in aProp)
                    {
                        Type propertyType = prop.PropertyType;
                        object propValue = prop.GetValue(entity);
                        if (propValue == null)
                        {
                            propValue = DBNull.Value;
                        }
                        oCmd.Parameters.Add("@" + prop.Name, typeToSqlDbTypeMapping[prop.PropertyType]).Value = propValue;

                    }
                    int iResult = oCmd.ExecuteNonQuery();
                    if (iResult > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                throw ex;
            }
        }

        private string GetTableNameByType<T>()
        {
            var type = typeof(T);
            bool hasAttribute = Attribute.IsDefined(type, typeof(TableNameAttribute));
            string szTableName = string.Empty;
            if (hasAttribute)
            {
                TableNameAttribute attribute = (TableNameAttribute)type.GetCustomAttribute(typeof(TableNameAttribute));
                szTableName = attribute.Name;
            }
            else
            {
                szTableName = type.Name;
            }
            return szTableName;
        }

        private bool IsPrimaryKey(PropertyInfo prop)
        {
            return Attribute.IsDefined(prop, typeof(PrimaryKeyAttribute));
        }

        private IEnumerable<PropertyInfo> GetPrimaryKeyPF<T>()
        {
            var type = typeof(T);
            var aProp = type.GetProperties();
            foreach (var prop in aProp)
            {
                if (IsPrimaryKey(prop))
                {
                    yield return prop;
                }
            }
            yield break;
        }

        public static T ToModel<T>(DataRow p_oRow)
        {
            Type type = typeof(T);
            PropertyInfo[] aProp = type.GetProperties();
            object instance = Activator.CreateInstance(type);
            foreach (var prop in aProp)
            {
                if (p_oRow.Table.Columns.Contains(prop.Name))
                {
                    Type propertyType = prop.PropertyType;
                    //如果是Nullable<T>，則取得T的型別
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        propertyType = Nullable.GetUnderlyingType(propertyType);
                    }
                    object value = p_oRow[prop.Name] == DBNull.Value ? null : Convert.ChangeType(p_oRow[prop.Name], propertyType);
                    p_oRow[prop.Name].GetType();
                    prop.SetValue(instance, value);
                }

            }
            return (T)instance;
        }

        public static IEnumerable<T> ToModelList<T>(DataTable p_oTable)
        {
            List<T> list = new List<T>();
            foreach (DataRow oRow in p_oTable.Rows)
            {
                list.Add(ToModel<T>(oRow));
            }
            return list;
        }

        
    }
}
