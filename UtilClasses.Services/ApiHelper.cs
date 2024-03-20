using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using UtilClasses.Interfaces;
using UtilClasses.Json;

namespace UtilClasses.Services;

[PublicAPI]
public class ApiHelper
{
    public WebApplication? App { get; private set; }
    public string Name { get; set; }
    public int Port { get; set; }

    private Action<IServiceCollection>? _configureServices;
    private Func<IServiceCollection, Task>? _configureServicesAsync;
    private Func<WebApplication, Task>? _onStartingAsync;
    private Action<WebApplication>? _onStarting;
    private WebApplicationBuilder? _builder;

    public ApiHelper(string name)
    {
        Name = name;
    }

    public T? GetAppSetting<T>(string name)
    {
        if (null == _builder)
            throw new NullReferenceException();
        return _builder.Configuration.GetValue<T>(name);
    }

    public void OnConfigureServices(Action<IServiceCollection> a) => _configureServices = a;
    public void OnConfigureServices(Func<IServiceCollection, Task> f) => _configureServicesAsync = f;
    public void OnStarting(Action<WebApplication> a) => _onStarting = a;
    public void OnStarting(Func<WebApplication, Task> f) => _onStartingAsync = f;


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

    public async Task Start(string[] args, int port)
    {
        _builder = WebApplication.CreateBuilder(args);
        Port = port;

        Helper.SetupDefaultConfig(_builder);
        Helper.SetupLogging(_builder, "");
        LoadStuff(_builder);

        Log.Logger.Information("---------------------------------------------");
        Log.Logger.Information($"Constructing and initializing {Name}");

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;

        _builder.WebHost
            .UseUrls()
            .UseKestrel(kso =>
            {
                //kso.ListenAnyIP(_addressInfo.Port, opt => opt.UseHttps(StoreName.My, "localhost"));
                //kso.ConfigureHttpsDefaults(o => { o.SslProtocols = SslProtocols.Tls13;});
                kso.ListenAnyIP(Port);
            });
        Log.Logger.Information($"Listening to port: {Port}");
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
        App.MapGet("/",
            async context =>
            {
                await context.Response.WriteAsync(
                    "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
            });
        App.UseExceptionHandler("/error");
        App.MapControllerRoute(name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        App.UseSwagger();
        App.UseSwaggerUI(opt =>
            opt.EnableTryItOutByDefault());


        Log.Logger.Information("Application configured successfully");

        Log.Logger.Information("---------------------------------------------");
        Log.Logger.Information("Application Starting");
        await RunOnStarting();
        Log.Logger.Information("---------------------------------------------");
        await App.RunAsync();
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