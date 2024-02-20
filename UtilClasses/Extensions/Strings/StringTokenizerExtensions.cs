using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Extensions.Enumerables;

namespace UtilClasses.Extensions.Strings
{
    public static class StringTokenizerExtensions
    {
        private class Envelope
        {
            private readonly char _start;
            private readonly char _end;
            public bool Active { get; set; }
            public Envelope(char start, char end)
            {
                _start = start;
                _end = end;
                Active = false;
            }

            public bool Match(char c)
            {
                if (!(Active ? c == _end : c == _start))return false;
                Active = !Active;
                return true;
            }
        }

        public static IEnumerable<string> Tokenize(this string str)
        {
            return Tokenize(str, new List<Envelope> {new('"', '"'), new('(', ')')}, new[] {' '});
        }
        public static TokenizerConfig ConfigureTokenizer(this string str) => new TokenizerConfig(str);

        public static IEnumerable<string> CustomTokenize(this string str, Action<TokenizerConfig> a)
        {
            var c = new TokenizerConfig(str);
            a(c);
            return c.Run();
        }
        public class TokenizerConfig
        {
            private readonly string _str;
            private List<Envelope> _envelopes = new List<Envelope>();
            private List<char> _delimiters = new List<char>();

            public TokenizerConfig(string str)
            {
                _str = str;
            }

            public TokenizerConfig AddEnvelope(char start, char end)
            {
                _envelopes.Add(new Envelope(start, end));
                return this;
            }

            public TokenizerConfig AddDelimiter(char delimiter)
            {
                _delimiters.Add(delimiter);
                return this;
            }

            public IEnumerable<string> Run() => _str.Tokenize(_envelopes, _delimiters);
        }
        
        private static IEnumerable<string> Tokenize(this string str, List<Envelope> envs, ICollection<char> delimiters)
        {
            bool escaped = false;
            var token = new List<char>();
            foreach (var c in str.ToCharArray())
            {
                if (c == '\\')
                {
                    if (escaped) token.Add('\\');
                    else escaped = true;
                    continue;
                }
                envs.Match(c);
                if (delimiters.Contains(c))
                {
                    switch (escaped)
                    {
                        case true:
                            escaped = false;
                            token.Add(c);
                            break;
                        case false when envs.Active():
                            token.Add(c);
                            continue;
                        default:
                        {
                            var ret = new string(token.ToArray()).Trim();
                            token.Clear();
                            if (!string.IsNullOrWhiteSpace(ret)) yield return ret;
                            break;
                        }
                    }
                }
                else
                {
                    token.Add(c);
                }
            }
            {
                var ret = new string(token.ToArray());
                if (!string.IsNullOrWhiteSpace(ret)) yield return ret;
            }
        }

        private static void Match(this IEnumerable<Envelope> envs, char c)
        {
            envs.ForEach(e=>e.Match(c));
        }

        private static bool Active(this IEnumerable<Envelope> envs)
        {
            return envs.Any(e => e.Active);
        }
    }
}
