using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Newtonsoft.Json;
using UtilClasses.Db.Extensions;

namespace UtilClasses.Db
{
    public class DbWrapper
    {
        private readonly string _connectionString;
        private readonly IEnumerable<SqlCommand> _onConnectionCommands;
        readonly JsonSerializerSettings _jsonSettings;
        public DbWrapper(string connectionString, JsonSerializerSettings jsonSettings) : this(connectionString, jsonSettings,new SqlCommand[] { })
        {}
        public  DbWrapper(string connectionString, JsonSerializerSettings jsonSettings, IEnumerable<SqlCommand> onConnectionCommands)
        {
            _connectionString = connectionString;
            _onConnectionCommands = onConnectionCommands;
            _jsonSettings = jsonSettings ?? new JsonSerializerSettings();

        }

        private void _conn_StateChange(object sender, StateChangeEventArgs e)
        {
            if (e.OriginalState != ConnectionState.Closed || e.CurrentState != ConnectionState.Open) return;
            var conn = (SqlConnection)sender;
            foreach (var cmd in _onConnectionCommands)
            {
                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
            }
        }

        private SqlConnection GetConn()
        {
            var conn = new SqlConnection(_connectionString);
            conn.StateChange += _conn_StateChange;
            conn.Open();
            return conn;
        }

        public List<T> Query<T>(IDbCall<List<T>> call)
        {
            var ret = QueryEnumerable(call);
            return ret as List<T> ?? ret.ToList();
        }

        public IEnumerable<T> Query<T>(IDbCall<IEnumerable<T>> call) => QueryEnumerable(call);
        private IEnumerable<T> QueryEnumerable<T>(IDbCall<IEnumerable<T>> call) =>
            OnConn(conn => conn.Query<T>(call.Procedure,
                (DynamicParameters)call.ParameterHelper,
                commandType: CommandType.StoredProcedure)
            );
        public List<T> Query<T>(IJsonDbCall<List<T>> call) =>
            JsonConvert.DeserializeObject<List<T>>(OnConn(conn => conn.ExecuteGetJson(call)), _jsonSettings);

        public T QueryOne<T>(IDbCall<T> call) =>
            OnConn(conn => conn.Query<T>(call.Procedure,
                (DynamicParameters) call.ParameterHelper,
                commandType: CommandType.StoredProcedure
            ).FirstOrDefault());

        public IEnumerable<T> Query<T>(string sql) => OnConn(conn => conn.Query<T>(sql, commandType: CommandType.Text));

        public string ExecuteGetXml(IDbCall<string> dbCall) => OnConn(conn => conn.ExecuteGetXml(dbCall));
        public string ExecuteGetJson(IDbCall<string> dbCall) => OnConn(conn => conn.ExecuteGetJson(dbCall));
        public string ExecuteGetJson(IJsonDbCall dbCall) => OnConn(conn => conn.ExecuteGetJson(dbCall));
        public string ExecuteScalar(string sql) => OnConn(conn => conn.ExecuteScalar(sql).ToString());
        public string ExecuteScalar(IDbCall<string> dbCall) => OnConn(conn => conn.ExecuteScalar(dbCall));
        public int ExecuteScalar(IValDbCall<int> dbCall)=>OnConn(conn=>int.Parse(conn.ExecuteScalar(dbCall).ToString()));

        private T OnConn<T>(Func<SqlConnection, T> f)
        {
            using (var conn = GetConn())
            {
                
                return f(conn);
            }
        }

        public T ExecuteScalar<T>(IDbCall<T> dbCall) => OnConn(conn => conn.ExecuteScalar<T>(dbCall));
        public T ExecuteScalar<T>(IValDbCall<T> dbCall) where T : struct, IConvertible => OnConn(conn => conn.ExecuteScalar(dbCall));


        public SqlCommand ExecuteNonQuery(IDbCall dbCall) => OnConn(conn => conn.ExecuteNonQuery(dbCall));
        public SqlCommand ExecuteNonQuery(string sql) => OnConn(conn => conn.ExecuteNonQuery(sql));

        public IEnumerable<T> QueryXml<T>(IDbCall<IEnumerable<T>> call) => OnConn(conn=>conn.QueryXml(call));

        public T QueryXmlOne<T>(IDbCall<T> call) where T:class => OnConn(conn => conn.QueryXmlOne(call));

        public T QueryOne<T>(IJsonDbCall<T> call) where T : class =>
            OnConn(conn => JsonConvert.DeserializeObject<T>(conn.ExecuteGetJson(call), _jsonSettings));
    }
}
