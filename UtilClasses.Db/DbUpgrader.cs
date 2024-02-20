using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Db
{
    public class DbUpgrader:IDisposable
    {
        private readonly DirectoryInfo _versionDir;
        private readonly SqlConnection _conn;

        public event Action<string, int> DirectoryCompleted; 
        public DbUpgrader(string connectionString, string path)
        {
            _versionDir = new DirectoryInfo(path);
            _conn = new SqlConnection(connectionString);
            _conn.Open();
        }
        public int Upgrade(string db)
        {
            db = db ?? "";
            var parentDir = new DirectoryInfo(Path.Combine(_versionDir.ToString(), db));
            int count = 0;
            foreach (var dir in parentDir.EnumerateDirectories().Where(d => d.EnumerateFiles().Any()))
            {
                int dirCount = 0;
                foreach (var file in dir.EnumerateFiles().Where(fi => !fi.Name.ToLower().Contains("runcheck")))
                {
                    try
                    {
                        if (!CheckConditions(file, _conn)) continue;
                        var runcheckFileName = file.FullName.Replace(".sql", "_RunCheck.sql");
                        if (File.Exists(runcheckFileName))
                        {
                            var strRunCheck = $"{GetSanitizedContent(new FileInfo(runcheckFileName))} Select 1 Else Select 0";
                            var cmd = _conn.CreateCommand();
                            cmd.CommandText = strRunCheck;
                            cmd.CommandTimeout = 0;
                            if (int.Parse(cmd.ExecuteScalar().ToString()) != 1)
                            {
                                continue;
                            }
                        }

                        var content = GetSanitizedContent(file);
                        foreach (var s in Regex.Split(content, Environment.NewLine + "GO"))
                        {
                            if(s.IsNullOrWhitespace()) continue;
                            var cmd = _conn.CreateCommand();
                            cmd.CommandText = s;
                            cmd.CommandTimeout = 0;
                            cmd.ExecuteNonQuery();
                            dirCount+=1;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Could not apply DB patch: {dir.Name}\\{file.Name} Exception: {ex.Message}");
                    }
                }
                DirectoryCompleted?.Invoke(dir.Name, dirCount);
                count += dirCount;
            }
            return count;
        }
        private static bool CheckConditions(FileInfo file, SqlConnection conn)
        {
            if (file.Name.Contains("RunCheck.sql"))
                return false;

            string runCheckName = file.Name.Replace(".sql", "_RunCheck.sql");
            foreach (var fi in file.Directory?.EnumerateFiles()??new FileInfo[0])
            {
                if (string.Compare(fi.Name, runCheckName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = string.Concat(GetSanitizedContent(fi), " Select 1 Else Select 0");
                    int result;
                    if (!int.TryParse(cmd.ExecuteScalar().ToString(), out result))
                        return false;

                    return 1 == result;
                }
            }
            return true;
        }
        private static string GetSanitizedContent(FileInfo file)
        {
            var content = new StringBuilder();
            var reader = new StreamReader(file.OpenRead());
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line!= null && line.Contains("GO"))
                {
                    var parts = Regex.Split(line, "GO");
                    bool foundChars = false;
                    foreach (var part in parts)
                    {
                        if (!string.IsNullOrWhiteSpace(part)) foundChars = true;
                    }
                    content.AppendLine(foundChars ? line : "GO");
                }
                else
                {
                    content.AppendLine(line);
                }
            }
            reader.Close();
            return content.ToString();
        }

        public void Dispose()
        {
            _conn?.Close();
            _conn?.Dispose();
        }
    }
}
