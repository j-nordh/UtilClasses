using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using Nancy;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Nancy
{
    public class BlobModule: NancyModule
    {
        private List<IResourceResolver> _resolvers;
       

        public BlobModule(IResourceResolver res)
        {
            _resolvers= new List<IResourceResolver> {res, new LocalResolver()};
            Get["/img/{filename}"] = x => PngResponse(x.filename);
            Get["/images/{filename}"] = x => PngResponse(x.filename);
        }

        private class LocalResolver :IResourceResolver
        {
            public IEnumerable<string> GetNames() => GetType().Assembly.GetManifestResourceNames();

            public Stream GetStream(string name) => GetType().Assembly.GetManifestResourceStream(name);
        }

        private byte[] GetBytes(string dir,string filename)
        {
            using (var stream = _resolvers.Select(r => r.MaybeGetStream(dir, filename)).FirstOrDefault(s => s != null))
            using (var rdr = new BinaryReader(stream))
            {
                return rdr.ReadBytes((int) stream.Length);
            }
        }

        private Response PngResponse(string filename)
        {
            var bytes = GetBytes("img",filename);
            return null == bytes ? HttpStatusCode.NotFound : bytes.AsResponse("image/png");
        }
    }

    public interface IResourceResolver
    {
        IEnumerable<string> GetNames();
        Stream GetStream(string name);
    }

    static class IResourceResolverExtensions
    {
        public static Stream MaybeGetStream(this IResourceResolver res, string dir, string filename)
        {
            var name =  res.GetNames().FirstOrDefault(s => s.ContainsOic(dir) && s.EndsWithIc2(filename));
            if (name == null) return null;
            return res.GetStream(name);
        }
    }
}
