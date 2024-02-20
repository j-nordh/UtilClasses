using System;
using Common.Interfaces;

namespace Common.Dto
{
    public class NameGuid:IHasReadOnlyNameGuid, IEquatable<NameGuid>
    {
        public Guid Id { get; }
        public string Name { get; }

        public NameGuid(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
        public NameGuid(IHasNameGuid src)
        {
            Id = src.Id;
            Name = src.Name;
        }

        public bool Equals(NameGuid other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id) && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NameGuid) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }
    }
}