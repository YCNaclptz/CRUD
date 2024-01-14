namespace CRUD_Project.EntityAttributes
{
    namespace CRUD_Project.DataAccessLayer.EntityAttribute
    {
        [AttributeUsage(AttributeTargets.Property)]
        public class PrimaryKeyAttribute : Attribute
        {

        }
        public class TableNameAttribute : Attribute
        {

            public TableNameAttribute(string name)
            {
                _name = name;
            }
            string _name;

            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }

        }
    }

}
