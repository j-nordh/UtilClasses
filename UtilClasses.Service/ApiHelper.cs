using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Common.Dto;
using MACS.Common;
using MACS.Dto;
using MACS.Dto.Config;
using MACS.SampleHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using UtilClasses.Extensions.Enums;
using UtilClasses.Extensions.Strings;

namespace MACS.Service;

public class ApiHelper
{
    private ServiceRegistry _services;
    private ServiceAddressInfo _addressInfo;
    public WebApplication App { get; private set; }
    public string Name { get; set; }

    private Action<IServiceCollection> _configureServices;
    private Func<IServiceCollection, Task> _configureServicesAsync;
    private Func<WebApplication, Task> _onStartingAsync;
    private Action<WebApplication> _onStarting;
    private Action<ConfigurationManager> _onAppConfiguration;
    private Func<ConfigurationManager, Task> _onAppConfigurationAsync;
    private string _assemblyName;
    private WebApplicationBuilder _builder;

    public T GetAppSetting<T>(string name) => _builder.Configuration.GetValue<T>(name);

    public void OnConfigureServices(Action<IServiceCollection> a) => _configureServices = a;
    public void OnConfigureServices(Func<IServiceCollection, Task> f) => _configureServicesAsync = f;
    public void OnStarting(Action<WebApplication> a) => _onStarting = a;
    public void OnStarting(Func<WebApplication, Task> f) => _onStartingAsync = f;


    protected virtual async Task RunConfigureServices(WebApplicationBuilder builder)
    {
        _configureServices?.Invoke(builder.Services);
        if (null != _configureServicesAsync)
            await _configureServicesAsync?.Invoke(builder.Services);
    }

    protected virtual async Task RunOnStarting()
    {
        using var serviceScope = App.Services.CreateScope();
        T Get<T>() where T : class => serviceScope.ServiceProvider
            .GetRequiredService<T>();

        await Get<MqttConnection>().Init();
        _onStarting?.Invoke(App);
        if (null != _onStartingAsync)
            await _onStartingAsync.Invoke(App);
    }
    protected virtual void LoadStuff(WebApplicationBuilder builder) { }
    
    protected virtual void ResolveAddress(WebApplicationBuilder builder)
    {
        var path = builder.Configuration.GetValue<string>("ServiceAddresses");
        path ??= "addresses.json";
        _services = new ServiceRegistry(path);

        _assemblyName = Assembly.GetEntryAssembly()?.GetName().Name?.RemoveAll("MACS.") ?? Name;
        _addressInfo = _services.Lookup(_assemblyName);
        if (_addressInfo.Port == 0)
            throw new Exception(
                $@"There is something wrong with the config file. 
After parsing, the ApiPort is still 0...");
    }
    public async Task Start(string[] args)
    {
        _builder = WebApplication.CreateBuilder(args);

        Helper.SetupDefaultConfig(_builder);
        Helper.SetupLogging(_builder, "");
        LoadStuff(_builder);
        ResolveAddress(_builder);

        Log.Logger.Information("---------------------------------------------");
        Log.Logger.Information($"Constructing and initializing {Name}");

        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
        
        _builder.WebHost
            .UseUrls()
            .UseKestrel(kso =>
            {
                //kso.ListenAnyIP(_addressInfo.Port, opt => opt.UseHttps(StoreName.My, "localhost"));
                //kso.ConfigureHttpsDefaults(o => { o.SslProtocols = SslProtocols.Tls13;});
                kso.ListenAnyIP(_addressInfo.Port);
                if(_addressInfo.Port == 23100)
                    kso.ListenAnyIP(6000, x => { x.Protocols = HttpProtocols.Http2;});
            });
        Log.Logger.Information($"Listening to port: {_addressInfo.Port}");
        _builder.Services
            .AddRouting()
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.SchemaFilter<EnumSchemaFilter>();
            })
            .AddSwaggerGenNewtonsoftSupport()
            .AddSingleton(_services)
            .AddSingleton(GetMqttConnection())
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
        App.MapGet("/", async context =>
        {
            await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
        });
        App.UseExceptionHandler("/error");
        App.MapControllerRoute(name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        App.UseSwagger();
        App.UseSwaggerUI(opt=>
            opt.EnableTryItOutByDefault());

        Log.Logger.Information("Application configured successfully");
        
        Log.Logger.Information("---------------------------------------------");
        Log.Logger.Information("Application Starting");
        await RunOnStarting();
        Log.Logger.Information("---------------------------------------------");
        await App.RunAsync();
    }

