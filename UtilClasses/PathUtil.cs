using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Extensions.Strings;
using UtilClasses.Files;

namespace UtilClasses
{
    public class PathUtil
    {
        private readonly string _basePath;

        public PathUtil(string basePath)
        {
            _basePath = basePath;
        }

        public PathUtil(Assembly ass) : this(ass.Location)
        {
        }

        public string GetRelativePath(string path) => PathUtil2.GetAbsolutePath(path, _basePath);
        public string GetAbsolutePath(string path) => PathUtil2.GetAbsolutePath(path, _basePath);

        public static string GetRelativePath(string fullPath, string basePath)
            => PathUtil2.GetRelativePath(fullPath, basePath);

        public static string GetAbsolutePath(string relPath, string basePath) =>
            PathUtil2.GetAbsolutePath(relPath, basePath);
    }

    public static class PathUtil2
    {
        public static string GetRelativePath(string fullPath, string? basePath = null)
        {
            if (!Path.IsPathRooted(fullPath)) return fullPath;
            basePath = GetBasePath(basePath);

            if (!basePath.EndsWith("\\"))
                basePath += "\\";

            var baseUri = new Uri(basePath);
            var fullUri = new Uri(fullPath);

            var relativeUri = baseUri.MakeRelativeUri(fullUri);

            // Uri's use forward slashes so convert to the preferred separator.
            return relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar);
        }

        public static string GetAbsolutePath(string relPath, string? basePath = null)
        {
            if (Path.IsPathRooted(relPath))
                return relPath;

            var newPath = Path
                .GetFullPath(new Uri(Path.Combine(GetBasePath(basePath), relPath)).LocalPath);
            return newPath.Replace('/', Path.PathSeparator);
        }

        private static string GetBasePath(string? basePath)
        {
            basePath ??= Environment.CurrentDirectory;
            
            if (File.Exists(basePath))
                basePath = Path.GetDirectoryName(basePath);
            if (null == basePath)
                throw new NullReferenceException("Something is VERY wrong with that base path...");

            if (!Path.IsPathRooted(basePath))
                throw new NullReferenceException(
                    "The supplied base path is not rooted, meaning it cannot be used...");
            return basePath;
        }
    }
}