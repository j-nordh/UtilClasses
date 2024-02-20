using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace UtilClasses.Compression
{
    internal class CompressedContent : HttpContent
    {
        private readonly HttpContent _content;
        private readonly Compressor _compressor;

        public CompressedContent(HttpContent content, Compressor compressor)
        {
            Ensure.Argument.NotNull(content, "content");
            Ensure.Argument.NotNull(compressor, "compressor");

            _content = content;
            _compressor = compressor;

            AddHeaders();
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected async override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Ensure.Argument.NotNull(stream, "stream");
            Debug.WriteLine("CompressedContent.SerializeToStreamAsync Enter| {0}", Environment.TickCount);
            using (_content)
            {
                var contentStream = await _content.ReadAsStreamAsync();
                Debug.WriteLine("CompressedContent.SerializeToStreamAsync Content Received| {0}", Environment.TickCount);
                await _compressor.Compress(contentStream, stream);
            }
            Debug.WriteLine("CompressedContent.SerializeToStreamAsync Done| {0}", Environment.TickCount);
        }

        private void AddHeaders()
        {
            foreach (var header in _content.Headers)
            {
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            Headers.ContentEncoding.Add(_compressor.EncodingType);
        }
    }
}