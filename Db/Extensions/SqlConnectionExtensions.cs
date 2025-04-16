using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;
using Newtonsoft.Json;
using UtilClasses.Core.Extensions.Enumerables;
using UtilClasses.Core.Extensions.Objects;
using UtilClasses.Core.Extensions.Strings;
using UtilClasses.Core.Extensions.Reflections;
namespace UtilClasses.Db.Extensions
{
    public static class SqlConnectionExtensions
    {
        public static string ExecuteGetXml(this SqlConnection conn, string sp, ParameterHelper ph, string nullValue)
        {
            string s=null;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sp;
                cmd.Parameters.AddRange(ph);
                cmd.CommandType = CommandType.StoredProcedure;
            
                try
                {
                    var rdr = cmd.ExecuteXmlReader();
                    while (rdr.Read())
                    {
                        s = rdr.ReadOuterXml();
                    }
                }
                catch (InvalidOperationException)
                {
                    return nullValue;
                }
            }
            return s.IsNullOrWhitespace() ? nullValue : s;
        }

        public static string ExecuteGetJson(this SqlConnection conn, string sp, ParameterHelper ph, string nullValue)
            => conn.OnCommand(sp, ph, cmd => cmd.ExecuteGetJson(nullValue));
        public static string ExecuteGetJson<T>(this SqlConnection conn, IJsonDbCall<T> call) => conn.ExecuteGetJson(call.Procedure, call.ParameterHelper, call.NullValue?.ToString());
        public static string ExecuteGetJson(this SqlConnection conn, IJsonDbCall call) => conn.ExecuteGetJson(call.Procedure, call.ParameterHelper,"");


        private static string ExecuteGetJson(this SqlCommand cmd, string nullValue)
        {
            var sb = new StringBuilder();
            try
            {
                var reader = cmd.ExecuteReader();
                if (!reader.HasRows) sb.Append(nullValue);
                else
                {
                    while (reader.Read())
                    {
                        sb.Append(reader.GetValue(0));
                    }
                }
                return sb.ToString();
            }
            catch (InvalidOperationException)
            {
                return nullValue;
            }
        }

        private static string OnCommand(this SqlConnection conn, string sp, ParameterHelper ph, Func<SqlCommand, string> f)
        {
            using (var cmd = conn.PrepCommand(sp, ph))
                return f(cmd);
        }

        private static SqlCommand PrepCommand<T>(this SqlConnection conn, IDbCall<T> call) =>
            PrepCommand(conn, call.Procedure, call.ParameterHelper);
        private static SqlCommand PrepCommand(this SqlConnection conn, string sp, ParameterHelper ph)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sp;
            cmd.Parameters.AddRange(ph);
            cmd.CommandType = CommandType.StoredProcedure;
            return cmd;
        }
        private static SqlCommand PrepCommand(this SqlConnection conn, string sql)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            return cmd;
        }

        private static CommandDefinition GetDef<T>(IDbCall<T> call) =>
            new CommandDefinition(call.Procedure,
                call.ParameterHelper.ToDict(),
                commandType: CommandType.StoredProcedure);
        private static CommandDefinition GetDef<T>(string sql) => new CommandDefinition(sql,commandType:CommandType.Text);
        private static CommandDefinition GetDef(string sql) => new CommandDefinition(sql, commandType: CommandType.Text);

        public static string ExecuteGetXml(this SqlConnection conn, IDbCall<string> q) =>
            conn.ExecuteGetXml(q.Procedure, q.ParameterHelper, q.NullValue);

        public static string ExecuteGetJson(this SqlConnection conn, IDbCall<string> q) =>
            conn.ExecuteGetJson(q.Procedure, q.ParameterHelper, q.NullValue);

        public static T ExecuteScalar<T>(this SqlConnection conn, IDbCall<T> q) =>
            conn.ExecuteScalar<T>(q.Procedure, q.ParameterHelper);
        public static IEnumerable<T> QueryXml<T>(this SqlConnection conn, IDbCall<IEnumerable<T>> call)
        {
            var xml = conn.ExecuteGetXml(call.Procedure, call.ParameterHelper, null);
            if (null == xml) return null;
            return JsonConvert.DeserializeObject<IEnumerable<T>>(xml.ToJson());
        }

        public static T QueryXmlOne<T>(this SqlConnection conn, IDbCall<T> call) where T : class
        {
            var xml = conn.ExecuteGetXml(call.Procedure, call.ParameterHelper, null);
            if (null == xml) return null;
            string json = xml.ToJson();

            return call.ReturnsEnumerable
                ? JsonConvert.DeserializeObject<IEnumerable<T>>(json).FirstOrDefault()
                : JsonConvert.DeserializeObject<T>(json);
        }

        public static List<T> Query<T>(this SqlConnection conn, IDbCall<IEnumerable<T>> call)
        {
            return conn.Query<T>(GetDef(call)).ToList();
        }
        public static List<T> Query<T>(this SqlConnection conn, IJsonDbCall<List<T>> call)
        => JsonConvert.DeserializeObject<List<T>>(conn.ExecuteGetJson(call));
        public static List<T> QueryDirect<T>(this SqlConnection conn, string q)
        {
            return conn.Query<T>(GetDef<T>(q)).ToList();
        }
        public static List<IDictionary<string, object>> QueryDirect(this SqlConnection conn, string q)
        {
            var lst = new List<IDictionary<string, object>>();
            conn.Query(q).ForEach(r => lst.Add((IDictionary<string,object>)r));
            return lst;
        }


        public static string ExecuteScalar(this SqlConnection conn, IDbCall<string> call) =>
            conn._ExecuteScalar(call)?.ToString() ?? call.NullValue;

        private static object _ExecuteScalar(this SqlConnection conn, IDbCall call)
        {
            using (var cmd = PrepCommand(conn, call.Procedure, call.ParameterHelper))
            {
                return cmd.ExecuteScalar();
            }
        }

        public static T ExecuteScalar<T>(this SqlConnection conn, IValDbCall<T> call) where T : struct, IConvertible
        {
            return conn._ExecuteScalar(call).Convert<T>();
        }
        
        public static SqlCommand ExecuteNonQuery(this SqlConnection conn, IDbCall call) =>
            conn.ExecuteNonQuery(call.Procedure, call.ParameterHelper);

        public static SqlCommand ExecuteNonQuery(this SqlConnection conn, string sql) =>
            PrepCommand(conn, sql).ExecNonQuery();

        public static void ExecuteNonQuery(this SqlConnection conn, IEnumerable<string> statements)
        {
            foreach (var statement in statements)
            {
                conn.ExecuteNonQuery(statement);
            }
        }
            
        public static SqlCommand ExecuteNonQuery(this SqlConnection conn, string sp, ParameterHelper ph) =>
            PrepCommand(conn, sp, ph).ExecNonQuery();

        private static SqlCommand ExecNonQuery(this SqlCommand cmd)
        {
            cmd.ExecuteNonQuery();
            return cmd;
        }

        public static SqlConnection Opened(this SqlConnection conn)
        {
            conn.Open();
            return conn;
        }
        public static void ExecuteReader(this SqlConnection conn, string sql, Action<SqlDataReader> rowHandler)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        rowHandler(rdr);
            }
        }

        public static void ExecuteScript(this SqlConnection conn, string script)
        {
            foreach (var s in Regex.Split(script.ToString(), Environment.NewLine + "GO"))
            {
                try
                {
                    if (s.IsNullOrWhitespace()) continue;
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = s;
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Caught {ex.GetType().Name} while executing:");
                    Console.WriteLine(s);
                    throw;
                }
            }
        }
    }
}
