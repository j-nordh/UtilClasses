using System.Collections.Generic;
using System.Linq;
using UtilClasses.Extensions.Enumerables;
using UtilClasses.Extensions.Strings;

namespace UtilClasses.CodeGeneration
{
    public class ClassBuilder : ICodeElement
    {
        public string Name { get; set; }
        private List<IInjector> injectors = new List<IInjector>();
        public bool IsStatic { get; set; }
        public string AccessModifier { get; set; }

        public IEnumerable<string> Requires => injectors.SelectManyStripNull(i => i.Using);

        public override string ToString()
        {
            var sb = new IndentingStringBuilder("\t").AutoIndentOnCurlyBraces();
            AppendTo(sb);
            return sb.ToString();
        }
        public ClassBuilder Inject(IInjector i)
        {
            injectors.Add(i);
            return this;
        }

        public void AppendTo(IndentingStringBuilder sb)
        {
            var inheriting = injectors.SingleOrDefault(i => i.Inherits != null);
            var implements = injectors.SelectManyStripNull(i => i.Implements).Distinct().AsSorted();
            if (inheriting != null)
                implements.Insert(0, inheriting.Inherits);
            var ii = !implements.Any() ? "" : $": {implements.Join(", ")}";
            var baseInitializer = inheriting == null ? "" : $": base({inheriting.BaseConstructorArguments})";

            sb
                .AppendLine($"{AccessModifier} {(IsStatic ? "static " : "")}class {Name}{ii}")
                .AppendLine("{")
                .AppendLines(injectors.SelectManyStripNull(i => i.Fields))
                .AppendLine()
                .Maybe(IsStatic && injectors.SelectMany(i => i.ConstructorArgs).NotNull().Any(), () => sb
                    .AppendLines($"static {Name}()", $"public {Name}({injectors.SelectManyStripNull(i => i.ConstructorArgs).Join(", ")}){baseInitializer}", "{")
                    .Inject(injectors, i => i.Constructor)
                    .AppendLine("}"))
                .Inject(injectors, i => i.Methods)
                .Inject(injectors, i => i.SubClasses)
                .AppendLine("}");
        }
    }
}