    private MqttConnection GetMqttConnection()
    {
        var addr = _services.Lookup(ServiceType.Mqtt);
        var settings = new MqttConnectionSettings()
        {
            Broker = addr.Address,
            Port = addr.Port,
            ClientId = $"{addr.Extra}_{_assemblyName}"
        };
        return new MqttConnection(settings);
    }
}





public class ApiHelper<TCfg> : ApiHelper where TCfg : class, IServiceConfig, new()
{
    public TCfg Config { get; private set; } = new();
    protected override void LoadStuff(WebApplicationBuilder builder)
    {
        var path = builder.Configuration.GetValue<string>("ConfigFile");
        var fullConfigPath = Path.GetFullPath(path!);
        if (!File.Exists(fullConfigPath))
            throw new Exception($"Could not find config file at {fullConfigPath}");
        Config = JsonUtil.Load<TCfg>(fullConfigPath);
    }

    protected override async Task RunConfigureServices(WebApplicationBuilder builder)
    {
        await base.RunConfigureServices(builder);
        builder.Services.AddSingleton(Config);
        _configureServices?.Invoke(builder.Services, Config);
        if(_configureServicesAsync!= null)
            await _configureServicesAsync(builder.Services, Config);
    }

    protected override async Task RunOnStarting()
    {
        await base.RunOnStarting();
        _onStarting?.Invoke(App, Config);
        if(_onStartingAsync!= null)
            await _onStartingAsync(App, Config);
    }

    private Action<IServiceCollection, TCfg> _configureServices;
    private Func<IServiceCollection, TCfg, Task> _configureServicesAsync;
    private Func<WebApplication, TCfg, Task> _onStartingAsync;
    private Action<WebApplication, TCfg> _onStarting;
    public void OnConfigureServices(Action<IServiceCollection, TCfg> a) => _configureServices = a;
    public void OnConfigureServices(Func<IServiceCollection, TCfg, Task> f) => _configureServicesAsync = f;

    public void OnStarting(Action<WebApplication, TCfg> a) => _onStarting = a;
    public void OnStarting(Func<WebApplication, TCfg, Task> f) => _onStartingAsync = f;
}

public class ApiHelper<TCfg, TParams> :ApiHelper<TCfg> where TCfg : class, IServiceConfig, new() where TParams : class, IInitialParameters
{
    public TParams InitialParameters { get; private set; }


    private Action<IServiceCollection, TCfg, TParams> _configureServices;
    private Func<IServiceCollection, TCfg, TParams, Task> _configureServicesAsync;
    private Func<WebApplication, TCfg, TParams, Task> _onStartingAsync;
    private Action<WebApplication, TCfg, TParams> _onStarting;
    public void OnConfigureServices(Action<IServiceCollection, TCfg, TParams> a) => _configureServices = a;
    public void OnConfigureServices(Func<IServiceCollection, TCfg, TParams, Task> f) => _configureServicesAsync = f;

    public void OnStarting(Action<WebApplication, TCfg, TParams> a) => _onStarting = a;
    public void OnStarting(Func<WebApplication, TCfg, TParams, Task> f) => _onStartingAsync = f;

    protected override void LoadStuff(WebApplicationBuilder builder)
    {
        base.LoadStuff(builder);
        InitialParameters = builder.Configuration.GetSection("InitialParameters").Get<TParams>();
    }

    protected override async Task RunConfigureServices(WebApplicationBuilder builder)
    {
        await base.RunConfigureServices(builder);
        builder.Services.AddSingleton(InitialParameters)
            .AddSingleton<IInitialParameters>(InitialParameters);
        _configureServices?.Invoke(builder.Services, Config, InitialParameters);
        if(null != _configureServicesAsync)
            await _configureServicesAsync(builder.Services, Config, InitialParameters);
    }

    protected override async Task RunOnStarting()
    {
        await base.RunOnStarting();
        _onStarting?.Invoke(App, Config, InitialParameters);
        if(_onStartingAsync!= null)
            await _onStartingAsync(App, Config, InitialParameters);
    }
}