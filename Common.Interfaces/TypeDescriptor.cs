using System;

namespace Common.Interfaces
{
    public class TypeDescriptor
    {
        public string Name { get;set; }
        public string Description {get; set; }
        public TypeDescriptor(){}
        public TypeDescriptor(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
    public static class TypeDescriptorExtensions
    {
        public static bool IsSet(this TypeDescriptor td) => 
            null != td 
            && !string.IsNullOrEmpty(td.Name) 
            && !td.Name.Equals("void",StringComparison.OrdinalIgnoreCase);
    }
}