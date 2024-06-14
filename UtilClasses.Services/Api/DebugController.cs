using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Strings;
using UtilClasses.Extensions.Types;
using UtilClasses.Files;

namespace UtilClasses.Services.Api;
#if !DEBUG
    [ApiExplorerSettings(IgnoreApi = true)]
#endif
[Controller]
[Route("[controller]")]
public class DebugController : Controller
{
    private readonly IEnumerable<EndpointDataSource> _endpointSources;
    private readonly IServiceCollection _serviceCollection;

    public DebugController(IEnumerable<EndpointDataSource> endpointSources, IServiceCollection serviceCollection)
    {
        _endpointSources = endpointSources;
        _serviceCollection = serviceCollection;
    }

    [HttpGet("routes")]
    public string PrintRoutes([FromQuery] bool updateFiles = false)
    {
        var sources = _endpointSources.ToList();
        var endpoints = sources.SelectMany(s => s.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(ep => ep.DisplayName.ContainsOic("controller."))
            .ToList();

        var assemblyName = Assembly.GetEntryAssembly()!.GetName().Name!;


        var dict = new DictionaryOic<List<string>>();

        foreach (var endpoint in endpoints)
        {
            var c = endpoint.DisplayName.SubstringAfter("Api.").SubstringBefore(".", out var rest)
                .RemoveAllOic("Controller");
            var action = rest.SubstringBefore(" (").SubstringAfter(".");
            dict.GetOrAdd(c).Add(action);
        }


        var tree = new TextTree("Sources");
        foreach (var c in dict.Keys.OrderBy(x => x))
        {
            var branch = tree.CreateBranch(c);
            foreach (var action in dict[c])
                branch.Add(action);
        }

        var sb = new IndentingStringBuilder("  ");
        sb.AppendLine(tree.ToString());

        var assemblies =
            endpoints.Select(ep => ep.DisplayName
                .SubstringAfter("(")
                .SubstringBefore(")")
            ).Distinct();

        // The following code could be adapted to generate route files. But it's a bit hard to generalize,
        // hence it is commented out as a possible future improvement.

        // var dir = new DirectoryInfo(Environment.CurrentDirectory);
        // while (!dir.GetDirectories().Any(d => d.Name.EqualsOic("MACS.Common")))
        //     dir = dir.Parent;
        //
        // string fileDir = Path.Combine(dir.FullName, "MACS.Common", "Routes");
        //
        // foreach (var ass in assemblies)
        // {
        //
        //     var name = ass.RemoveAllOic("macs.");
        //     if (!name.EqualsOic("Service"))
        //         name = name.RemoveAllOic("Service");
        //     name = name.MakeIt().StartWithUpperCase;
        //
        //     var cls = GetClass(name, endpoints.Where(ep => ep.DisplayName.StartsWithOic(ass)));
        //     sb.AppendLine(cls);
        //     if (updateFiles)
        //     {
        //         FileSaver.SaveIfChanged(Path.Combine(fileDir, $"{name}Routes.cs"), cls);
        //     }
        // }

        return sb.ToString();
    }

    [HttpGet("di")]
    public Dictionary<string, List<string>> DependecyInjectionProblems(bool hideOk = false)
    {
        var serviceProvider = _serviceCollection.BuildServiceProvider();
        var res = new DictionaryOic<List<string>>();
        foreach (var service in _serviceCollection)
        {
            var serviceType = service.ServiceType as System.Type;
            try
            {
                if (serviceType.ContainsGenericParameters)
                {
                    res.GetOrAdd("NotValidated")
                        .Add($"{serviceType.SaneName()}: Type ContainsGenericParameters == true");
                    continue;
                }

                var _ = serviceProvider.GetService(service.ServiceType);
                if (hideOk) continue;
                res.GetOrAdd("Valid").Add(serviceType.ToString());
            }
            catch (Exception e)
            {
                res.GetOrAdd("Problems").Add($"{serviceType.SaneName()}: {e.Message}");
            }
        }

        return res;
    }

    string GetClass(string name, IEnumerable<RouteEndpoint> endpoints)
    {
        var sb = new IndentingStringBuilder("  ");

        //var routes = dict.SelectMany(c => c.Value.Select( a => (Controller:c.Key, Action: a)));
        var routes = endpoints
            .Select(ep => ep.RoutePattern.RawText.SubstringBefore("/{")).Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        sb.AppendLines($"public static class {name}Routes", "{")
            .Indent()
            //.AppendObjects(routes, t => $"public const string {t.Controller}_{t.Action} = \"{t.Controller}/{t.Action}\"")
            .AppendObjects(routes, r => $"public const string {r.ReplaceOic("/", "_")} = \"{r}\";")
            .Outdent()
            .Append("}");

        return sb.ToString();
    }
}