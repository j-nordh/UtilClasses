using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Tasks;

namespace UtilClasses.Cli
{
    public static class MenuItemExtensions
    {
        public static void Add(this List<MenuItem> lst, string name) => lst.Add(new MenuItem(name));

        public static void Add(this List<MenuItem> lst, string name, Func<MenuItem, Task> a) =>
            lst.Add(new MenuItem(name) { Action = a });

        public static void Add(this List<MenuItem> lst, string name, Func<MenuItem, Task> a,
            IEnumerable<(string Name, object Tag)> subItems) =>
            lst.Add(new MenuItem(name).With(subItems.Select(t => new MenuItem(t.Name) { Action = a, Tag = t.Tag }).ToList()));

        public static MenuItem With<T>(this MenuItem mi, params (string Name, T Tag)[] subItems) =>
            mi.With(subItems.AsEnumerable());
        public static MenuItem With<T>(this MenuItem mi, IEnumerable<(string Name, T Tag)> subItems)
        {
            mi.AddChildren(subItems.Select(t => new MenuItem(t.Name) { Action = mi.Action, Tag = t.Tag }));
            return mi;
        }
        public static MenuItem WithTag(this MenuItem mi, object tag)
        {
            mi.Tag = tag;
            return mi;
        }
        public static MenuItem WithAction<T>(this MenuItem mi, Action<T> a)
        {
            var b = new MenuBuilder(mi);
            b.WithAction(a);
            return mi;
        }
        public static MenuItem WithAction<T>(this MenuItem mi, Func<T, Task> a)
        {
            var b = new MenuBuilder(mi);
            b.WithAction(a);
            return mi;
        }

        public static MenuItem With(this MenuItem mi, IEnumerable<MenuItem> subItems)
        {
            mi.AddChildren(subItems);
            return mi;
        }

        //public static MenuItem ForChildren<T>(this MenuItem mi, IEnumerable<T> items, Action<MenuItem> a, Func<T, string> formatter = null)
        //{
        //    var itemList = items.ToList();
        //    foreach (var child in mi.Children)
        //    {
        //        child.With(itemList, formatter);
        //        foreach (var grandChild in child.Children)
        //            a(grandChild);
        //    }

        //    return mi;
        //}


    }

    public class MenuBuilder
    {
        public MenuItem Itm { get; }

        public MenuBuilder()
        {
            Itm = new MenuItem();
        }
        public MenuBuilder(string name)
        {
            Itm = new MenuItem(name);
        }
        public MenuBuilder(MenuItem itm)
        {
            Itm = itm;
        }

        private object Tag => Itm.Tag;


        public MenuBuilder WithName(string name) => Do(i => i.Name = name);
        private MenuBuilder Do(Action<MenuItem> a)
        {
            a(Itm);
            return this;
        }

        private MenuBuilder AddChild(string name, Action<MenuBuilder>? a = null)
        {
            var c = new MenuItem(name);
            Itm.AddChild(c);
            a?.Invoke(new MenuBuilder(c));
            return this;
        }

        public MenuBuilder Add(params MenuBuilder[] mbs)
        {
            Itm.AddChildren(mbs.Select(mb => mb.Itm));
            return this;
        }

        public MenuBuilder Add(string name, Action<MenuBuilder> a) => AddChild(name, a);
        public MenuBuilder Add<T>(params (string Name, T Tag)[] subItems) =>
            Add(subItems.AsEnumerable());
        public MenuBuilder Add<T>(IEnumerable<(string Name, T Tag)> subItems)
        {
            Itm.AddChildren(subItems.Select(t => new MenuItem(t.Name) { Action = Itm.Action, Tag = t.Tag }));
            return this;
        }
        public MenuBuilder AddMultiple<T>(IEnumerable<T> subItems, Func<T, string>? formatter = null)
        {
            formatter ??= o => o?.ToString() ?? "";
            Itm.AddChildren(subItems.Select(t => new MenuItem(formatter(t)) { Action = Itm.Action, Tag = t }));
            return this;
        }
        public MenuBuilder AddMultiple(IEnumerable<MenuBuilder> subItems)
        {
            Itm.AddChildren(subItems.Select(mb => mb.Itm));
            return this;
        }
        public MenuBuilder AddMultiple<T>(IEnumerable<T> subItems, Action<MenuBuilder> cfg, Func<T, string>? formatter = null)
        {
            formatter ??= o => o?.ToString()??"";
            foreach (var o in subItems)
            {
                var mb = new MenuBuilder(formatter(o)).WithTag(o);
                cfg?.Invoke(mb);
                Itm.AddChild(mb.Itm);
            }
            return this;
        }

