using System.Data;

namespace UtilClasses.Core.Extensions.IDataReaders;

public static class DataReaderExtensions
{
    public static int GetInt(this IDataReader rdr, string name)
    {
        return int.Parse(rdr[name].ToString());
    }

    public static string GetString(this IDataReader rdr, string name)
    {
        return rdr[name].ToString();
    }
}