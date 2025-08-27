using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UtilClasses.Core;

public class PathUtil
{
    public string BasePath { get; }

    public PathUtil(params string[] basePath)
    {
        BasePath = StaticPathUtil.Combine(basePath);
    }
    public PathUtil(string first, params string[] basePath)
    {
        BasePath = StaticPathUtil.Combine(first, basePath);
    }
    public static PathUtil FromAssembly(Assembly ass, params string[] subDirParts) => new(ass.Location, subDirParts);

    public static PathUtil FromType<T>(params string[] subDirParts) => FromType(typeof(T), subDirParts);

    public static PathUtil FromType(Type t, params string[] subDirParts)
    {
        var path = Path.GetDirectoryName(t.Assembly.Location)!;
        if (subDirParts.Any())
            path = StaticPathUtil.Combine(path, subDirParts);
        return new PathUtil(path);
    }

    public string GetRelativePath(string path) => StaticPathUtil.GetRelativePath(BasePath, path);
    public string GetAbsolutePath(string path) => StaticPathUtil.GetAbsolutePath(BasePath, path);
    public string GetAbsolutePath(params string[] paths) => StaticPathUtil.GetAbsolutePath(BasePath, paths);

    public  string Combine(string first, params string[] parts) => StaticPathUtil.Combine(new[] { BasePath, first }.Union(parts).ToArray());
    public string Combine(params string[] parts) => StaticPathUtil.Combine(new[] { BasePath }.Union(parts).ToArray());

}

public static class StaticPathUtil
{
    public static string GetRelativePath(string basePath, params string[] fullPath)
    {
        basePath = GetBasePath(basePath);
        if (!fullPath.Any())
            return basePath;
        if (!Path.IsPathRooted(fullPath.First())) return Combine(fullPath);
        

        if (!basePath.EndsWith(Path.PathSeparator.ToString()))
            basePath += Path.PathSeparator.ToString();

        var baseUri = new Uri(basePath);
        var fullUri = new Uri(Combine(fullPath));

        var relativeUri = baseUri.MakeRelativeUri(fullUri);

        // Uri's use forward slashes so convert to the preferred separator.
        return relativeUri.ToString().Replace('/', Path.PathSeparator);
    }

    public static string GetAbsolutePath(string basePath, params string[] relPath )
    {
        if(!relPath.Any())
            return basePath;
        
        if (Path.IsPathRooted(relPath.First()))
            return Combine(relPath);

        var newPath = Path
            .GetFullPath(new Uri(Combine(GetBasePath(basePath), Combine(relPath))).LocalPath);
        return newPath.Replace('/', Path.PathSeparator).Replace('\\', Path.PathSeparator);
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