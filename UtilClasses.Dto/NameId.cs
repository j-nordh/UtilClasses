using UtilClasses.Interfaces;

namespace UtilClasses.Dto
{
    public class NameId : IHasLongNameId
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }
}
