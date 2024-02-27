//using Common.Dto;
//using Common.Interfaces;
//using MACS.Common;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Net;
//using System.Reflection;
//using System.Threading.Tasks;
//using UtilClasses;
//using UtilClasses.Extensions.Strings;
//using UtilClasses.Extensions.Types;

//namespace MACS.Service.Api
//{

//    public abstract class CrudApiBase<T, TMan>  where T : IHasGuid where TMan : class, ICrudRepository<T>
//    {
//        private readonly TMan _man;
//        protected readonly IStringProvider _stringProvider;
//        protected List<VerbDescriptor> _descriptors = new();
//        private bool _descriptorsInitialized = false;
//        private readonly string _singleName;
//        private readonly string _pluralName;
//        private string _route;
//        protected abstract bool AllowDeleteAll { get; }
//        protected abstract bool AllowGetAll { get; }
//        protected abstract bool AllowPutAll { get; }


//        protected CrudApiBase(TMan man, IStringProvider stringProvider)
//        {
//            _man = man;
//            _stringProvider = stringProvider;
//            _singleName = StringUtil.ToSingle(typeof(T).Name.RemoveAllOic("Wrapper"));
//            _singleName = StringUtil.TrimInterfaceIndicator(_singleName);
//            _pluralName = StringUtil.FixPluralization($"{_singleName}s");
//            _route = StringUtil.FixPluralization(_pluralName.RemoveAllOic("Config")).ToLower();
//        }

//        protected virtual string AddDescription => $"Adds a {_singleName} to the system.";

//        protected virtual string GetOneDescription =>
//            $"Returns a single {_singleName} as specified by the provided Id, if any.";
//        protected virtual string GetAllDescription =>
//            $"Returns all {_pluralName} currently available in the system.";

//        protected virtual string DeleteAllDescription =>
//            $"Removes all {_pluralName} from the system.";
//        protected virtual string SetAllDescription =>
//            $"Removes all {_pluralName} from the system and then adds the provided {_pluralName}.";
//        protected virtual string SetDescription =>
//            $"Sets (ads or overwrites) a {_singleName} in the system.";
//        protected virtual string DeleteDescription =>
//            $"Deletes a {_singleName} as specified by the provided Id, if any.";
//        //[BaseRoute(HttpVerbs.Any, "/")]
//        //public async Task Switch()
//        //{
//        //    var p = Request.QueryString.Maybe("Id") ?? Route.SubPath;
//        //    var id = p.MaybeAs<Guid>();

//        //    switch (Route.SubPath?.ToLower().Trim('/'))
//        //    {
//        //        case "all":
//        //            await AllSwitch();
//        //            return;
//        //    }

//        //    switch (Request.HttpVerb)
//        //    {
//        //        case HttpVerbs.Delete:
//        //            Ensure.Argument.NotNull(id, "This method requires an Id");
//        //            Delete(id.Value);
//        //            Response.SetEmptyResponse(HttpStatusCode.Accepted);
//        //            break;
//        //        case HttpVerbs.Get:
//        //            Ensure.Argument.NotNull(id, "This method requires an Id");
//        //            WriteResult(Get(id.Value));
//        //            break;
//        //        case HttpVerbs.Post:
//        //            WriteResult(await Add());
//        //            break;
//        //        case HttpVerbs.Put:
//        //            await Set();
//        //            break;
//        //        default:
//        //            throw new ArgumentOutOfRangeException();
//        //    }
//        //}

//        //private void WriteResult(object o)
//        //{
//        //    Response.ContentType = "application/json";
//        //    Response.OutputStream.WriteUtf8(JsonUtil.Serialize(o));
//        //}

