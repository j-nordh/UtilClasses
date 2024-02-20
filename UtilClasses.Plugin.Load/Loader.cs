using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Lifetime;
using System.Threading.Tasks;
using UtilClasses.Extensions.Strings;
using UtilClasses.Extensions.Types;

namespace UtilClasses.Plugins.Load
{
    public abstract class Loader<T> : MarshalByRefObject, IDisposable where T:class
    {
            
        protected T Instance { get; private set; }
        public MarshaledEventPropagator<Exception> Exception;
        public MarshaledEventPropagator<string> Log;

        public bool Load(string path)
        {
            var assemblies = new List<AssemblyWithPath>();
            var resolver = new AssemblyResolver(Log);
            AppDomain.CurrentDomain.AssemblyLoad 
                += resolver.OnAssemblyLoad;
            AppDomain.CurrentDomain.AssemblyResolve 
                += resolver.ResolveAssembly;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve 
                += resolver.ReflectionOnlyResolveAssembly;
            AppDomain.CurrentDomain.TypeResolve 
                += resolver.ResolveType;
            AppDomain.CurrentDomain.UnhandledException 
                += resolver.OnUnhandledException;
            
            var name = Path.GetFileNameWithoutExtension(path);
            if (File.Exists(path))
            {
                assemblies.Add(new AssemblyWithPath(path));
                Log?.RaiseEvent(name, $"Trying to load a specific plugin from: {path}");
            }
            else if (Directory.Exists(path))
            {
                Log?.RaiseEvent(name, $"Trying to load plugins from the directory: {path}");
                var dirInfo = new DirectoryInfo(path);
                assemblies.AddRange(dirInfo.EnumerateFiles()
                    .Where(f =>f.Name.EndsWithAnyOic("dll", "exe"))
                    .Select(f =>
                        new AssemblyWithPath(f.FullName))
                    .ToList());
            }
            else
                throw new ArgumentException(@"The supplied path is neither a file nor a directory", path);


        foreach (var awp in assemblies)
        {
            var type = awp.Assembly
                .ExportedTypes
                .RequireNotAbstract()
                .RequireInterface<T>()
                .FirstOrDefault();

            if (type == null) continue;
            Log?.RaiseEvent(name, 
                $"Loading plugin: {awp.Name}...");
            var realAss =AppDomain.CurrentDomain
                .Load(File.ReadAllBytes(awp.Path));
            type = realAss.ExportedTypes
                .FirstOrDefault_MatchName(type);
            if (type == null)
            {
                Exception?.RaiseEvent(name, new Exception("Failed to load the indentified factory type."));
                continue;
            }
            try
            {
                Instance = Activator.CreateInstance(type) as T;
                return true;
            }
            catch (Exception e)
            {
                Exception?.RaiseEvent(name, e);
                return false;
            }
        }
            return false;
        }

        private class AssemblyWithPath
        {
            public AssemblyWithPath(string path, bool reflectionOnly=true)
            {
                Assembly = reflectionOnly
                    ? Assembly.ReflectionOnlyLoad(File.ReadAllBytes(path))
                    : Assembly.Load(File.ReadAllBytes(path));
                Path = path;
            }
            public string Path { get; }
            public Assembly Assembly { get; }

            public string Name => Assembly.GetName().Name;
        }
      
        public abstract Task Start();
        public abstract Task Stop();

        public virtual void Dispose()
        {
            Stop();
            (Instance as IDisposable)?.Dispose();
        }

        
        public override Object InitializeLifetimeService() => null;// returning null sets the lifetime to infinite
    }
}