using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using UtilClasses.Core;
using UtilClasses.Core.Extensions.Enumerables;
using UtilClasses.Core.Extensions.Lists;
using UtilClasses.Interfaces;
using UtilClasses.Json;

namespace UtilClasses.Services;

[PublicAPI]
public class ApiHelper
{
    public WebApplication? App { get; private set; }
    public string Name { get; set; }
    public int Port { get; set; } = -1;

    public bool? RunAsService { get; set; }
    public bool GenerateLandingPage { get; set; } = true;

    private Action<IServiceCollection>? _configureServices;
    private Func<IServiceCollection, Task>? _configureServicesAsync;
    private Func<WebApplication, Task>? _onStartingAsync;
    private Action<FirstChanceExceptionEventArgs>? _onException;
    private Action<WebApplication>? _onStarting;
    private WebApplicationBuilder? _builder;

    private List<Action<LoggerConfiguration>?> _logConfigs = new();

    public ApiHelper(string name)
    {
        Name = name;
    }

    public ApiHelper()
    {
        Name = Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown service";
    }

    public T? GetAppSetting<T>(string name)
    {
        if (null == _builder)
            throw new NullReferenceException();
        return _builder.Configuration.GetValue<T>(name);
    }

    public T? GetAppSettingObject<T>(string name)
    {
        if (null == _builder)
            throw new NullReferenceException();
        var section = _builder.Configuration.GetRequiredSection(name);
        return section.Get<T>();
    }

    public T GetRequiredAppSettingObject<T>(string name)
    {
        var ret = GetAppSettingObject<T>(name);
        if (null == ret)
            throw new KeyNotFoundException(
                $"Could not find a required object matching {name}. Please check the appsettings.json file.");
        return ret;
    }

    public List<T> GetAppSettingList<T>(string name) where T : class
    {
        if (null == _builder)
            throw new NullReferenceException();
        var section = _builder.Configuration.GetSection(name);
        var children = section.GetChildren();
        return children
            .Select(s => s.Get<T>())
            .NotNull()
            .ToList();
    }

    public void OnConfigureServices(Action<IServiceCollection> a) => _configureServices = a;
    public void OnConfigureServices(Action<IServiceCollection, IConfiguration> a) => _configureServices = sc => a(sc, _builder!.Configuration);
    public void OnConfigureServices(Func<IServiceCollection, Task> f) => _configureServicesAsync = f;
    public void OnConfigureServices(Func<IServiceCollection, IConfiguration, Task> f) => _configureServicesAsync = sc => f(sc, _builder!.Configuration);
    public void OnStarting(Action<WebApplication> a) => _onStarting = a;
    public void OnStarting(Func<WebApplication, Task> f) => _onStartingAsync = f;
    public void OnStarting(Action<WebApplication, IConfiguration> a) => _onStarting = app => a(app, _builder!.Configuration);
    public void OnStarting(Func<WebApplication, IConfiguration, Task> f) => _onStartingAsync = app => f(app, _builder!.Configuration);
    public void OnException(Action<FirstChanceExceptionEventArgs> a) => _onException = a;

    public void WithLogFilter<T>() where T : ILogEventFilter, new()
    {
        _logConfigs.Add(cfg => cfg.Filter.With<T>());
    }

    public void AddConsoleLog() => _logConfigs.Add(cfg => cfg.WriteTo.Console());


    protected virtual async Task RunConfigureServices(WebApplicationBuilder builder)
    {
        _configureServices?.Invoke(builder.Services);
        if (null != _configureServicesAsync)
            await _configureServicesAsync.Invoke(builder.Services);
    }

    protected virtual async Task RunOnStarting()
    {
        if (null == App)
            throw new NullReferenceException();
        _onStarting?.Invoke(App);
        if (null != _onStartingAsync)
            await _onStartingAsync.Invoke(App);
    }

    protected virtual void LoadStuff(WebApplicationBuilder builder)
    {
    }

