using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UtilClasses.Core.Extensions.Enumerables;

namespace UtilClasses.Core.Cli;

public class MenuItem
{
    public event Action? PreActivate;
    public event Action? PreMenuDisplay;
    public event Action? PostActivate;
    private List<MenuItem> _children;
    public string? Name { get; set; }

    public IReadOnlyList<MenuItem> Children => _children;

    public Func<MenuItem, Task>? Action { get; set; }
    public object? Tag { get; set; }
    public string? Banner { get; set; }
    public Func<string>? BannerFunc { get; set; }
    public bool ClearScreen { get; set; }
    public bool PauseOnExit { get; set; }
    public MenuItem? Parent { get; set; }
    public char? Key { get; set; }
    public bool UseNumbers { get; set; }
    public bool AllowAll { get; set; }
    public bool Selected { get; set; }
    public bool ExitOnSelection { get; set; }
    public Action<IEnumerable<object>>? AllAction { get; set; }
    public Func<IEnumerable<MenuItem>>? Populator { get; set; }
    public bool IsActive { get; private set; }
    public MenuItem()
    {
        _children = new List<MenuItem>();
        UseNumbers = true;
        AllowAll = true;
        PreActivate += () => IsActive = true;
        PostActivate += () => IsActive = false;
    }
    public MenuItem(string name) : this()
    {
        Name = name;
    }
    public MenuItem(string name, Func<MenuItem, Task> a) : this()
    {
        Name = name;
        Action = a;
    }

    public void AddChild(MenuItem mi)
    {
        _children.Add(mi);
        mi.Parent = this;
    }
    public MenuItem AddChildren(IEnumerable<MenuItem> mis)
    {
        var miList = mis.ToList();
        _children.AddRange(miList);
        miList.ForEach(c => c.Parent = this);
        return this;
    }

    public override string ToString() => Name??"";

    //public static MenuItem Wizard<T1, T2>(string name, Func<T1, T2, Task> f, IEnumerable<T1> items1, IEnumerable<T2> items2, Func<T1, string> formatter1 = null, Func<T2, string> formatter2 = null)
    //{
    //    var ret = new MenuItem(name).With(items1, formatter1);
    //    ret.ForChildren(items2, mi => mi.With(async x => await f((T1)x.Parent.Tag, (T2)x.Tag)), formatter2);
    //    return ret;
    //}
    //public static MenuItem Wizard<T1, T2, T3>(string name, Func<T1, T2, T3, Task> a, IEnumerable<T1> items1, IEnumerable<T2> items2, IEnumerable<T3> items3)
    //{
    //    var ret = new MenuItem(name).With(items1);
    //    ret.ForChildren(items2, mi => mi.ForChildren(items3, _=> mi.Action = x => a((T1)x.Parent.Parent.Tag, (T2)x.Parent.Tag, (T3)x.Tag)));
    //    return ret;
    //}

    protected virtual async Task RunAction()
    {
        if (Action != null) await Action(this);
        if (!PauseOnExit) return;
        Console.Write("Done. Press enter to continue...");
        Console.ReadLine();
    }

    public virtual async Task Activate()
    {
        PreActivate?.Invoke();
        if (null != Populator && !Children.Any())
        {
            _children = Populator().ToList();
        }
        if (!Children.Any())
        {
            await  RunAction();
            PostActivate?.Invoke();
            return;
        }
        while (true)
        {
            PreMenuDisplay?.Invoke();
            var options = GetOptions();

            AddOptions(options);
            options.Add(('x', new MenuItem("Exit")));
            if (ClearScreen)
                Console.Clear();
            if (Banner.IsNotNullOrEmpty())
            {
                Console.WriteLine(Banner);
                Console.WriteLine();
            }

            if (BannerFunc != null)
            {
                Console.WriteLine(BannerFunc());
                Console.WriteLine();
            }
            Console.WriteLine($"{Name}:");
            foreach (var o in options)
            {
                Console.WriteLine(LineRenderer(o.Index, o.MenuItem));
            }

            var a = ConsoleUtil.Prompt("Please select an option: ", options.Select(t => t.Index));
            if (a.Equals('x'))
                break;
            if (await HandleInput(a, options)) break;
        }
        PostActivate?.Invoke();
    }

    protected virtual async Task<bool> HandleInput(char a, List<(char Index, MenuItem MenuItem)> options) =>
        await HandleInput(a, options.First(o => o.Index == a).MenuItem);

    protected virtual async Task<bool> HandleInput(char a, MenuItem mi)
    {
        switch (a)
        {
            case 'a':
                if (null == AllAction)
                    Children.ForEach(c => c.Activate());
                else
                    AllAction(Children.Select(c => c.Tag).Where(o=>o!= null).Select(o=>o!));
                break;
            default:
                await mi.Activate();
                break;
        }
        return ExitOnSelection;
    }

    protected virtual string LineRenderer(char index, MenuItem menuItem) => $"  {index}): {menuItem}";

    protected virtual void AddOptions(List<(char Index, MenuItem MenuItem)> options)
    {
        if (AllowAll && Children.All(c => !c.Children.Any()))
            options.Add(('a', new MenuItem("All")));
    }

    protected virtual List<(char Index, MenuItem MenuItem)> GetOptions()
    {
        var options = new List<(char Index, MenuItem MenuItem)>();
        var i = 0;
        foreach (var child in Children.NotNull())
        {
            var index = child.Key ?? (UseNumbers
                ? (char)('1' + i)
                : (char)('a' + i));
            options.Add((index, child));
            i++;
        }

        return options;
    }
}