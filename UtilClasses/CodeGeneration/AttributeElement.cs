using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.CodeGeneration
{
    public class AttributeElement : SimpleClassBuilder
    {
        private readonly List<string> _requires;
        private readonly List<string> _constructorParameters;
        private readonly string _pType;
        private readonly string _pName;
        private readonly string _propName;

        public override IEnumerable<string> Implements => new[] { "Attribute" };
        public override IEnumerable<string> Requires => _requires;
        public AttributeElement(string name) : base(name + "Attribute")
        {
            _requires = new List<string> { "System" };
            _constructorParameters = new List<string>();
        }
        public AttributeElement(string name, string pType, string pName) : base(name + "Attribute")
        {
            _requires = new List<string> { "System" };
            _constructorParameters = new List<string> { $"{pType} {pName}" };
            _pType = pType;
            _pName = pName;
            _propName = _pName.MakeIt().PascalCase();
        }
        protected override void Preamble(IndentingStringBuilder sb)
        {
            base.Preamble(sb);
            if (_pType.IsNullOrWhitespace()) return;
            sb.AppendLine($"public {_pType} {_propName} {{ get; }}");
        }
        protected override void ConstructorBody(IndentingStringBuilder sb)
        {
            base.ConstructorBody(sb);
            if (_pType.IsNullOrWhitespace()) return;
            sb.AppendLine($"{_propName} = {_pName};");
        }
        public override IEnumerable<string> ConstructorParameters => _constructorParameters;
    }
}