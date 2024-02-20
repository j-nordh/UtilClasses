using System;
using System.Collections.Generic;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Strings;

namespace UtilClasses
{
    public class LineBuffer
    {
        private string _prev = "";
        public List<string> Lines { get; }
        //public event Action<string> LineFound;
        private List<Action<string>> _lineFoundHandlers = new(); 
        public LineBuffer(bool saveLines)
        {
            Lines = saveLines ? new List<string>() : null;
        }

        public void Subscribe(Action<string> a)
        {
            _lineFoundHandlers.Add(a);
        }
        public string Tail => _prev;

        public void Append(string? s)
        {
            if (null == s) return;
            s = _prev + s;
            _prev = "";
            while (s.IsNotNullOrEmpty())
            {
                if (!s!.Contains("\n"))
                {
                    _prev = s;
                    break;
                }
                var l = s.SubstringBefore("\n", out var rest);
                OnLine(l);
                s = rest.IsNullOrEmpty()
                    ? null
                    : rest.Substring(1);
            }
        }

        private void OnLine(string l)
        {
            Lines?.Add(l);
            l = l.Replace("\r", "");
            _lineFoundHandlers.ForEach(a => a(l));
            //CTrace.Go(LineFound?.GetInvocationList().Length.ToString());
            //LineFound?.Invoke(l.Replace("\r", ""));
        }

        public void Flush()
        {
            if (_prev.IsNullOrEmpty()) return;
            OnLine(_prev);
            _prev = "";

        }

        public void Clear()
        {
            Lines.Clear();
            _prev = "";
        }

        public override string ToString()
        {
            Flush();
            return Lines?.Join("\n");
        }
    }
}
