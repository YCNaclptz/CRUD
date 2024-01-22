namespace CRUD_Project.DataAccessLayer
{
    public interface IDbHelper
    {
        T Add<T>(T entity) where T : class;

        bool Update<T>(T entity) where T : class;

        bool Delete<T>(T entity) where T : class;

        IEnumerable<T> Query<T>() where T : class;

        T Find<T>(params object[] keyValues) where T : class;
    }
}
