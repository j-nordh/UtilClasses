using System;
using System.IO;
using System.Text;
using Nancy;
using Newtonsoft.Json;

namespace UtilClasses.Nancy
{
    public static class NancyExtensions
    {
        public static Response AsResponse(this string content) => new Response
        {
            Contents = stream =>
            {
                using (var wr = new StreamWriter(stream, Encoding.UTF8))
                    wr.Write(content);
            }
        };

        public static Response AsResponse(this string content, string contentType)
        {
            var response = AsResponse(content);
            response.ContentType = contentType;
            return response;
        }

        public static Response AsResponse(this byte[] bytes, string contentType = null)
            => new Response()
            {
                ContentType = contentType ?? "application/octet-stream",
                Contents = stream =>
                {
                    using (var writer = new BinaryWriter(stream))
                        writer.Write(bytes);
                }
            };

        public static Response AsResponse(this Exception ex) => new Response
        {
            Contents = stream =>
            {
                using (var wr = new StreamWriter(stream, Encoding.UTF8))
                    wr.Write(JsonConvert.SerializeObject(ex));
            },
            ContentType = "application/json",
            StatusCode = HttpStatusCode.InternalServerError
        };

        public static T WithStatus<T>(this T r, HttpStatusCode code) where T:Response
        {
            r.StatusCode = code;
            return r;
        }
        public static T WithContentTypeJson<T>(this T r) where T : Response
        {
            r.ContentType = "application/json";
            return r;
        }
    }
}