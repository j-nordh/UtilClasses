using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nancy;
using UtilClasses.Extensions.Dictionaries;
using UtilClasses.Extensions.Objects;

namespace UtilClasses.Nancy
{
    public class CommandModule : NancyModule
    {
        private readonly IRendererResolver _renderRes;
        readonly Dictionary<string, Func<Task>> _actions = new Dictionary<string, Func<Task>>(StringComparer.OrdinalIgnoreCase);

        public CommandModule(ICommandResolver res, IRendererResolver renderRes)
        {
            _renderRes = renderRes;
            Get["/do"] = x => Do();
            foreach (var cmd in res.Commands)
            {
                _actions[cmd.Key] = cmd.Action;
            }
        }

        private async Task<Response> Do()
        {
            var action = Request.Query.action as DynamicDictionaryValue;

            var cmd = _actions.Maybe(action);
            if (cmd == null)
                return _renderRes.GetRenderer.RenderPage("Error", "That action is not valid...",false).AsResponse();

            await cmd();
            return _renderRes.GetRenderer.RenderRedirect("/").AsResponse();
        }


    }
    public interface ICommandResolver
    {
        IEnumerable<CommandDescriptor> Commands { get; }
    }

    public class CommandDescriptor
    {
        public string Text { get; }
        public string Key { get; }
        public Func<Task> Action { get; }

        public CommandDescriptor(string text, Func<Task> action, string key = null)
        {
            Text = text;
            Action = action;
            Key = key ?? Guid.NewGuid().ToString("N");
        }

        public static implicit operator HtmlElement(CommandDescriptor c) =>
            c.WhenNotNull(() => HtmlElement.Link(c.Text, $"/do?action={c.Key}"));
    }

    public static class CommandDescriptorExtensions
    {
        public static void Add(this List<CommandDescriptor> lst, string text, Action a, string key = null)
            => lst.Add(new CommandDescriptor(text, ()=>
            {
                a();
                return Task.FromResult(0);
            }, key));
        public static void Add(this List<CommandDescriptor> lst, string text, Func<Task> a, string key = null)
            => lst.Add(new CommandDescriptor(text, () =>
            {
                a();
                return Task.FromResult(0);
            }, key));
        public static void Add(this List<CommandDescriptor> lst, bool doAdd, string text, Action a, string key)
        {
            if (doAdd) lst.Add(text, a, key);
        }
    }
    public interface IRendererResolver
    {
        HtmlRenderer GetRenderer { get; }
    }
}