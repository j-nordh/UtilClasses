using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UtilClasses.Extensions.Streams;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Extensions.Assemblies
{
    public static class AssemblyExtensions
    {
        public static string? GetResourceString(this Assembly ass, string name) => ass.GetResourceStream(name)?.AsString();

        public static Stream? GetResourceStream(this Assembly ass, string name, bool recursive = true)
        {
            if (null == name) return null;

            var names = ass.GetManifestResourceNames().Where(n => n.ContainsOic(name)).ToList();
            
            if (!names.Any()) 
                return File.OpenRead(ass.GetResourcePath(name, recursive));
            
            if (names.Count > 1) throw new DuplicateNameException();
            return ass.GetManifestResourceStream(names.Single());
        }

        public static string GetResourcePath(this Assembly ass, string name, bool recursive = true)
        {
            var dir = new DirectoryInfo(Path.GetDirectoryName(ass.Location)!);
            var files = dir.EnumerateFiles().Where(fi => fi.Name.ContainsOic(name)).ToList();
            if (recursive && !files.Any())
            {
                //Try sub directories
                files = dir.GetFileSystemInfos("*", SearchOption.AllDirectories)
                    .OfType<FileInfo>()
                    .Where(fi => fi.Name.ContainsOic(name)).ToList();
            }
            if (!files.Any()) throw new KeyNotFoundException();
            if (files.Count > 1) throw new DuplicateNameException();
            return files.Single().FullName;
        }

        public static void SaveResource(this Assembly ass, string name, string content, Encoding? enc = null)
        {
            var dir = new DirectoryInfo(Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(ass.CodeBase).Path))!);

            //var workdir = new DirectoryInfo(Environment.CurrentDirectory.Trim('\\'));
            if (!dir.FullName.EndsWithAnyOic("bin\\Debug", "bin\\Release"))
                dir = dir.Parent;

            dir = dir?.Parent?.Parent;
            var files = dir?.EnumerateFiles().Where(fi => fi.Name.ContainsOic(name)).ToList() ?? new List<FileInfo>();
            if (!files.Any()) throw new KeyNotFoundException();
            if (files.Count > 1) throw new DuplicateNameException();

            enc ??= Encoding.UTF8;

            File.WriteAllText(files.Single().FullName, content, enc);

        }

        public static string GetFileVersion(this Assembly assembly)
        {

            var value = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)
                .OfType<AssemblyFileVersionAttribute>()
                .SingleOrDefault();

            return value?.Version ?? "?.?.?.?";
        }

        public static string FormatResourceString(this Assembly ass, string name, params string[] args) => string.Format(ass.GetResourceString(name), args);

        public static bool IsDevDebug(this Assembly ass, out string dir)
        {
#if !DEBUG
            dir = null;
            return false;
#endif
            dir = "";
            if (!Debugger.IsAttached) return false;
            var p = ass.Location;
            var di = new DirectoryInfo(Path.GetDirectoryName(p)!);
            di = di.Parent;
            if (!di.Name.EqualsOic("debug")) return false;
            di = di.Parent;
            if (!di.Name.EqualsOic("bin")) return false;
            di = di.Parent;
            dir = di.FullName;
            return true;
        }
    }


}
