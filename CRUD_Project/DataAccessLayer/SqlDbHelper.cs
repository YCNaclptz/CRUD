using Microsoft.Data.SqlClient;
using System.Data;

namespace CRUD_Project.DataAccessLayer
{
    public class SqlDbHelper
    {
        private SqlConnection m_oConn;
        private string m_szConn = "Data Source=127.0.0.1,1439;Database=northwind;User ID=admin;Password=admin123;TrustServerCertificate=true;";
        private bool m_bIsTransaction = false;
        private SqlTransaction m_oTransaction;

        public SqlDbHelper(string connString)
        {
            m_szConn = connString;
        }
        public void OpenDbResource(bool IsTransaction)
        {
            m_oConn = new SqlConnection(m_szConn);
            m_oConn.Open();
            if (IsTransaction)
            {
                m_oTransaction = m_oConn.BeginTransaction();
            }
        }
        public void CloseDbResource()
        {
            m_oConn.Close();
        }
        public SqlCommand CreateDbCommand()
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
        public void Add<T>(T entity) where T : class
        {
            throw new NotImplementedException();
        }

        public bool Delete<T>(T entity) where T : class
        {
            throw new NotImplementedException();
        }

        public bool Delete<T>(IEnumerable<T> entitys) where T : class
        {
            throw new NotImplementedException();
        }

        public T Find<T>(params object[] keyValues) where T : class
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Query<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public int SaveChanges()
        {
            throw new NotImplementedException();
        }

        public void Update<T>(T entity) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
