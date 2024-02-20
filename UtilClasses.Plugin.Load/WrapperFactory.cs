using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UtilClasses.Plugins.Load
{
    public class WrapperFactory
    {
        public event Action<string, string> LogMessage;
        public event Action<string, Exception> ExceptionCaught;
        
        public List<TWrapper> LoadAll<T, TLoader, TWrapper>(string pluginPath, Func<string, TWrapper> creator
        ) where TWrapper : PluginWrapper<T, TLoader>
            where T : class
            where TLoader : Loader<T>
        {
            int count = 0;
            var wrappers = new List<TWrapper>();
            foreach (var dir in Directory.EnumerateDirectories(pluginPath))
            {
                LogMessage?.Invoke("WrapperFactory", $"Investigating {dir.Substring(dir.LastIndexOf('\\'))}");
                if (Directory.EnumerateFiles(dir).Select(Path.GetFileName)
                    .Any(name => name.Equals("ignore", StringComparison.OrdinalIgnoreCase)))
                {
                    LogMessage?.Invoke("WrapperFactory", "Found ignore file, directory will be ignored.");
                    continue;
                }
                var path = Path.Combine(pluginPath, dir);
                var wrapper = creator(path);
                wrapper.ExceptionCaught+=(s,e)=>ExceptionCaught?.Invoke(s,e);
                wrapper.Log+=(s,msg)=>LogMessage?.Invoke(s,msg);
                if (!wrapper.Load()) continue;
                wrappers.Add(wrapper);
                count += 1;
            }

            LogMessage?.Invoke("WrapperFactory", $"Loaded {count} plugins.");
            return wrappers;
        }
    }
}