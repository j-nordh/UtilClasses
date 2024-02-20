using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Numerics;
using System.Collections;

namespace UtilClasses.JWT
{
    public class Validator
    {
        private readonly JwtParameters _sp;
        private readonly JsonWebTokenHandler _handler;
        private readonly RsaSecurityKey _rsaKey;


        public Validator(JwtParameters sp)
        {
            _sp = sp;
            _rsaKey = Crypto.GetKey(sp.PemContent, sp.PemPassword);
            _handler = new JsonWebTokenHandler();
        }

        public IEnumerable<Claim> ValidateAccessClaims(string tokenString)
        {
            var jwt = ValidateToken(tokenString, true);
            if (null == jwt) return null;
            var ret = jwt.Claims.Where(c => c.Type != JwtParameters.COMPRESSED_CLAIM).ToList();
            return ret;
        }

        public JsonWebToken ValidateToken(string tokenString, bool isAccess)
        {
            var tvp = new TokenValidationParameters
            {
                ValidAudience = isAccess? _sp.AccessAudience:_sp.RefreshAudience,
                IssuerSigningKey = _rsaKey,
                ValidIssuer = _sp.Issuer,
                ClockSkew = new TimeSpan(0, 0, 5)
            };
            var res = _handler.ValidateToken(tokenString, tvp);
            return res.SecurityToken as JsonWebToken;
        }

        public ParsedToken Parse(string tokenString)
        {
            var jwt = ValidateToken(tokenString, true);
            var perms = jwt.DecompressClaim(TokenCreator.PERMISSIONS);
            var groups = jwt.DecompressClaim(TokenCreator.GROUPS);
            return new ParsedToken() { Name = jwt.Id, Permissions = perms, Groups = groups };
        }
    }
    public class ParsedToken
    {
        public string Name;
        public List<int> Permissions;
        public List<int> Groups;
    }

    public static class  JsonWebTokenExtensions
    {
        public static Claim GetClaim(this JsonWebToken jwt, string type) => jwt.Claims.First(c => c.Type.Equals(type));
        public static List<int> DecompressClaim(this JsonWebToken jwt, string type)
        {
            var compressed = jwt.GetClaim(type).Value;
            var bits = new BitArray(BigInteger.Parse(compressed).ToByteArray());
            var ret = new List<int>();
            for (int i = 0; i < bits.Count; i++)
                if (bits[i]) ret.Add(i);
            return ret;
        }
    }

}
