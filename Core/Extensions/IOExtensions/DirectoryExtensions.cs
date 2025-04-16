using System.Collections.Generic;
using System.IO;

namespace UtilClasses.Core.Extensions.IOExtensions;

public static class DirectoryExtensions
{
    public static IEnumerable<FileInfo> FilesMatchingNumber(this DirectoryInfo dir, int number, bool recursive=false, string? prefix=null, string? suffix=null)
    {
        foreach(var f in dir.EnumerateFiles("*.*", recursive?SearchOption.AllDirectories:SearchOption.TopDirectoryOnly))
        {
            if(null!= prefix && !f.Name.StartsWith(prefix)) continue;
            if(null!=suffix && ! f.Name.EndsWith(suffix)) continue;
            var prefixLength = prefix?.Length ?? 0;
            var suffixLength = suffix?.Length ?? 0;
            var section = f.Name.Substring(prefixLength, f.Name.Length - suffixLength);
            if(!int.TryParse(section, out var n)) continue;
            if(n != number) continue;
            yield return f;
        }
    }
}