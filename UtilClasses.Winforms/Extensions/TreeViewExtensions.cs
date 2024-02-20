using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Winforms.Extensions
{
    public static class TreeViewExtensions
    {
        public static IEnumerable<TreeNode> ToTreeNodes(this IEnumerable<string> items) => items.ToTreeNodes(s => s.Select(c => "" + c), s => s);
        public static Node<string> ToTree(this IEnumerable<string> items) => items.ToTree(s => s.Select(c => "" + c), s => s);

        public static void AddRange(this TreeNodeCollection tnc, IEnumerable<TreeNode> nodes) =>
            tnc.AddRange(nodes as TreeNode[] ?? nodes.ToArray());

        public static Node<T> ToTree<T>(this IEnumerable<T> items,
            Func<T, IEnumerable<string>> keyFunc, Func<T, string> nameFunc)
        {
            var root = new Node<T>("");
            foreach (var i in items)
            {
                root.Set(keyFunc(i), i, nameFunc(i));
            }
            return root;
        }


        public static IEnumerable<TreeNode> ToTreeNodes<T>(this IEnumerable<T> items,
            Func<T, IEnumerable<string>> keyFunc, Func<T, string> nameFunc)
        {
            return items.ToTree(keyFunc, nameFunc).GetTree().Nodes.Cast<TreeNode>();
        }

        public class Node<T>
        {
            public List<Node<T>> Children { get; }

            public Node(string key)
            {
                Key = key;
                Children = new List<Node<T>>();
            }

            public string Name { get; set; }
            public T Value { get; set; }
            public string Key { get; }

            public bool Set(IEnumerable<string> keyChain, T value, string name)
            {
                var keys = keyChain as IList<string> ?? keyChain.ToList();
                var key = keys.First();
                if (Key.IsNotNullOrEmpty()) //If I'm not the root element
                {
                    if (!key.Equals(Key)) return false;
                    if (keys.Count == 1) //I'm the correct node, set my values
                    {
                        Value = value;
                        Name = name;
                        return true;
                    }
                    keys = keys.Skip(1).ToList(); //discard first key only if not a root element.
                }
                key = keys.First(); //take the next key (if not root, then it will be the same key once more...)
                if(!Children.Any(c=>c.Key.Equals(key))) Children.Add(new Node<T>(key));
                return Children.Any(c=>c.Set(keys, value, name));
            }

            public TreeNode GetTree()
            {
                var node = new TreeNode(Name) {Tag = this};
                foreach (var c in Children)
                {
                    node.Nodes.Add(c.GetTree());
                }
                return node;
            }
        }
    }
}
