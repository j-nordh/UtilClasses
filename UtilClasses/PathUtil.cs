using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses
{
    public class PathUtil
    {
        private readonly string _basePath;

        public PathUtil(string basePath)
        {
            if (File.Exists(basePath))
                basePath = Path.GetDirectoryName(basePath);
            _basePath = basePath;
        }
        public PathUtil(Assembly ass) :this(ass.Location){}

        public string GetRelativePath(string path) => GetRelativePath(path, _basePath);
        public string GetAbsolutePath(string path) => GetAbsolutePath(path, _basePath);

        public static string GetRelativePath(string fullPath, string basePath)
        {

            //if (!Path.IsPathRooted(basePath)) return basePath;
            if (!Path.IsPathRooted(fullPath)) return fullPath;
            if (!fullPath.Contains("\\")) return fullPath;
            // Require trailing backslash for path
            if (!basePath.EndsWith("\\"))
                basePath += "\\";

            Uri baseUri = new Uri(basePath);
            Uri fullUri = new Uri(fullPath);

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

            // Uri's use forward slashes so convert back to backward slashes
            return relativeUri.ToString().Replace("/", "\\");
        }

        public static string GetAbsolutePath(string relPath, string basePath)
        {
            if (null == relPath) return null;
            if (Path.IsPathRooted(relPath))
                return relPath;
            if (File.Exists(basePath))
                basePath = Path.GetDirectoryName(basePath);

            return Path.GetFullPath(new Uri(Path.Combine(basePath, relPath)).LocalPath);
        }
    }
}
