using System;
using System.Collections.Generic;
using System.Linq;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.Plugins
{
    [Serializable]
    public class MetaData
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    [Serializable]
    public class MetaDataWithParameters : MetaData
    {
        public List<ParameterMetaData> Parameters { get; set; }
        public MetaDataWithParameters()
        {
            Name = "";
            Description = "";
            Parameters = new List<ParameterMetaData>();
        }
    }
    [Serializable]
    public class ParameterMetaData : MetaData
    {
        public bool Mandatory { get; set; }
    }
    [Serializable]
    public class ValidatingParameterMetaData: ParameterMetaData
    {
        private readonly Func<string, string> _validatorFunc;

        public ValidatingParameterMetaData(Func<string, string> validatorFunc)
        {
            _validatorFunc = validatorFunc;
        }
        
        public string Validate(string value)
        {
            var res = _validatorFunc(value);
            return string.IsNullOrEmpty(res) ? res : $"{Name}: {res}";
        }

        public static ValidatingParameterMetaData FromEnum<T>(string name, string description, bool mandatory) where T : struct
        {
            return new ValidatingParameterMetaData(Validators.IsEnum<T>)
            {
                Name = name,
                Description = description + " Valid values: " + string.Join(", ", Enum.GetNames(typeof(T))),
                Mandatory = mandatory
            };
        }

        public ParameterMetaData ToParameterMetaData() =>
            new ParameterMetaData() {Name = Name, Description = Description, Mandatory = Mandatory};
    }

    public static class ValidatingParameterMetaDataExtensions
    {
        public static void Add(this List<ValidatingParameterMetaData> lst, string name, string description,
            bool mandatory, Func<string, string> validatingFunc)
        {
            lst.Add(new ValidatingParameterMetaData(validatingFunc) {Name = name, Description = description, Mandatory = mandatory});
        }

        public static void Add(this List<ValidatingParameterMetaData> lst, string name, string description,
            Func<string, string> validatingFunc) => lst.Add(name, description, false, validatingFunc);

        public static void Add(this List<ValidatingParameterMetaData> lst, string name, string description) =>
            lst.Add(name, description, false, v => null);
        public static void Add<T>(this List<ValidatingParameterMetaData> lst, string name, string description, bool mandatory, T dummy) where T:struct=>
            lst.Add(ValidatingParameterMetaData.FromEnum<T>(name,description,mandatory));

        public static void Assert(this List<ValidatingParameterMetaData> lst, Dictionary<string, string> ps)
        {
            var errors = lst.Validate(ps).Where(s=>!string.IsNullOrWhiteSpace(s)).ToList();
            if (!errors.Any()) return;
            throw new Exception("Validation errors raised:\r\n" + string.Join("\r\n",errors));
        }

        public static IEnumerable<string> Validate(this List<ValidatingParameterMetaData> lst,
            Dictionary<string, string> ps)
        {
            foreach (var param in lst.Where(p => p.Mandatory))
            {
                if (!ps.ContainsKey(param.Name))
                {
                    yield return $"The mandatory parameter {param.Name} is not supplied.";
                }
            }
            foreach (var param in lst.Where(p => ps.ContainsKey(p.Name)))
            {
                yield return param.Validate(ps[param.Name]);
            }
        }
    }

    public static class Validators
    {
        public static string IsGuid(string v)
        {
            Guid g;
            return Guid.TryParse(v, out g) ? null : $"The provided value ({v}) could not be parsed as a guid.";
        }

        public static string IsLongerThan(string v, int limit) =>
            (v?.Length ?? 0) > limit ? "" : $"The provided value ({v}) seems way too short to be a valid token.";

        public static string IsEnum<T>(string s) where T : struct
        {
            T v;
            return Enum.TryParse(s, true, out v)
                ? ""
                : $"The provided value ({v}) could not be parsed as a {typeof(T).Name}.";
        }

        public static string IsInt(string s) => s.IsInt() ? "" : $"The value provided({s}) is not an integer.";
    }

    public static class VPMDExtensions
    {
        public static ParameterMetaData ToDTO(this ValidatingParameterMetaData v) => new ParameterMetaData { Name = v.Name, Description = v.Description };
    }
}
