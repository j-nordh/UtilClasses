using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using UtilClasses.Extensions.StringBuilders;

namespace UtilClasses.Db
{
    public class ParameterHelper
    {
        protected readonly List<Parameter> _ps;

        public ParameterHelper()
        {
            _ps = new List<Parameter>();
        }

        public struct Parameter
        {
            public string Name;
            public object Value;
            public DbType T;
            public int Size;
            public byte Precision;
            public byte Scale;
            public ParameterDirection Dir;

            public override string ToString()
            {
                return $"{Name}={Value} Type:{T} Size:{Size} Dir{Dir}";
            }

            public int RealSize()
            {
                if (T != DbType.String || Size != -1) return Size;
                return Value.ToString().Length;
            }

            public static implicit operator SqlParameter(Parameter p)
            {
                var ret = new SqlParameter()
                {
                    ParameterName = p.Name,
                    DbType = p.T,
                    Size = p.Size,
                    Direction = p.Dir,
                    Value = p.Value
                };
                if (p.T == DbType.String && p.Size == -1) ret.Size = int.MaxValue;
                if (p.T != DbType.Decimal) return ret;
                ret.Scale = 4;
                ret.Precision = 18;
                return ret;
            }
        }

        public void AddIn(string name, object value, DbType t, int size)
        {
            _ps.Add(new Parameter() { Name = name, Value = value, T = t, Dir = ParameterDirection.Input, Size = size });
        }

        public void AddOut(string name, object value, DbType t, int size)
        {
            _ps.Add(new Parameter() { Name = name, Value = value, T = t, Dir = ParameterDirection.Output, Size = size });
        }

        public void AddInBool(string name, bool value)
        {
            AddIn(name, value, DbType.Boolean, 1);
        }

        public void Add(Parameter p)
        {
            _ps.Add(p);
        }

        public void AddRange(IEnumerable<Parameter> ps)
        {
            _ps.AddRange(ps);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            _ps.ForEach(p => sb.AppendLine(p));
            return sb.ToString();
        }
        public string AsString => ToString();

        public Dictionary<string, object> ToDict() => this;

        public static implicit operator DynamicParameters(ParameterHelper ph)
        {
            var dp = new DynamicParameters();
            foreach (var p in ph._ps)
            {
                byte? scale = null;
                byte? precision = null;

                if (p.T == DbType.Decimal)
                {
                    precision = 18;
                    scale = 4;
                }

                if (DbType.String == p.T)
                {
                    var str = p.Value.ToString();
                    dp.Add(p.Name, new DbString { Value = str, Length = str.Length, IsAnsi = false, IsFixedLength = false }, DbType.String, p.Dir);
                }
                else
                {
                    dp.Add(p.Name, p.Value, p.T, p.Dir, p.Size, precision, scale);    
                }
                
            }
            return dp;
        }

        public IEnumerable<SqlParameter> ToSqlParameters() => _ps.Select(p => (SqlParameter)p);

        public static implicit operator List<SqlParameter>(ParameterHelper ph) => ph.ToSqlParameters().ToList();

        public static implicit operator SqlParameter[](ParameterHelper ph) => ph.ToSqlParameters().ToArray();

        public static implicit operator Dictionary<string, object>(ParameterHelper ph) =>
            ph._ps.ToDictionary(p => p.Name.TrimStart('@'), p => p.Value);
    }
}
