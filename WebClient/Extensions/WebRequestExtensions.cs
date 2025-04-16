using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Extensions.Streams;

namespace UtilClasses.WebClient.Extensions
{
    public static class WebRequestExtensions
    {
        public static string GetResponseString(this WebRequest req, Encoding enc = null)
        {
            enc = enc ?? Encoding.UTF8;
            return req.GetResponse().GetResponseStream().AsString(enc, rewind: false);
        }
    }
}
