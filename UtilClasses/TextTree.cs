using System;
using System.Collections.Generic;
using Common.Interfaces;

namespace UtilClasses
{
    public class TextTree
    {
        private string Name { get; set; }
        private TextTree? Parent { get; }
        public List<TextTree> Children { get; } = new();
        public TextTree(TextTree parent)
        {
            Name = "";
            Parent = parent;
        }
        public TextTree(string name)
        {
            Parent = null;
            Name = name;
        }

        public TextTree Add(string name, Action<TextTree>? cfg = null)
        {
            var child = new TextTree(this) { Name = name };
            cfg?.Invoke(child);
            Children.Add(child);
            return this;
        }
        public TextTree CreateBranch(string name)
        {
            var child = new TextTree(this) { Name = name };
            Children.Add(child);
            return child;
        }

        public TextTree Add<T>(IEnumerable<T> items, Func<T, string> nameExtractor,  Action<TextTree, T>? cfg = null)
        {
            foreach (var i in items)
            {
                Add(nameExtractor(i), tt => cfg?.Invoke(tt, i));
            }

            return this;
        }

        public TextTree Add<T>(IEnumerable<T> items, Action<TextTree, T>? cfg = null) where T : IHasName =>
            Add(items, i => i.Name, cfg);

        public override string ToString()
        {
            var sb = new IndentingStringBuilder("  ");
            sb.AppendLine(Name);
            Append(sb,"");
            return sb.ToString();
        }

        public void Append(IndentingStringBuilder sb, string prefix)
        {
            for (var i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                string line = "│ ";
                string arrow ="├→";
                if (i == Children.Count - 1)
                {
                    arrow = "└→";
                    line = "  ";
                }
                sb.AppendLine(prefix + arrow + child.Name);
                child.Append(sb, prefix + line);
            }
        }
    }
}
