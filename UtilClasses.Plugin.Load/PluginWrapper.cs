using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UtilClasses.Plugins.Load
{
    [Serializable]
    public abstract class PluginWrapper<T, TLoader>:IDisposable where T:class where TLoader : Loader<T>
    {

        private readonly string _path;
        private AppDomain _domain;
        protected Sponsor<TLoader> _sponsor;

        private readonly MarshaledEventPropagator<Exception> _exceptionPropagator;
        private readonly MarshaledEventPropagator<string> _logPropagator;
        protected bool _initiated;
        public event Action<string, string> Log;
        public event Action<string, Exception> ExceptionCaught;
        public string Name { get; protected set; }

        protected PluginWrapper(string path)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
                throw new ArgumentException("The supplied path is neither a directory nor an existing file.");
            
            _path = path;
            _logPropagator = new MarshaledEventPropagator<string>();
            _logPropagator.Event += (s, msg) => Log?.Invoke(s, msg);
            _exceptionPropagator = new MarshaledEventPropagator<Exception>();
            _exceptionPropagator.Event += (s, msg) => ExceptionCaught?.Invoke(s, msg);
        }

        public virtual void Init()
        {
            if (_initiated) return;
            var dir = Directory.Exists(_path) ? _path : Path.GetDirectoryName(_path);
            Name = Path.GetFileNameWithoutExtension(_path);
            var setup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(GetType().Assembly.Location),
                PrivateBinPath = dir,
                DisallowApplicationBaseProbing = false,
                DisallowBindingRedirects = false
            };

            _domain = AppDomain.CreateDomain($"PluginDomain {Name}", null, setup);

            _sponsor = new Sponsor<TLoader>(_domain.CreateInstanceAndUnwrap<TLoader>())
            {
                Instance =
                {
                    Exception = _exceptionPropagator,
                    Log = _logPropagator
                }
            };
            _sponsor.Instance.Load(_path);
            _initiated = true;
        }



        public void Start() => _sponsor.Instance.Start();

        public void Stop() => _sponsor.Instance.Stop();

        public bool Load()
        {
            try
            {
                Init();
            }
            catch (Exception e)
            {
                ExceptionCaught?.Invoke(Name, e);
                return false;
            }
            return true;
        }

        public virtual void Dispose()
        {
            _sponsor.Dispose();
            AppDomain.Unload(_domain);
        }

        
    }

    public class AssemblyResolver : MarshalByRefObject
    {
        private readonly MarshaledEventPropagator<string> _logPropagator;

        public AssemblyResolver(MarshaledEventPropagator<string> logPropagator)
        {
            _logPropagator = logPropagator;
        }

        public Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var domain = sender as AppDomain;
            if (null == domain) throw new ArgumentNullException(nameof(domain), "The sender must be an application domain...");
            var ass = domain.GetAssemblies().FirstOrDefault(a => a.GetName().Equals(new AssemblyName(args.Name)));
            if (null != ass)
            {
                _logPropagator?.RaiseEvent("Resolver", "Assembly already loaded!");
                return ass;
            }
            var path = GetPath(domain, args.Name, "Resolver");
            return Assembly.Load(File.ReadAllBytes(path));
            
        }

        public  void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            _logPropagator?.RaiseEvent("Resolver", $"Loaded {args.LoadedAssembly.GetName().Name}.");
        }

        public Assembly ReflectionOnlyResolveAssembly(object sender, ResolveEventArgs args)
        {
            string preamble =
                $"{args.RequestingAssembly.GetName().Name} requested reflection only assembly {args.Name}";
            var ass =AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies()
                .FirstOrDefault(a => a.GetName().Name.Equals(new AssemblyName(args.Name).Name));
            if (ass != null)
            {
                _logPropagator?.RaiseEvent("Resolver", $"{preamble} that was found in the cache.");
                return ass;
            }
            var path = GetPath(sender as AppDomain, args.Name, "ReflectionResolver");
            if (null == path)
            {
                _logPropagator?.RaiseEvent("Resolver", $"{preamble} but it was not found in the plugin dir or its parents. Trying to load from GAC instead.");
                return Assembly.ReflectionOnlyLoad(args.Name);
            }
            _logPropagator?.RaiseEvent("Resolver", $"{preamble} that is loaded from {path}");
            return Assembly.ReflectionOnlyLoad(File.ReadAllBytes(path));
        }

        public Assembly ResolveType(object sender, ResolveEventArgs args)
        {
            _logPropagator.RaiseEvent("TypeResolver", $"Trying to resolve {args.Name}");
            var domain = sender as AppDomain;
            if (null == domain) throw new ArgumentNullException(nameof(domain), "The sender must be an application domain...");                             
            foreach (var p in Directory.EnumerateFiles(domain.SetupInformation.PrivateBinPath, "*.dll"))
            {
                var ass = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(p));
                var type = ass.ExportedTypes.FirstOrDefault(t => t.Name.Equals(args.Name));
                if(null==type)continue;
                _logPropagator?.RaiseEvent("TypeResolver", $"Found it! {p}");
                return domain.Load(File.ReadAllBytes(p));
            }
            return null;
        }

        private string GetPath(AppDomain domain, string assemblyName, string logName)
        {
            var name = new AssemblyName(assemblyName).Name;
            _logPropagator?.RaiseEvent(logName, $"Trying to resolve {name}");
            if (null == domain) throw new ArgumentNullException(nameof(domain), "The sender must be an application domain...");
            
            var dir = domain.SetupInformation.PrivateBinPath;
            if(null==dir)
                throw new ArgumentNullException("domain.SetupInformation.PrivateBinPath");
            while ((dir?.Length ?? 0) > 3)
            {
                var p = Path.Combine(dir, $"{name}.dll");

                if (File.Exists(p))
                {
                    _logPropagator?.RaiseEvent(logName, $"Found it! {p}");
                    return p;
                }
                dir = Path.GetDirectoryName(dir);
            }
            return null;
        }

        public void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var e = args.ExceptionObject as Exception;
            _logPropagator?.RaiseEvent("Resolver",$"Unhandled {e?.GetType().Name}: {e?.Message}" );
        }

        public void FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            try
            {
                _logPropagator?.RaiseEvent("Resolver", $"First chance {e.Exception.GetType().Name}: {e.Exception.Message}");
            }
            catch (Exception ex)
            {
                //Exception while handling exception!
                var debug = ex.Message;
            }
            
        }
    }

    public static class AppDomainExtensions
    {
        public static Assembly LoadFromType<T>(this AppDomain domain) =>
            domain.Load(File.ReadAllBytes(typeof(T).Assembly.Location));

        public static Assembly LoadFromType(this AppDomain domain, Type t) =>
            domain.Load(File.ReadAllBytes(t.Assembly.Location));

        public static T CreateInstanceAndUnwrap<T>(this AppDomain domain) =>
            (T) domain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);

        public static T CreateInstanceFromAndUnwrap<T>(this AppDomain domain) =>
            (T) domain.CreateInstanceFromAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
    }
}

