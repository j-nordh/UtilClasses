using System;
using UtilClasses.Interfaces;
using UtilClasses.Dto;

namespace UtilClasses.Extensions.NameGuids
{
    public static class NameGuidExtensions
    {
        public static NameGuid ToNameGuid<T>(this T src) where T:IHasNameGuid=> new(src);
        public static void SetId<T>(this T o, Guid val) where T : IHasWriteGuid => o.Id = val;
    }
}
