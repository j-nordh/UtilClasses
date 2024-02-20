using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UtilClasses.Compression
{
    internal class CompressionHandler : DelegatingHandler
    {
        public Collection<Compressor> Compressors { get; }

        public CompressionHandler()
        {
            Compressors = new Collection<Compressor> {new GZipCompressor()};
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Debug.WriteLine("CompressionHander.SendAsync Enter| {0}", Environment.TickCount );
            var response = await base.SendAsync(request, cancellationToken);
            Debug.WriteLine("CompressionHander.SendAsync got response| {0}", Environment.TickCount);
            if (null == request.Headers.AcceptEncoding || request.Headers.AcceptEncoding.Count <= 0) return response;

            var encoding = request.Headers.AcceptEncoding.FirstOrDefault(re=>Compressors.Any(c=>c.EncodingType.Equals(re.Value, StringComparison.InvariantCultureIgnoreCase)));
            if (null == encoding) return response;

            var compressor = Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding.Value, StringComparison.InvariantCultureIgnoreCase));
            if (null == compressor) return response;

            response.Content = new CompressedContent(response.Content, compressor);
            return response;
        }
    }
}