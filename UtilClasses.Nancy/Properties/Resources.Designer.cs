﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UtilClasses.Nancy.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("UtilClasses.Nancy.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;!doctype html&gt;
        ///&lt;html lang=&quot;en&quot;&gt;
        ///	&lt;head&gt;
        ///		&lt;meta charset=&quot;UTF-8&quot;&gt;
        ///			&lt;title&gt;%title%&lt;/title&gt;
        ///%css%
        ///%script%
        ///	&lt;/head&gt;
        ///	&lt;body&gt;
        ///		&lt;div class=&quot;container&quot;&gt;
        ///			%logo%
        ///			&lt;br /&gt;
        ///			&lt;div class=&quot;content&quot;&gt;&lt;h1&gt;%title%&lt;/h1&gt;
        ///				%image%
        ///				%description%
        ///				%content%
        ///		&lt;/div&gt;
        ///	&lt;/body&gt;
        ///&lt;/html&gt;.
        /// </summary>
        internal static string DefaultPageTemplate {
            get {
                return ResourceManager.GetString("DefaultPageTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to div.container { text-align: center; padding-top: 100px;}
        ///div.content { margin-top: 20px; display: inline-block; width: 625px; border-radius: 25px; background-color: #ffdd00; color: #111111; padding: 10px; }
        ///div.content h1 { margin-top: 0; }
        ///div.content.smaller { width:530px; font-size: medium; }
        ///div.column{ width:33%; float: left; text-align: left; font-size: small; }
        ///.caption { font-weight=bolder; font-size: x-large; width: 100%; text-align: center; }
        ///.actionName { font-weight:bolder; font-size:large [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string DefaultStyle {
            get {
                return ResourceManager.GetString("DefaultStyle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;!doctype html&gt;
        ///&lt;html lang=&quot;en&quot;&gt;
        ///&lt;head&gt;
        ///	&lt;meta charset=&quot;UTF-8&quot;&gt;
        ///	&lt;title&gt;%title%&lt;/title&gt;
        ///	&lt;script&gt;
        ///		window.onload = function() {
        ///  window.location.replace(&quot;%location%&quot;);
        ///};
        ///	&lt;/script&gt;
        ///&lt;/head&gt;
        ///&lt;body&gt;
        ///&lt;/body&gt;
        ///&lt;/html&gt;.
        /// </summary>
        internal static string RedirectPage {
            get {
                return ResourceManager.GetString("RedirectPage", resourceCulture);
            }
        }
    }
}