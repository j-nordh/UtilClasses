using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
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

        public static PathUtil FromAssembly(Assembly ass)
            => new PathUtil(Path.GetDirectoryName(ass.Location));
        public static PathUtil FromType<T>(params string[] subDirParts)
        {
            var path = Path.GetDirectoryName(typeof(T).Assembly.Location);
            if (subDirParts.Any())
                path = StaticPathUtil.Combine(path, subDirParts);
            return new PathUtil(path);
        }

        public string GetRelativePath(string path) => StaticPathUtil.GetRelativePath(path, _basePath);
        public string GetAbsolutePath(string path) => StaticPathUtil.GetAbsolutePath(path, _basePath);

        public static string GetRelativePath(string fullPath, string basePath)
            => StaticPathUtil.GetRelativePath(fullPath, basePath);

        public static string GetAbsolutePath(string relPath, string basePath) =>
            StaticPathUtil.GetAbsolutePath(relPath, basePath);

        public  string Combine(string first, params string[] parts) => StaticPathUtil.Combine(new[] { _basePath, first }.Union(parts).ToArray());
        public string Combine(params string[] parts) => StaticPathUtil.Combine(new[] { _basePath }.Union(parts).ToArray());

    }

    public static class StaticPathUtil
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
        public static string Combine(string first, params string[] parts) => Combine(new[] { first }.Union(parts).ToArray());
        public static string Combine(params string[] parts)
        {
            if (!parts.Any())
                return "";
            var path = parts.First();
            foreach (var part in parts.Skip(1))
            {
                path = Path.Combine(path, part);
            }
            return path;
        }
    }
}