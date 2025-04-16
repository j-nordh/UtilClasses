using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UtilClasses.Extensions.Strings;
using UtilClasses.JWT;

namespace UtilClasses.Web.JWT
{
    public class JwtLicenseValidator
    {
        private readonly string _htsAddress;
        private Task<HashSet<string>> _modules;
        private readonly Regex _regex = new Regex("[a-zA-Z0-9-_]+?.[a-zA-Z0-9-_]+?.[a-zA-Z0-9-_]+");
        private readonly JwtParameters _ps;
        private bool _invalid;
        public Exception Ex { get; private set; }
        public DateTime Expires { get; private set; } = DateTime.MinValue;
        public static JwtLicenseValidator Get { get; private set; }
        private JwtLicenseValidator(string htsAddress, JwtParameters ps)
        {
            _htsAddress = htsAddress.MakeIt().EndWith('/');
            Refresh();
            _ps = ps;
        }

        public static void Initialize(string htsAddress)
        {
            Get = new JwtLicenseValidator(htsAddress);
        }

        public void Refresh()
        {
            _modules = Task.Run(RefreshAsync);
        }

        public async Task<HashSet<string>> RefreshAsync()
        {
            var ret = new HashSet<string>();
            try
            {
                Ex = null;
                var client = new HttpClient { BaseAddress = new Uri(_htsAddress) };
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var res = await client.GetAsync("License");
                if (res.IsSuccessStatusCode)
                {
                    var content = await res.Content.ReadAsStringAsync();
                    if (content.IsNullOrEmpty()) throw new Exception("No license recieved from server.");
                    var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
                    var jwt = response["License"];
                    if (!_regex.IsMatch(jwt)) throw new Exception("This does not look like a JWT token: " + jwt);
                    var validator = new Validator(_ps);
                    var token = validator.ValidateToken(jwt);
                    if(token == null ) throw new Exception("Token validation failed!");
                    var modules = token.Claims.First(c => c.Type == "Modules").Value.ToLower();
                    ret.UnionWith(JsonConvert.DeserializeObject<List<string>>(modules));
                    Expires = token.ValidTo.AddHours(-1);
                }
                _invalid = !res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Ex = ex;
            }
            return ret;
        }


        public void Require(string module)
        {   
            if(!IsLicensed(module.ToLower())) throw new Exception($"The module {module} is not licensed for use.");
        }

        public bool IsLicensed(string module)
        {
            if (_modules==null || _invalid ||Expires > DateTime.MinValue && Expires <= DateTime.UtcNow)
                Refresh();
            return _modules.ConfigureAwait(false).GetAwaiter().GetResult().Contains(module.ToLower());
        }

        public IEnumerable<string> Modules => _modules.Result;
    }
}
