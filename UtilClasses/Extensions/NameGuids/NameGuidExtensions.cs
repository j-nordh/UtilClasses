using Common.Dto;
using Common.Interfaces;

namespace UtilClasses.Extensions.NameGuids
{
    public static class NameGuidExtensions
    {
        public static NameGuid ToNameGuid<T>(this T src) where T:IHasNameGuid=> new(src);
    }
}
