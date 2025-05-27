using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UtilClasses.Core.Extensions.Funcs;
using UtilClasses.Core.Extensions.Enumerables;

namespace UtilClasses.Winforms.Extensions
{
    public static class ListViewItemExtensions
    {
        public static ListViewItem WithTag(this ListViewItem itm, object tag)
        {
            itm.Tag = tag;
            return itm;
        }
        public static IEnumerable<T> GetTags<T>(this ListView.ListViewItemCollection lvic) where T : class => lvic.Cast<ListViewItem>().Select(lvi => lvi.Tag as T);
        public static IEnumerable<T> GetCheckedTags<T>(this ListView.ListViewItemCollection lvic) where T : class => lvic.GetChecked().Select(lvi => lvi.Tag as T);

        public static IEnumerable<ListViewItem> GetChecked(this ListView.ListViewItemCollection lvic) => lvic.Get(lvi => lvi!= null && lvi.Checked);
        public static IEnumerable<ListViewItem> GetSelected(this ListView.ListViewItemCollection lvic) => lvic.Get(lvi => lvi != null && lvi.Selected);
        public static IEnumerable<ListViewItem> Get(this ListView.ListViewItemCollection lvic, Func<ListViewItem, bool> predicate) => lvic.Cast<ListViewItem>().Where(predicate);
        public static IEnumerable<ListViewItem> Get<T>(this ListView.ListViewItemCollection lvic, Func<T, bool> predicate) where T:class
            => lvic.Cast<ListViewItem>().Where(i=>predicate(i.Tag as T));

        public static void UpdateItems<T>(this ListView.ListViewItemCollection lvic, Func<ListViewItem, bool> predicate, Action<T> tagUpdater, Action<ListViewItem, T> itemUpdater) where T : class
        {
            foreach (var i in lvic.Get(predicate))
            {
                var o = i.Tag as T;
                tagUpdater(o);
                itemUpdater(i, o);
            }
        }
        public static Updater Update(this ListView.ListViewItemCollection lvic) => new Updater(lvic);
        //public static Updater<T> Update<T>(this ListView.ListViewItemCollection lvic, Action<T> tagUpdater) where T : class
        //    => lvic.Update().With(tagUpdater);
        //public static void UpdateChecked<T>(this ListView.ListViewItemCollection lvic, Action<T> tagUpdater, Func<ListViewItem, T, ListViewItem> itemUpdater) where T : class
        //    => lvic.Update().Checked().With(tagUpdater).With(itemUpdater);

        //public static void UpdateSelected<T>(this ListView.ListViewItemCollection lvic, Action<T> tagUpdater, Func<ListViewItem, T, ListViewItem> itemUpdater) where T : class
        //=> lvic.Update().Selected().With(tagUpdater).With(itemUpdater);

        public static ListViewItem Set(this ListViewItem lvi, string value, int index=0)
        {
            while (lvi.SubItems.Count <= index+1)
                lvi.SubItems.Add(value);
            lvi.SubItems[index].Text = value;
            return lvi;
        }
        public static ListViewItem Set(this ListViewItem lvi, params string[] values)
        {
            values.ForEach((i, s) => lvi.Set(s, i));
            return lvi;
        }

        public static void AddRange(this ListView.ListViewItemCollection lvic, IEnumerable<ListViewItem> items) => lvic.AddRange(items.ToArray());
        public static void Set(this ListView.ListViewItemCollection lvic, IEnumerable<ListViewItem> items)
        {
            lvic.Clear();
            lvic.AddRange(items.ToArray());
        }
        


        public static IEnumerable<ListViewItem> AllItems(this ListView lv) => lv.Items.Cast<ListViewItem>();
        public static IEnumerable<ListViewItem> ForEachItem(this ListView lv, Action<ListViewItem> op) => lv.Items.Cast<ListViewItem>().ForEach(op);
    }
    public class Updater
    {
        public ListView.ListViewItemCollection Items { get; set; }
        public Func<ListViewItem, bool> Predicate { get; set; }
        public Updater Selected()
        {
            Predicate = i => i.Selected;
            return this;
        }

        public Updater Checked()
        {
            Predicate = i => i.Checked;
            return this;
            }
        public Updater(ListView.ListViewItemCollection items)
        {
            Items = items;
            Predicate = i => true;
        }
        public Updater<T> With<T>(Action<T> updater) where T : class => new Updater<T>(this).With(updater);

        public Updater<T> With<T>(Func<ListViewItem, T, ListViewItem> itemUpdater) where T:class => new Updater<T>(this).With(itemUpdater);
    }
    public class Updater<T> : Updater where T : class
    {

        public Action<T> TagUpdater { get; set; }
        public Action<ListViewItem, T> ItemUpdater { get; set; }

        public Updater(ListView.ListViewItemCollection items) : base(items)
        {
            TagUpdater = o => { };
            ItemUpdater = (i, o) => { };
        }
        public Updater(Updater u) : base(u.Items)
        {
            Predicate = u.Predicate;
        }
        public Updater<T> With(Action<T> updater) => Do(() => TagUpdater = updater);
        public Updater<T> With(Func<ListViewItem, T, ListViewItem> itemUpdater) => Do(() => ItemUpdater = itemUpdater.AsVoid());
        private Updater<T> Do(Action a)
        {
            a();
            return this;
        }
        public void Run()
        {
            foreach (var i in Items.Get(Predicate))
            {
                var o = i.Tag as T;
                if (null == o) continue;
                TagUpdater(o);
                ItemUpdater(i, o);
            }
        }
    }
}
