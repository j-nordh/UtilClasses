using System;
using System.Data;
using UtilClasses.Core.Extensions.Decimals;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Db.Extensions
{
    public static class ParameterDefExtensions
    {
        public static ParameterHelper.Parameter In(this ParameterDef def, object val) =>
            new ParameterHelper.Parameter()
            {
                Name = def.Name,
                Value = val,
                T = def.DbType,
                Dir = ParameterDirection.Input,
                Size = def.Length
            }.FixBool();

        public static ParameterHelper.Parameter In(this ParameterDef def, long val) => def.In((object)val);

        public static ParameterHelper.Parameter In(this ParameterDef def, decimal val) =>
            new ParameterHelper.Parameter()
            {
                Name = def.Name,
                Value = val,
                T = DbType.Decimal,
                Dir = ParameterDirection.Input,
                Size = def.Length,
                Scale = 6,
                Precision= 18
            }.FixBool();



        public static ParameterHelper.Parameter Out(this ParameterDef def, object value = null)
        {
            return new ParameterHelper.Parameter() { Name = def.Name, Value = value, T = def.DbType, Dir = ParameterDirection.Output, Size = def.Length };
        }

        public static ParameterHelper.Parameter FixBool(this ParameterHelper.Parameter p)
        {
            if (p.T == DbType.Boolean && p.Value != null && p.Value != DBNull.Value)
                p.Value = p.Value.ToString().AsBoolean();
            return p;
        }
    }
}
