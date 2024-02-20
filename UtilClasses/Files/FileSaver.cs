using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UtilClasses.Extensions.Objects;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Files
{
    public class FileSaver
    {
        private readonly string _path;
        private List<HandCodedBlock> _blocks;
        private string _newContent;

        public string NewContent
        {
            get => _blocks.Aggregate(_newContent, (s, b) => b.ApplyTo(s));
            set => _newContent = value;
        }
        public string OldContent { get; }
        public Encoding Encoding { get; set; }
        public bool IgnoreCase { get; set; }
        public bool CompareWithoutWhitespace { get; set; }
        public bool Force { get; set; }

        public FileSaver AddBlock(HandCodedBlock block)
        {
            block.Update(OldContent);
            _blocks.Add(block);
            return this;
        }




        public FileSaver(string path)
        {
            _path = path;
            _blocks = new List<HandCodedBlock>();
            Encoding = Encoding.UTF8;
            CompareWithoutWhitespace = true;
            OldContent = "";
            if (File.Exists(_path))
                using(var fs = File.Open(_path, FileMode.Open,FileAccess.Read))
                    OldContent = new StreamReader(fs, Encoding).ReadToEnd();
            _blocks.ForEach(b => b.Update(OldContent));

        }
        public FileSaver(string path, string newContent) : this(path)
        {
            NewContent = newContent;
        }

        public static bool SaveIfChanged(string path, string newContent) => new FileSaver(path, newContent).SaveIfChanged();


        public bool SaveIfChanged()
        {
            if (!IsChanged()) return false;
            var dir = Path.GetDirectoryName(_path);
            if (dir.IsNotNullOrEmpty() && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(_path, NewContent, Encoding);
            return true;
        }


        private bool IsChanged()
        {
            if (Force) return true;
            var org = OldContent;
            var content = NewContent;
            if (CompareWithoutWhitespace)
            {
                org = org.RemoveAllWhitespace();
                content = content.RemoveAllWhitespace();
            }
            return !content.Equals(org, IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }

    }

    public class HandCodedBlock
    {
        public string Start { get; }
        public string End { get; }
        public string? Content { get; set; }

        public HandCodedBlock () 
        {
            Start = "//############ Hand Coded Start ############";
            End = "//############  Hand Coded End  ############";
        }
        public HandCodedBlock(string start, string end, string? content = null)
        {
            Start = start;
            End = end;
            Content = content;
        }

        public static HandCodedBlock FromKeyword(string keyword, string prefix = "//############ ", string suffix = " ############", string? content = null) => 
            new HandCodedBlock($"{prefix}Hand Coded {keyword} Start{suffix}", $"{prefix} Hand Coded {keyword} End {suffix}");
        public static HandCodedBlock ShortComment(string keyword, string? content) => new($"//Start{keyword}", $"//End{keyword}", content);
        public static HandCodedBlock ShortComment(string keyword) => ShortComment(keyword, null);

        public void Update(string oldContent)
        {
            Content = null;
            if (Start.IsNullOrEmpty() || End.IsNullOrEmpty() || !oldContent.Contains(Start) || !oldContent.Contains(End))
                return;
            Content = oldContent.SubstringAfter(Start).SubstringBefore(End);
        }

        public IAppendable Empty(bool withRegion=false) => new EmptyAppendable(Start, End, withRegion);

        public string ApplyTo(string newContent)
        {
            if (Content.IsNullOrWhitespace()) return newContent;
            var before = newContent.SubstringBefore(Start, out var rest);
            if (null == rest) return newContent; //no start tag found
            var after = rest.SubstringAfter(End);
            var indent = new string(rest.Substring(0, rest.IndexOf(End)).Reverse().TakeWhile(c=>c== ' ' || c=='\t').Reverse().ToArray());
            return new IndentingStringBuilder("")
                .Append(before)
                .AppendLines(Start.TrimEnd(), Content.TrimEnd())
                .Append(indent)
                .Append(End)
                .Append(after).ToString();
        }

        private class EmptyAppendable:IAppendable
        {
            public bool WithRegion { get; }
            public string Start { get; }
            public string End { get; }
            public EmptyAppendable(string start, string end, bool withRegion=false)
            {
                Start = start;
                End = end;
                WithRegion = withRegion;
            }

            public IndentingStringBuilder AppendObject(IndentingStringBuilder sb)=>sb
    .Maybe(WithRegion, () => sb.AppendLine("#region Hand Coded"))
    .AppendLines(Start, "", "", End)
    .Maybe(WithRegion, () => sb.AppendLine("#endregion"));
            
        }
    }
    public static class FileSaverExtensions
    {
        public static FileSaver WithBlock(this FileSaver fs, HandCodedBlock block)
        {
            fs.AddBlock(block);
            return fs;
        }

        public static FileSaver WithBlocks(this FileSaver fs,  IEnumerable<HandCodedBlock> blocks)
        {
            foreach (var block in blocks) fs.AddBlock(block);
            return fs;
        }

        public static FileSaver WithBlock(this FileSaver fs, string start, string end) 
        {
            fs.AddBlock(new HandCodedBlock(start, end));
            return fs;
        }

        public static FileSaver WithContent(this FileSaver fs, string content) => fs.Do(() => fs.NewContent = content);
        public static FileSaver Forced(this FileSaver fs, bool force = true) => fs.Do(() => fs.Force = force);
    }
}
