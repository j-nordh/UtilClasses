using UtilClasses.Extensions.Strings;

namespace UtilClasses.Nancy
{
    public class ImageDescriptor
    {
        public string Url { get; set; }
        public string Alt { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }

        public override string ToString()
        {
            var alt = Alt.IsNullOrEmpty() ? "" : $" alt=\"{Alt}\"";
            var width = null == Width ? "" : $" width=\"{Width}\"";
            var height = null == Height ? "" : $" height=\"{Height}\"";
            return $"<img src=\"{Url}\"{alt}{width}{height}/><br/>";
        }

        public static ImageDescriptor Logo => new ImageDescriptor()
        {
            Url = "/img/RiseLogo.png",
            Alt = "The RISE logotype",
            Width = 81,
            Height = 104
        };
    }
}
