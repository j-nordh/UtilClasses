using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Extensions.Objects;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.JWT
{
    public class Authorizer
    {
        public enum UnauthorizedReason
        {
            [Description("HTTP header Authorization scheme must be \"Bearer\".")] NoBearer,
            [Description("No token was provided as HTTP header Authorization parameter.")] NoToken,
            [Description("The token provided has expired.")] TokenExpired,
            [Description("The user with the provided credentials is not authorized to access the URL requested.")] Forbidden,
            [Description("The user does not posess the required permissions to access the URL requested.")] NoPermission
        }

        public class AuthResult
        {
            
            public UnauthorizedReason? Reason { get; }
            public ClaimsIdentity Identity { get; }

            public AuthResult(UnauthorizedReason reason)
            {
                Reason = reason;
                Identity = null;
            }

            public AuthResult(ClaimsIdentity identity)
            {
                Identity = identity;
                Reason = null;
            }
        }
        private readonly Validator _validator;

        public Authorizer(JwtParameters sp)
        {
            _validator = new Validator(sp);
        }

        public AuthResult Authorize(string authHeader)
        {
            if (authHeader == null || !authHeader.StartsWith("Bearer"))
                return new AuthResult(UnauthorizedReason.NoBearer);
            var parts = authHeader.Split(' ');
            var token = parts.Length == 2 ? parts[1] : null;
            if (token.IsNullOrWhitespace())
            {
                return new AuthResult(UnauthorizedReason.NoToken);
            }
            try
            {
                var claims = _validator.ValidateAccessClaims(token).ToList();
                var subjectClaim = claims?.FirstOrDefault(c => c.Type == "unique_name");
                if (null == subjectClaim)
                    return new AuthResult(UnauthorizedReason.Forbidden);
                return new AuthResult(new ClaimsIdentity(new  GenericIdentity(subjectClaim.Value), claims));
            }
            catch (Exception e)
            {
                if (!e.Message.ContainsOic("Lifetime validation")) throw;
                return new AuthResult(UnauthorizedReason.TokenExpired);
            }
        }
    }

    public static class AuthorizerExtensions
    {
        public static string Description(this Authorizer.UnauthorizedReason p)
        {
            var type = typeof(Authorizer.UnauthorizedReason);
            var memInfo = type.GetMember(p.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length == 0 ? "" : attributes.First().As<DescriptionAttribute>().Description;
        }
    }
}