    public async Task Start(string[] args)
    {
        RunAsService ??= !(Debugger.IsAttached || args.Contains("--console"));
        ServiceWrapper? serviceWrapper = null;
        if (RunAsService == true)
        {
            var webApplicationOptions = new WebApplicationOptions()
            {
                Args = args,
                ContentRootPath = AppContext.BaseDirectory,
                ApplicationName = Process.GetCurrentProcess().ProcessName
            };
            serviceWrapper = new();
            _builder = WebApplication.CreateBuilder(webApplicationOptions);
            _builder.Host.UseWindowsService();
            _builder.Services.AddWindowsService()
                .AddHostedService(_ => serviceWrapper!);
        }
        else
        {
            _builder = WebApplication.CreateBuilder(args);
        }

        Port = GetAppSetting<int>("Port");


        Helper.SetupDefaultConfig(_builder);
        Helper.SetupLogging(_builder, "", cfg => _logConfigs.ForEach(a => a?.Invoke(cfg)), false);
        LoadStuff(_builder);

        Log.Logger.Information("---------------------------------------------");
        Log.Logger.Information($"Constructing and initializing {Name}");

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;

        _builder.WebHost
            .UseUrls()
            .UseKestrel(kso => { kso.ListenAnyIP(Port); });

        Log.Logger.Information($"Configured port: {Port}");
        _builder.Services
            .AddRouting()
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.SchemaFilter<EnumSchemaFilter>();
            })
            .AddSwaggerGenNewtonsoftSupport()
            .AddSingleton(_builder.Services)
            .AddControllers()
            .AddApplicationPart(GetType().Assembly);
        Log.Logger.Information("---------------------------------------------");
        Log.Logger.Information("Configuring the application");
        await RunConfigureServices(_builder);

        try
        {
            App = _builder.Build();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        App.UseDefaultFiles();
        //App.UseStaticFiles();
        if (GenerateLandingPage)
            App.MapGet("/",
                async context =>
                {
                    await context.Response.WriteAsync(
                        $"Congratulations, you have reached {Name}. If you're trying to reach the API specify the correct endpoint or try /api/Swagger or /Swagger instead. Have a nice day!");
                });


        App.UseExceptionHandler("/error");
        App.MapControllerRoute(name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        App.UseSwagger();
        App.UseSwaggerUI(opt =>
            opt.EnableTryItOutByDefault());
        if (null != _onException)
        {
            AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
        }


        Log.Logger.Information("Application configured successfully");

        Log.Logger.Information("---------------------------------------------");
        Log.Logger.Information("Application Starting");
        await RunOnStarting();
        Log.Logger.Information("---------------------------------------------");

        await App.RunAsync();
    }

    public class ServiceWrapper : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private volatile bool _insideFirstChanceExceptionHandler;

    private void OnFirstChanceException(object? sender, FirstChanceExceptionEventArgs args)
    {
        if (_insideFirstChanceExceptionHandler)
            return; // Prevent recursion if an exception is thrown inside this method

        _insideFirstChanceExceptionHandler = true;
        try
        {
            _onException?.Invoke(args);
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, "Caught exception in global exception handler");
        }
        finally
        {
            _insideFirstChanceExceptionHandler = false;
        }
    }
}

[PublicAPI]
public class ApiHelper<TCfg> : ApiHelper where TCfg : class, IHasPort, new()
{
    public TCfg Config { get; private set; } = new();

    protected override void LoadStuff(WebApplicationBuilder builder)
    {
        if (!File.Exists(_loadPath))
            throw new Exception($"Could not find config file at {_loadPath}");
        Config = JsonUtil.Load<TCfg>(_loadPath);
        Port = Config.Port;
    }

    public ApiHelper(string name, string loadPath) : base(name)
    {
        _loadPath = loadPath;
    }

    protected override async Task RunConfigureServices(WebApplicationBuilder builder)
    {
        await base.RunConfigureServices(builder);
        builder.Services.AddSingleton(Config);
        _configureServices?.Invoke(builder.Services, Config);
        if (_configureServicesAsync != null)
            await _configureServicesAsync(builder.Services, Config);
    }

    protected override async Task RunOnStarting()
    {
        await base.RunOnStarting();
        if (null == App)
            throw new NullReferenceException();
        _onStarting?.Invoke(App, Config);
        if (_onStartingAsync != null)
            await _onStartingAsync(App, Config);
    }

    private Action<IServiceCollection, TCfg>? _configureServices;
    private Func<IServiceCollection, TCfg, Task>? _configureServicesAsync;
    private Func<WebApplication, TCfg, Task>? _onStartingAsync;
    private Action<WebApplication, TCfg>? _onStarting;
    private readonly string _loadPath;

    public void OnConfigureServices(Action<IServiceCollection, TCfg> a) => _configureServices = a;
    public void OnConfigureServices(Func<IServiceCollection, TCfg, Task> f) => _configureServicesAsync = f;

    public void OnStarting(Action<WebApplication, TCfg> a) => _onStarting = a;
    public void OnStarting(Func<WebApplication, TCfg, Task> f) => _onStartingAsync = f;
}