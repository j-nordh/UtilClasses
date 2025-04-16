using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace UtilClasses.Core.Compression;

class DecompressionHandler : HttpClientHandler
{
    public Collection<Compressor> Compressors;

    public DecompressionHandler()
    {
        Compressors = new Collection<Compressor> {new GZipCompressor()};
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        var encoding = response.Content.Headers.ContentEncoding?.FirstOrDefault();
        if (null == encoding) return response;
        var compressor = Compressors.FirstOrDefault(c =>
            c.EncodingType.Equals(encoding, StringComparison.InvariantCultureIgnoreCase));
        if (compressor != null)
            response.Content = await DecompressContent(response.Content, compressor);

        return response;
    }

    private static async Task<HttpContent> DecompressContent(HttpContent compressedContent, Compressor compressor)
    {
        using (compressedContent)
        {
            MemoryStream decompressed = new MemoryStream();
            await compressor.Decompress(await compressedContent.ReadAsStreamAsync(), decompressed);
            var newContent = new StreamContent(decompressed);
            // copy content type so we know how to load correct formatter
            newContent.Headers.ContentType = compressedContent.Headers.ContentType;

            return newContent;
        }
    }
}