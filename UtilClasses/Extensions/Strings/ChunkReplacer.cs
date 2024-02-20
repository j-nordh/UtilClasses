using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilClasses.Extensions.Strings
{
    public class ChunkReplacer
    {
        private readonly StringBuilder _sb;
        private readonly int _start;
        private int _len;
        private int _before;
        public ChunkReplacer(StringBuilder sb, int start, int stop)
        {
            _sb = sb;
            _start = start;
            _len = stop - start;
            _before = _sb.Length;
        }

        public ChunkReplacer Replace(string oldVal, string newVal)
        {
            _sb.Replace(oldVal, newVal, _start, _len);
            var after = _sb.Length;
            _len = _before - after;
            _before = after;
            return this;
        }

    }
    public class Chunk
    {
        public int Start { get; }
        public int Stop { get; }
        public int Length => Stop - Start;
        private readonly StringBuilder _sb;
        private string _content;


        public Chunk(int start, int stop, StringBuilder sb)
        {
            Start = start;
            Stop = stop;
            _sb = sb;
            _content = sb.ToString(start, Length);

        }
        public Chunk(StringBuilder sb) : this(0, sb.Length, sb)
        { }

        public IEnumerable<Chunk> GetChunks(char startDelimiter, char stopDelimiter, bool onlyTopNodes, int startOffset, int stopOffset)
        {
            var _startIndices = new Stack<int>();
            var blockStart = Start + startOffset;
            var blockLen = Stop - stopOffset - blockStart;
            int i;
            for (int x = 0; x < blockLen; x++)
            {
                i = x + blockStart;
                if (_sb[i] == startDelimiter) _startIndices.Push(i);
                if (_sb[i] == stopDelimiter)
                {
                    var start = _startIndices.Pop();
                    if (_startIndices.Count == 0 || !onlyTopNodes)
                        yield return new Chunk(start, i, _sb);
                }
            }
        }

        public int IndexOfOic(string needle) => _content.IndexOfOic(needle);
        public string Substring(int start, int len) => _content.Substring(start, len);
        public Chunk SubChunk(int start, int stop)
        {
            if (stop < 0) stop += _content.Length;
            return new Chunk(Start + start, Start + stop, _sb);
        }
        public Dictionary<string, Chunk> KeyVals()
        {
            var ret = new Dictionary<string, Chunk>();
            var matches = new Dictionary<char, char>() { { '{', '}' }, { '[', ']' }, { '"', '"' } };
            var _inside = new Stack<char>();
            int i;
            var sb = new StringBuilder();
            string key="";
            int valStart=0;
            for (var x = 0; x < Length; x++)
            {
                i = x + Start;
                var c = _sb[i];
                if (matches.ContainsKey(c))
                    _inside.Push(c);
                if (_inside.Any() && matches[_inside.Peek()] == c)
                    _inside.Pop();

                if (!_inside.Any() && c == ':')
                {
                    key = sb.ToString().Trim();
                    sb.Clear();
                    valStart = i + 1;
                }
                else if (!_inside.Any() && c == ',')
                {
                    ret[key] = SubChunk(valStart, i);
                    valStart = 0;
                    key = "";
                    sb.Clear();
                }
                else
                    sb.Append(c);
                    
            }
            if (key.IsNotNullOrEmpty() && valStart > 0)
                ret[key] = SubChunk(valStart, Stop);

            return ret;
        }

        public ChunkGetter Get => new ChunkGetter(this);

        public class ChunkGetter
        {
            char StartDelimiter { get; set; }
            char StopDelimiter { get; set; }
            int StartOffset { get; set; }
            int StopOffset { get; set; }
            bool _onlyTopChunks;
            Chunk _chunk;
            public static implicit operator Func<IEnumerable<Chunk>>(ChunkGetter g) => ()
                => g._chunk.GetChunks(
                    g.StartDelimiter,
                    g.StopDelimiter,
                    g._onlyTopChunks,
                    g.StartOffset,
                    g.StopOffset);

            public ChunkGetter(Chunk parent)
            {
                _chunk = parent;
                StartOffset = 0;
                StopOffset = 0;
            }

            public ChunkGetter OnlyTop() => Do(() => _onlyTopChunks = true);
            public ChunkGetter OnlyTop(bool val) => Do(() => _onlyTopChunks = val);
            public ChunkGetter Offset(int offset) => Offset(offset, offset);
            public ChunkGetter Offset(int startOffset, int stopOffset)
            {
                StartOffset = startOffset;
                StopOffset = stopOffset;
                return this;
            }
            public ChunkGetter Curly() => Delimiters('{', '}');
            public ChunkGetter Brackets() => Delimiters('[', ']');
            public ChunkGetter Delimiters(char start, char stop)
            {
                StartDelimiter = start;
                StopDelimiter = stop;
                return this;
            }

            public IEnumerable<Chunk> Chunks() => _chunk.GetChunks(StartDelimiter, StopDelimiter, _onlyTopChunks, StartOffset, StopOffset);

            private ChunkGetter Do(Action a)
            {
                a();
                return this;
            }


        }
        public static IEnumerable<Chunk> FromString(string s, char startDelimiter, char stopDelimiter, bool onlyTopNodes)
        {
            if (null == s) return null;
            int starts = s.Count(c => c == startDelimiter);
            if (starts == 0 || starts != s.Count(c => c == stopDelimiter))
                throw new ArgumentException("The supplied file does not have matching delimiters.");
            var sb = new StringBuilder();
            sb.Append(s);
            return new Chunk(sb).GetChunks(startDelimiter, stopDelimiter, onlyTopNodes, 0, 0);
        }
        public override string ToString() => _sb.ToString(Start, Length);

        public string Describe() => $"Start: {Start}\r\nStop: {Stop}\r\nContent:{_sb.ToString(Start, Length)}";
    }
}
