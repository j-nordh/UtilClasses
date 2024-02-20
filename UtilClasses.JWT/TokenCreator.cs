using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Common.Interfaces;
using System.Numerics;
using System.Collections;
using System.Security.Principal;
using UtilClasses.Extensions.BitArrays;
using UtilClasses.Extensions.Strings;
using Microsoft.IdentityModel.JsonWebTokens;

namespace UtilClasses.JWT
{
    public class TokenCreator
    {
        public const string GROUPS = "Groups";
        public const string PERMISSIONS = "Permissions";


        public TokenCreator(JwtParameters ps, TokenPropResolvers propResolvers)
        {
            Validator = new Validator(ps);
            Create = new Creators(propResolvers, ps, new SigningCredentials(Crypto.GetKey(ps.PemContent, ps.PemPassword), "RS512"));
            Refresh = new Refreshers(this, propResolvers);
        }

        public Validator Validator { get; }

        public Refreshers Refresh { get; }
        public Creators Create { get; }

        public class Refreshers
        {
            private readonly TokenCreator _creator;
            private readonly TokenPropResolvers _propRes;

            public Refreshers(TokenCreator creator, TokenPropResolvers propRes)
            {
                _creator = creator;
                _propRes = propRes;
            }

            public string RefreshToken(string currentToken)
            {
                return _creator.Create.RefreshToken(ValidateToken(currentToken).Id);
            }

            public string AccessToken(string refreshToken)
            {
                return _creator.Create.AccessToken(ValidateToken(refreshToken).Id);
            }
            private JsonWebToken ValidateToken(string token)
            {
                var ret = _creator.Validator.ValidateToken(token, false);
                if (_propRes.Serial(ret.Id) > ret.Claims.Where(c => c.Type.Equals("SER")).Select(c => c.Value.AsInt()).First())
                    throw new UnauthorizedAccessException();
                return ret;
            }
        }

        public class Creators
        {
            JwtSecurityTokenHandler _handler = new JwtSecurityTokenHandler();
            TokenPropResolvers _propRes;
            private JwtParameters _ps;
            SigningCredentials _creds;

            public Creators(TokenPropResolvers propRes, JwtParameters ps, SigningCredentials creds)
            {
                _propRes = propRes;
                _ps = ps;
                _creds = creds;
            }

            public string AccessToken(string name)
            {
                var descriptor = new SecurityTokenDescriptor();
                var claims = new ClaimsIdentity(new GenericIdentity(name));
                claims.AddClaim(new Claim(GROUPS, Compress(_propRes.Groups(name))));
                claims.AddClaim(new Claim(PERMISSIONS, Compress(_propRes.Permissions(name))));
                var des = new SecurityTokenDescriptor
                {
                    Audience = _ps.AccessAudience,
                    Expires = DateTime.UtcNow + _ps.AccessTimeout,
                    IssuedAt = DateTime.UtcNow,
                    Issuer = _ps.Issuer,
                    SigningCredentials = _creds,
                    Subject = claims
                };
                return _handler.CreateEncodedJwt(des);
            }

            public string RefreshToken(string username)
            => _handler.CreateEncodedJwt(new SecurityTokenDescriptor
            {
                Audience = _ps.RefreshAudience,
                Expires = DateTime.UtcNow + _ps.RefreshTimeout,
                IssuedAt = DateTime.UtcNow,
                Issuer = _ps.Issuer,
                Subject = new ClaimsIdentity(new GenericIdentity(username), new Claim[] { new Claim("SER", _propRes.Serial(username).ToString()) }),
                SigningCredentials = _creds
            });

            public string Compress(IEnumerable<int> items)
            {
                if (null == items || !items.Any()) return "0";
                var lst = items as List<int> ?? items.ToList();
                var max = lst.Max(i => i) + 1;
                var bits = new BitArray(max);
                lst.ForEach(i => bits.Set(i, true));

                return new BigInteger(bits.ToByteArray()).ToString();
            }
            public string Compress(IEnumerable<IHasId> items) => Compress(items?.Select(i => (int)i.Id) ?? new int[] { });
        }
    }
    public class TokenPropResolvers
    {
        public Func<string, IEnumerable<IHasId>> Groups { get; set; }
        public Func<string, IEnumerable<int>> Permissions { get; set; }
        public Func<string, int> Serial { get; set; }

        public TokenPropResolvers()
        {
            Groups = _ => new IHasId[] { };
            Permissions = _ => new int[] { };
            Serial = _ => 1;
        }
    }
}
