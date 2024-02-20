using System;
using UtilClasses.JWT.Properties;

namespace UtilClasses.JWT
{
    public class JwtParameters
    {
        public const string COMPRESSED_CLAIM = "CompressedClaim";
        public string Issuer { get; }
        public string AccessAudience { get; }
        public string RefreshAudience { get; }
        public string PemContent { get; }
        public string PemPassword { get; }
        public string Algorithm { get; }
        public TimeSpan AccessTimeout {get;}
        public TimeSpan RefreshTimeout { get; }
        public JwtParameters( 
            string pemContent, 
            string pemPassword = null,
            string issuer= "RISE",
            string accessAudience= "RISE_Access", 
            string refreshAudience= "RISE_Refresh", 
            string alg="RS512", 
            TimeSpan? accessTimeout=null, 
            TimeSpan? refreshTimeout=null)
        {
            Issuer = issuer;
            AccessAudience = accessAudience;
            RefreshAudience = refreshAudience;
            PemContent = pemContent;
            PemPassword = pemPassword;
            Algorithm = alg;
            AccessTimeout = accessTimeout ?? TimeSpan.FromHours(1);
            RefreshTimeout = refreshTimeout ?? TimeSpan.FromHours(18);
        }
    }
}