        public MenuBuilder WithTag<T>(T o) => Do(mi => mi.Tag = o);

        public static MenuBuilder Select<T>(string name, IEnumerable<T> values, Func<T, string> namer, Action<MenuBuilder>? cfg = null) =>
            new MenuBuilder(new SelectMenuItem(name)).WithChildren(values, namer, cfg).WithClearScreen();

        //private MenuBuilder Add<T>(string name, Func<T, Task> a) =>   Do(m=>m.AddChild(new MenuItem(name, mi => a((T)mi.Tag))));
        public MenuBuilder Add<T>(string name, Func<T, Task> a) => AddChild(name, i => a((T)i.Tag));
        public MenuBuilder Add(bool predicate, string name, Action a)
        {
            if (!predicate) return this;
            return AddChild(name, b => b.WithAction(a));
        }

        public MenuBuilder Add(string name, Action a) => AddChild(name, b => b.WithAction(a));
        public MenuBuilder Add(string name, Func<Task> f) => AddChild(name, b => b.WithActionAsync(f));
        public MenuBuilder Add(bool predicate, string name, Func<Task> f) => predicate ? Add(name, f) : this; 
        public MenuBuilder Add(string name, Func<Task> f, Action<MenuBuilder> cfg) => AddChild(name, b =>
        {
            b.WithActionAsync(f);
            cfg?.Invoke(b);
        });
        public MenuBuilder Add(string name, Action a, Action<MenuBuilder> cfg) => AddChild(name, b =>
        {
            b.WithAction(a);
            cfg?.Invoke(b);
        });

        public MenuBuilder AddNoPause(string name, Func<Task> f, Action<MenuBuilder>? cfg = null) => AddChild(name, b =>
        {
            b.WithActionAsync(f).WithNoPauseOnExit();
            cfg?.Invoke(b);
        });
        public MenuBuilder Add(string name) => AddChild(name);

        public MenuBuilder AddWithChildren<T>(string name, Action<T> a, IEnumerable<T> os, Func<T, string>? formatter = null) =>
            AddWithChildren(name, os, formatter).WithAction(a);
        public MenuBuilder AddWithChildren<T>(string name, Func<T, Task> f, IEnumerable<T> os, Func<T, string>? formatter = null) =>
            AddWithChildren(name, os, formatter).WithActionAsync(f);
        public MenuBuilder AddWithChildren<T>(string name, IEnumerable<T> os, Func<T, string>? formatter = null) =>
            AddChild(name, b => b.WithChildren(os, formatter));

        public MenuBuilder AddWithPopulator<T>(string name, Action<T> a, Func<IEnumerable<T>> pop, Func<T, string>? formatter = null, Action<MenuBuilder>? config = null)
        {
            formatter ??= o => o?.ToString() ?? "";
            return AddChild(name, b =>
            {
                b
                    .WithPopulator(pop, formatter)
                    .Itm.Populator = () => pop().Select(o => new MenuItem(formatter(o)).WithTag(o).WithAction(a));

                config?.Invoke(b);
            });
        }

        public MenuBuilder AddWithPopulator<T>(string name, Func<T, Task> a, Func<IEnumerable<T>> pop, Func<T, string>? formatter = null, Action<MenuBuilder>? config = null)
        {
            formatter ??=  o => o?.ToString() ?? "";
            return AddChild(name, b =>
            {
                b
                    .WithPopulator(pop, formatter)
                    .Itm.Populator = () => pop().Select(o => new MenuItem(formatter(o)).WithTag(o).WithAction(a));

                config?.Invoke(b);
            });
        }

