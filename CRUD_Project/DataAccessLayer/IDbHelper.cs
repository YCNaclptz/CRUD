namespace CRUD_Project.DataAccessLayer
{
    public interface IDbHelper
    {
        void Add<T>(T entity) where T : class;

        void Update<T>(T entity) where T : class;

        bool Delete<T>(T entity) where T : class;

        bool Delete<T>(IEnumerable<T> entitys) where T : class;

        IEnumerable<T> Query<T>() where T : class;

        T Find<T>(params object[] keyValues) where T : class;

        int SaveChanges();
    }
}
