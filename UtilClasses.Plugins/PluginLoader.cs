using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UtilClasses.Extensions.Strings;
using UtilClasses.Plugins.Load;

namespace UtilClasses.Plugins
{
    public class PluginLoader<T, TWrapper, TLoader> where T : class where TWrapper : PluginWrapper<T, TLoader> where TLoader : Loader<T>
    {
        private readonly string _dir;
        private readonly Func<string, TWrapper> _wrapperCreator;
        private readonly bool _searchSubDirs;

        public static T LoadSingle(string filename)
        {
            if (filename.Contains(".."))
                throw new ArgumentException("plugin filename contains \"..\", which is illegal.");
            filename = Path.Combine("Plugins", filename);
            if (!File.Exists(filename))
                throw new ArgumentException($"Could not find the specified plugin: {Path.GetFullPath(filename)}");
            var ass = Assembly.Load(File.ReadAllBytes(filename));
            var provider =
                ass.GetExportedTypes()
                    .Where(type => typeof(T).IsAssignableFrom(type))
                    .Select(Activator.CreateInstance)
                    .Cast<T>()
                    .FirstOrDefault();
            if (null == provider)
                throw new ArgumentException(
                    $"The specified plugin does not contain an {typeof(T).Name} with a parameterless constructor.");
            return provider;
        }


        public PluginLoader(string dir, Func<string, TWrapper> wrapperCreator, bool searchSubDirs=true)
        {
            _dir = dir;
            _wrapperCreator = wrapperCreator;
            _searchSubDirs = searchSubDirs;
        }

        public event Action<string, TWrapper> Loaded;
        public event Action<Assembly, Exception> Exception;
        public event Action<Assembly, string> Log;
        public void Load()
        {
            if (!_searchSubDirs)
            {
                LoadSpecific(_dir);
                return;
            }
            var dirInfo = new DirectoryInfo(_dir);
            foreach (var dir in dirInfo.EnumerateDirectories())
            {
                LoadSpecific(dir.FullName);
            }
        }

        private void LoadSpecific(string dir)
        {
            var dirInfo = new DirectoryInfo(dir);
            var assemblies = dirInfo.EnumerateFiles()
                .Where(f => f.Name.EndsWithIc2("dll") || f.Name.EndsWithIc2("exe"))
                .Select(f => Assembly.ReflectionOnlyLoadFrom(f.FullName));

            foreach (var ass in assemblies)
            {
                var factoryType = ass.ExportedTypes.FirstOrDefault(t => !t.IsAbstract && typeof(T).IsAssignableFrom(t));
                if (factoryType == null)
                    continue;
                Log?.Invoke(ass, $"Loading {ass.GetName().Name}...");
                try
                {
                    var wrapper = _wrapperCreator(ass.Location);
                    wrapper.Start();
                    Loaded?.Invoke(ass.GetName().Name, wrapper);
                }
                catch (Exception e)
                {
                    Exception?.Invoke(ass, e);
                }
            }
        }
    }
}
