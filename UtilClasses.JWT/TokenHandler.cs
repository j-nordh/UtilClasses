using Microsoft.IdentityModel.JsonWebTokens;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UtilClasses.Extensions.Objects;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.JWT
{
    public class SingleTokenHandler
    {
        private readonly Func<Task<string>> _updateFunc;
        private bool _lastParseSucceeded;
        private string _token;
        private readonly TimeSpan _margin;
        public DateTime ExpiresUtc { get; private set; }
        private TimeSpan _serverOffset;
        private static readonly Regex FormatRegex = new Regex(@"^[a-zA-Z0-9-_]+\.[a-zA-Z0-9-_]+\.[a-zA-Z0-9-_]+$");

        public SingleTokenHandler(Func<Task<string>>updater, TimeSpan margin)
        {
            _updateFunc = updater;
            _margin = margin;
            _token = null;
        }
        public SingleTokenHandler(string initialTtoken, Func<Task<string>> updater, TimeSpan margin):this(updater, margin)
        {
            _token = initialTtoken;
            Parse();
        }

        public async Task<string> Token(bool refreshIfNeeded=true)
        {
            if(refreshIfNeeded) await Refresh();
            return _token;
        }

        public async Task Refresh()
        {
            if (!NeedsRefresh) return;
            var res = await _updateFunc();
            if (res.IsNullOrEmpty()) throw new Exception("No token resolved");
            if (!FormatRegex.IsMatch(res))
                throw new Exception("The token provided is not in a valid format. See data...").With(e =>
                    e.Data["Token"] = res);
            _token = res;
            Parse();
        }
        
        public async Task ForceRefresh()
        {
            ExpiresUtc = DateTime.MinValue;
            await Refresh();
        }
        public bool NeedsRefresh
        {
            get
            {
                if (_token.IsNullOrEmpty()) return true; //Could it be a duck?
                if (!FormatRegex.IsMatch(_token)) return true;//does it look lika a duck?
                if (!_lastParseSucceeded) return true;  //does it quack like a duck?
                if (ExpiresUtc.Year < 2000) return true;
                var deadline = ExpiresUtc + _serverOffset - _margin;
                return DateTime.UtcNow > deadline;      //is the duck too old to eat?    
            }
        }
        private void Parse()
        {
            _lastParseSucceeded = false;
            if(_token.IsNullOrEmpty())
                throw new ArgumentException("No token to parse!");
            var token = new JsonWebToken(_token);
            ExpiresUtc= token.ValidTo;
            _lastParseSucceeded = true;
            _serverOffset = DateTime.UtcNow - token.IssuedAt;
        }
        
        public void Invalidate()
        {
            _token = null;
        }

        public bool Valid => _token.IsNotNullOrEmpty() && _lastParseSucceeded && ExpiresUtc > DateTime.UtcNow;

        internal void SetToken(string token)
        {
            _token = token;
            Parse();
        }
    }

    public class TokenHandlerParameters
    {
        public string InitialToken { get; set; }
        public Func<string, Task<string>> RefreshFunc { get; set; }
        public TimeSpan Margin;
    }
    public class RefreshingTokenHandlerParameters
    {
        public TokenHandlerParameters Access { get; set; }
        public TokenHandlerParameters Refresh { get; set; }

        public Func<Task<string>> PanicGetNewRefreshTokenFunc;

    }
    public class RefreshingTokenHandler
    {
        public SingleTokenHandler Access {get;}
        public SingleTokenHandler Refresh { get; }
        private Func<Task<string>> _panicFunc;

        //public RefreshingTokenHandler(LoginResult dto, IRouteExecuter exec)
        //{
        //    _exec = exec;
        //    Access = new SingleTokenHandler(dto.AccessToken,
        //        Call(VanguardGate.GetAccessToken), TimeSpan.FromSeconds(20));
        //    Refresh = new SingleTokenHandler(dto.RefreshToken,
        //        Call(VanguardGate.RefreshToken), dto.RefreshMargin);
        //}

        public RefreshingTokenHandler(RefreshingTokenHandlerParameters ps)
        {
            Access = GetHandler(ps.Access);
            Refresh = GetHandler(ps.Refresh);
            _panicFunc = ps.PanicGetNewRefreshTokenFunc;
        }

        private SingleTokenHandler GetHandler(TokenHandlerParameters p) => new SingleTokenHandler(p.InitialToken,
            async () => await p.RefreshFunc(await GetRefreshToken()), p.Margin);

        private async Task<string> GetRefreshToken()
        {
            if (Refresh.Valid) return await Refresh.Token(false);
            if (_panicFunc == null)
                throw new Exception("The refresh token is invalid and no panic function is provided");
            var token = await _panicFunc();
            Refresh.SetToken(token);
            return token;
        }
        public async Task ForceRefresh()
        {
            await Refresh.ForceRefresh();
            await Access.ForceRefresh();
        }

        public async Task<RefreshingTokenHandler> RefreshIfNeeded()
        {
            await Task.WhenAll(Access.Refresh(), Refresh.Refresh());
            return this;
        }
        public bool AreValid => Refresh.Valid && Access.Valid;

        public void Invalidate()
        {
            Refresh.Invalidate();
            Access.Invalidate();
        }
    }

    public static class TokenHandlerExtensions
    {
        public static async Task<string> MaybeGetAccessToken(this RefreshingTokenHandler th)
        {
            if (null == th) return null;
            return await th.Access.Token();
        }

        public static async Task<string> MaybeGetRefreshToken(this RefreshingTokenHandler th)
        {
            if (null == th) return null;
            return await th.Refresh.Token();
        }
    }
}
