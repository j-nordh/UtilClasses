using SupplyChain.Dto;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.CodeGen
{
    public class TypeLoader
    {
        private readonly TextFileMap _map;

        private readonly List<string> _libPaths = new();
        public TypeLoader(TextFileMap map)
        {
            _map = map;
            InitLibPaths();
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ReflectionAssemblyResolve;
        }

        private void InitLibPaths()
        {
            Directory.EnumerateDirectories(@"c:\Program Files\dotnet\sdk")
                .Where(p => Regex.IsMatch(p, @"\d\.\d"))
                .Select(p => new DirectoryInfo(Path.Combine(p, "Microsoft", "Microsoft.NET.Build.Extensions")))
                .Last(d => d.Exists)
                .EnumerateDirectories()
                .Where(d => d.EnumerateFiles("netstandard.dll", SearchOption.AllDirectories).Any())
                .Reverse()
                .Select(d => d.FullName)
                .Into(_libPaths);


            Directory.EnumerateDirectories(@"c:\Program Files\Microsoft Visual Studio\")

                .Where(p => Path.GetFileName(p).All(char.IsDigit))
                .Select(p => new DirectoryInfo(Path.Combine(p, "Professional", "MSBuild", "Microsoft",
                    "Microsoft.NET.Build.Extensions")))
                .Last(d => d.Exists)
                .EnumerateDirectories()
                .Where(d => Regex.IsMatch(d.Name, @"net\d+"))
                .Select(d => Path.Combine(d.FullName, "lib"))
                .Where(Directory.Exists)
                .Reverse()
                .Into(_libPaths);
        }

        private Assembly ReflectionAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var ass = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.ReflectionOnly && a.FullName.Equals(args.Name));
            if (ass != null) return ass;

            var name = new AssemblyName(args.Name).Name + ".dll";
            var path = _map.All.Select(nsi => nsi.Dll).NotNull()
                .Where(p => p.EndsWithOic(name))
                .Select(_map.GetAbsolutePath)
                .Where(File.Exists)
                .FirstOrDefault();

            return path != null
                ? Assembly.ReflectionOnlyLoadFrom(path)
                : _map.All
                    .Select(nsi => Path.GetDirectoryName(nsi.Dll))
                    .Union(_libPaths)
                    .Distinct()
                    .NotNull()
                    .Select(p => Path.Combine(_map.GetAbsolutePath(p), name))
                    .ForEach(s => Debug.WriteLine(s))
                    .Where(File.Exists)
                    .ForEach(s => Debug.WriteLine($"Found {s}"))
                    .Select(Assembly.ReflectionOnlyLoadFrom)
                    .FirstOrDefault();
        }

        public Assembly GetAssemblyFromTypeName(string typeName) => GetAssembly(_map.Get(typeName).Dll);

        public Assembly GetAssembly(string path)
        {
            if (path.IsNullOrEmpty())
                throw new ArgumentException("Not a valid path, it does not contain any letters");
            path = _map.GetAbsolutePath(path);
            if (!File.Exists(path))
                throw new ArgumentException("That file does not exist.");

            var name = Path.GetFileNameWithoutExtension(path);

            var ass = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies()
                .FirstOrDefault(a => a.GetName().Name.EqualsIc2(name));
            ass ??= Assembly.ReflectionOnlyLoadFrom(path);
            return ass;
        }
        public Type Get(string name)
        {
            var ass = GetAssemblyFromTypeName(name);
            var ret = ass.GetType(name);
            if (null != ret) return ret;
            var dts = ass.GetTypes();
            return dts.First(ti =>
                ti.Name.EqualsOic(name)
                || ti.Name.EndsWith($".{name}")
                );
        }
    }
}
