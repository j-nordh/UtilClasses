using System.Collections.Generic;
using System.Threading.Tasks;
using UtilClasses.Core.Extensions.Enumerables;

namespace UtilClasses.Core.Cli;

public class SelectMenuItem : MenuItem
{

    public SelectMenuItem()
    {
        ClearScreen = true;
    }
    public SelectMenuItem(string name) : base(name)
    {

    }
    protected override string LineRenderer(char index, MenuItem menuItem)
    {
        if (index.Equals('x')) return base.LineRenderer(index, menuItem);
        var sel = menuItem.Selected ? "[X] " : "[ ] ";
        return $"  {index}): {sel}{menuItem}";
    }

    protected override void AddOptions(List<(char Index, MenuItem MenuItem)> options) => options.AddRange(
        new[] { ('a', new MenuItem("All")), ('n', new MenuItem("None")) });

    protected override Task<bool> HandleInput(char a, MenuItem mi)
    {

        switch (a)
        {
            case 'a':
                Children.ForEach(c => c.Selected = true);
                break;
            case 'n':
                Children.ForEach(c => c.Selected = false);
                break;
            default:
                mi.Selected = !mi.Selected;
                break;
        }

        return Task.FromResult(false);
    }
}