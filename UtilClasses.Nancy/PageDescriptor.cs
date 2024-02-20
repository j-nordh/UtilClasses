using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilClasses.Extensions.Strings;
using UtilClasses.Nancy.Properties;

namespace UtilClasses.Nancy
{
    public class PageDescriptor
    {
        public string Title { get; set; }
        public ImageDescriptor Image;
        public string Description;
        public string Content;
        public bool WithScripts =true;

        //public override string ToString()
        //    => new KeywordReplacer()
        //        .Add("logo", ImageDescriptor.Logo.ToString())
        //        .Add("title", "Hogia TOS Notification Engine" + (Title.IsNullOrEmpty() ? "" : " - " + Title))
        //        .Add("css", $"<style>\r\n{Resources.Css}\r\n</style>\r\n")
        //        .Add("image", Image?.ToString() ?? "")
        //        .Add("description", Description)
        //        .Add("content", Content)
        //        .Run(Resources.DefaultPageTemplate);
    }
}
