using System;
using System.Data.SqlClient;
using UtilClasses.Db.Extensions;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Db
{
    public class ConnectionManager
    {
        private readonly string _connectionString; 

        public ConnectionManager(string db, string source = null, string username = null, string password = null)
        {
            var builder = new SqlConnectionStringBuilder { DataSource = source ?? "localhost", InitialCatalog = db };
            if (username.IsNullOrEmpty() && password.IsNullOrEmpty())
                builder.IntegratedSecurity = true;
            else
            {
                builder.IntegratedSecurity = false;
                builder.UserID = username;
                builder.Password = password;
            }

            _connectionString = builder.ConnectionString;
        }

        public SqlConnection Open() => new SqlConnection(_connectionString).Opened();

        public T Do<T>(Func<SqlConnection, T> f)
        {
            using (var c = Open())
                return f(c);
        }
    }
}
