using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UtilClasses.Core.Files
{
    public static class FsUtil
    {
        public static bool FileExists(params string[] parts) => File.Exists(Path.Combine(parts));
        public static bool DirExists(params string[] parts) => Directory.Exists(Path.Combine(parts));

        public static void EnsureDirExists(string path)
        {
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
        }
    }


}
