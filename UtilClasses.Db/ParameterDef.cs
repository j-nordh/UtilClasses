using System.Data;

namespace UtilClasses.Db
{
    public class ParameterDef
    {
        public ParameterDef(string name, DbType dbType, int length)
        {
            Name = name;
            DbType = dbType;
            Length = length;
        }

        public string Name { get; }
        public DbType DbType { get; }
        public int Length { get; }

    }
}
