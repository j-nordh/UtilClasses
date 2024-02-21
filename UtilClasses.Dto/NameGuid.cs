using System;
using UtilClasses.Interfaces;

namespace UtilClasses.Dto
{
    public class NameGuid : IHasNameGuid
    {
        public NameGuid(string name, Guid id)
        {
            Name = name;
            Id = id;
        }
        public NameGuid(IHasNameGuid obj)
        {
            Id = obj.Id;
            Name = obj.Name;
        }
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
