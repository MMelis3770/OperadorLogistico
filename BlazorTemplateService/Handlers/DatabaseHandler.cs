using DatabaseConnection;

namespace BlazorTemplateService.Handlers
{
    public class DatabaseHandler
    {
        private IDatabaseConnection _connection;

        public DatabaseHandler(IDatabaseConnection connection)
        {
            _connection = connection;
        }

        public bool Exists(string query, object _params = null)
        {
            return _connection.Query<dynamic>(query, _params).Any();
        }

        public T GetDynamic<T>(string query, object _params = null)
        {
            return _connection.Query<T>(query, _params).FirstOrDefault();
        }
    }
}
