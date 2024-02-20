using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Json;
using UtilClasses.Extensions.Lists;
using UtilClasses.Extensions.Strings;
using UtilClasses.Nancy.Properties;

namespace UtilClasses.Nancy
{
    public class HtmlRenderer
    {
        private readonly string _defaultTitle;
        private readonly ImageDescriptor _logo;
        private readonly string _style;
        private readonly string _scripts;
        private readonly string _pageTemplate;
        public HtmlRenderer(string defaultTitle, ImageDescriptor logo, string replacePageTemplate=null, string replaceStyle = null, string scripts = null)
        {
            _defaultTitle = defaultTitle;
            _logo = logo;
            _style = replaceStyle ?? Resources.DefaultStyle;
            if (_style.IsNotNullOrEmpty())
                _style = $"<style>{_style}</style>";
            _scripts = scripts;
            _pageTemplate = replacePageTemplate ?? Resources.DefaultPageTemplate;
        }
        public string GetHtml(string template, Dictionary<string, string> replacements) =>
            new KeywordReplacer().With(replacements).Run(template);

        public string RenderPage(PageDescriptor pd)
        {
            var title = pd.Title.IsNullOrEmpty()?_defaultTitle : _defaultTitle + " - " + pd.Title;
            var kr = new KeywordReplacer();
            kr.Add(new List<KeyValuePair<string, string>>
            {
                {"title", title},
                {"css", _style},
                {"logo", _logo?.ToString() },
                {"content", pd.Content},
                {"image", pd.Image?.ToString()},
                {"description", pd.Description },
                {"script", pd.WithScripts && _scripts.IsNotNullOrEmpty() ? _scripts : ""}
            });
            return kr.Run(_pageTemplate);
        }
        public string RenderPage(string title, string content, bool withScripts)
        {
            if (title.IsNullOrEmpty()) title = _defaultTitle;
            else title = _defaultTitle + " - " + title;
            var kr = new KeywordReplacer();
            kr.Add(new List<KeyValuePair<string, string>>
            {
                {"title", title},
                {"style", _style},
                {"content", content},
                {"scripts", withScripts && _scripts.IsNotNullOrEmpty()? _scripts:"" }
            });
            return kr.Run(_pageTemplate);
        }

        public string RenderRedirect(string location)
        {
            var kr = new KeywordReplacer();
            kr.Add("title", _defaultTitle);
            kr.Add("location", location);
            return kr.Run(Resources.RedirectPage);
        }
    }
}