        public static MenuBuilder Wizard<T1, T2>(string name, Func<T1, T2, Task> f, IEnumerable<T1> items1, IEnumerable<T2> items2, Func<T1, string>? formatter1 = null, Func<T2, string>? formatter2 = null)
        {
            var ret = new MenuBuilder(name).WithChildren(items1, formatter1);
            ret.OnChildren(c => c.WithChildren(items2, formatter2, mi => mi.WithActionAsync(async x => await f((T1)x.Parent.Tag, (T2)x.Tag))));
            return ret;
        }
        public static MenuBuilder Wizard<T1, T2, T3>(string name, Func<T1, T2, T3, Task> a, IEnumerable<T1> items1, IEnumerable<T2> items2, IEnumerable<T3> items3,
            Func<T1, string>? formatter1 = null, Func<T2, string>? formatter2 = null, Func<T3, string>? formatter3 = null)
        {
            var ret = new MenuBuilder(name).WithChildren(items1, formatter1);
            ret.OnChildren(c => c.WithChildren(items2, formatter2).OnChildren(gc => gc.WithChildren(items3, formatter3))
                .OnChildren(ggc => ggc.WithActionAsync(
                    x => a((T1)x.Parent.Parent.Tag, (T2)x.Parent.Tag, (T3)x.Tag))));

            return ret;
        }

        public MenuBuilder WithKey(char? key) => Do(i => i.Key = key);
        public MenuBuilder WithAll(bool withAll = true) => Do(i => i.AllowAll = withAll);
        public MenuBuilder WithBannerFunc(Func<string> bf) => Do(i => i.BannerFunc = bf);
        public MenuBuilder WithExitOnSelection(bool val = true) => Do(i => i.ExitOnSelection = val);
        public MenuBuilder WithClearScreen(bool val = true) => Do(i => i.ClearScreen = val);
        public MenuBuilder WithSelected(bool val = true) => Do(i => i.Selected = val);
        public MenuBuilder WithNoPauseOnExit() => Do(i => i.PauseOnExit = false);

        public MenuBuilder WithAllAction<T>(Action<IEnumerable<T>> a) => Do(i =>
        {
            i.AllAction = os => a(os.Cast<T>());
            if (null == a) i.AllowAll = false;
        });

        public MenuBuilder WithActionAsync<T1, T2>(Func<T1, T2, Task> a) =>
            Do(i => i.Action = _ => a((T1)i.Parent.Tag, (T2)i.Tag));

        public MenuBuilder WithAction(Action a) => Do(i => i.Action = _ => a.RunAsTask());
        public MenuBuilder WithAction(Func<Task> a) => Do(i => i.Action =_=> a());
        public MenuBuilder WithAction<T>(Action<T> a) => Do(mi => mi.Action = async i => await Task.Run(() => a((T)i.Tag)));
        public MenuBuilder WithAction<T>(Func<T, Task> a) => Do(mi => mi.Action = async i => await a((T)i.Tag));
        public MenuBuilder WithPopulator<T>(Func<IEnumerable<T>> pop, Func<T, string>? formatter = null)
        {
            formatter ??= o => o?.ToString() ?? "";
            Itm.Populator = () => pop().Select(o => new MenuItem(formatter(o)).WithTag(o));
            return this;
        }

        public MenuBuilder WithActionAsync(Func<Task> f) => Do(i => i.Action = _ => f());

        public MenuBuilder WithActionAsync(Func<MenuItem, Task> a) => Do(i => i.Action = a);
        public MenuBuilder WithActionAsync<T>(Func<T, Task> a) => Do(mi => mi.Action = async i => await Task.Run(() => a((T)i.Tag)));
        public MenuBuilder WithChildren<T>(IEnumerable<T> os, Func<T, string>? formatter = null, Action<MenuBuilder>? cfg = null)
        {
            formatter ??= o => o?.ToString() ?? "";
            foreach (var o in os)
            {
                AddChild(formatter(o), cfg);
            }
            return this;
        }
        public MenuBuilder OnChildren(Action<MenuBuilder> cfg) => Do(i => i.Children.ForEach(c => cfg(new MenuBuilder(c))));
    }
}