//        //[BaseRoute(HttpVerbs.Any, "/all")]
//        //public async Task AllSwitch()
//        //{
//        //    await Task.FromResult(0);
//        //    switch (Request.HttpVerb)
//        //    {
//        //        case HttpVerbs.Delete:
//        //            if (!AllowDeleteAll) throw new MethodAccessException("DELETE is not allowed on \"All\".");
//        //            Clear();
//        //            Response.SetEmptyResponse(HttpStatusCode.OK);
//        //            break;
//        //        case HttpVerbs.Get:
//        //            if (!AllowGetAll) throw new MethodAccessException("GET is not allowed on \"All\".");
//        //            WriteResult(All());
//        //            break;
//        //        case HttpVerbs.Put:
//        //            if (!AllowPutAll) throw new MethodAccessException("PUT is not allowed on \"All\".");
//        //            Clear();
//        //            var lst = await HttpContext.GetBody<List<T>>();
//        //            List<T> ret = new();
//        //            foreach (var o in lst)
//        //                ret.Add(_man.Add(o));
//        //            WriteResult(ret);
//        //            break;
//        //        default:
//        //            throw new ArgumentOutOfRangeException();
//        //    }
//        //}
//        //public virtual async Task<T> Add() => _man.Add(await HttpContext.GetBody<T>());

//        public virtual List<T> All() => _man.Get();

//        public virtual T Get(Guid id) => _man.Get(id);

//        //public virtual async Task Set()
//        //{
//        //    var o = await HttpContext.GetBody<T>();
//        //    _man.Put(o.Id, o);
//        //}

//        public virtual void Clear() => _man.Clear();

//        public virtual void Delete(Guid id) => _man.Delete(id);
//        //protected void Describe(string name, HttpVerbs verb, string expectsBody, string returns, string description,
//        //    params (string Name, string Type, bool Optional)[] ps) =>
//        //    Describe(name, verb, expectsBody, returns, description, null, ps);
//        //protected void Describe(string name, HttpVerbs verb, string expectsBody, string returns, string description, string route, params (string Name, string Type, bool Optional)[] ps)
//        //{
//        //    if (route.IsNotNullOrEmpty())
//        //        route = route.Trim('/').Insert(0, "/");
//        //    _descriptors.Add(new VerbDescriptor()
//        //    {
//        //        Description = description,
//        //        FromBody = _stringProvider.DescribeType(expectsBody),
//        //        Name = name,
//        //        Route = $"/{_route}{route}",
//        //        Returns = _stringProvider.DescribeType(returns),
//        //        Verb = verb.ToString().ToUpper(),
//        //        Parameters = ps.Any() ? ps.Select(t => new VerbParameterDescriptor() { Name = t.Name, Type = t.Type, Optional = t.Optional }).ToList() : new List<VerbParameterDescriptor>()
//        //    });
//        //}
//        //public virtual VerbDescriptor Describe(MethodInfo mi)
//        //{
//        //    InitDescriptors();
//        //    var ret = _descriptors.FirstOrDefault(d => d.Matches(mi));
//        //    if (null != ret) return ret;
//        //    Debug.WriteLine($"Could not find descriptor for: {mi.Name}({mi.GetParameters().Select(p => $"{p.ParameterType.SaneName()} {p.Name}").Join(", ")})");
//        //    return new VerbDescriptor();
//        //}

//        //private void InitDescriptors()
//        //{
//        //    if (_descriptorsInitialized) return;
//        //    var typeName = typeof(T).SaneName();

//        //    Describe("Add", HttpVerbs.Post, typeName, typeName, AddDescription);

//        //    Describe("Get", HttpVerbs.Get, null, typeName,
//        //        GetOneDescription);

//        //    Describe("Set", HttpVerbs.Put, typeName, "", SetDescription);
//        //    Describe("Delete", HttpVerbs.Delete, "", "", DeleteDescription,
//        //        ("Id", "Guid", false));
//        //    if (AllowGetAll)
//        //        Describe("GetAll", HttpVerbs.Get, null, $"List<{typeName}>", GetAllDescription, "all");
//        //    if (AllowGetAll)
//        //        Describe("DeleteAll", HttpVerbs.Delete, null, "", DeleteAllDescription, "all");
//        //    if (AllowPutAll)
//        //        Describe("SetAll", HttpVerbs.Put, $"List<{typeName}>", $"List<{typeName}>", SetAllDescription, "all");
//        //    _descriptorsInitialized = true;
//        //}

//        //public List<VerbDescriptor> DescribeAll()
//        //{
//        //    InitDescriptors();
//        //    return _descriptors;
//        //}
//    }
//}