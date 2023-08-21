using System.Collections.Generic;
using System.Linq;

namespace JustAssets.ShaderPatcher.Parts
{
    public class Parameter : IParameter
    {
        public Variable Type { get; }

        public Variable Register { get; set; }

        public Variable Name { get; }

        public Numeric ArraySize { get; }

        public List<string> Modifiers { get; }

        private Parameter(ICollectionPart part)
        {
            var parts = part.Parts.Where(x=> !(x is CommandEndToken)).ToList();

            var registerAssignmentIndex = parts.FindIndex(x => x is SyntaxToken st && st.Token == ":");

            if (registerAssignmentIndex < 0)
                registerAssignmentIndex = parts.Count;

            var beforeAssignment = parts.Take(registerAssignmentIndex).ToList();
            var afterAssignment = parts.Skip(registerAssignmentIndex + 1).ToList();

            if (beforeAssignment.Any(x => x is SyntaxToken st && st.Token == "[") && beforeAssignment.Any(x => x is SyntaxToken st && st.Token == "]"))
            {
                var findIndex = beforeAssignment.FindIndex(x => x is SyntaxToken st && st.Token == "[");
                var arraySize = beforeAssignment[findIndex + 1];
                ArraySize = (Numeric) arraySize;
                beforeAssignment.RemoveRange(findIndex, 3);
            }

            if (beforeAssignment.Count >= 2)
            {
                Type = beforeAssignment[beforeAssignment.Count - 2] as Variable;
                Name = beforeAssignment[beforeAssignment.Count - 1] as Variable;
                Modifiers = new List<string>(beforeAssignment.Take(beforeAssignment.Count - 2).Select(x=>x.Serialize()));
            }

            if (afterAssignment.Count >= 1)
            {
                Register = (Variable) afterAssignment[0];
            }
        }

        public Parameter(Variable type, Variable register, Variable name, List<string> modifiers)
        {
            Type = type;
            Register = register;
            Name = name;
            Modifiers = modifiers ?? new List<string>();
        }

        public string Serialize()
        {
            var result = "";

            foreach (var modifier in Modifiers)
                result += $"{modifier} ";

            result += $"{Type?.Name} {Name?.Name}";

            if (ArraySize != null)
                result += $"[{ArraySize.Number}]";

            if (Register != null)
                result += $" : {Register.Name}";

            return result;
        }

        public static IParameter Create(IPart part)
        {
            if (!(part is ICollectionPart partList))
                return new UnparsedParameter(part);

            if (partList.Parts.Count > 0 && partList.Parts[0] is Untouched firstPart && firstPart.Content.StartsWith("#"))
                    return new InlineDefine(part);
            
            var parameter = new Parameter(partList);

            if (parameter.Name != null)
                return parameter;

            return new UnparsedParameter(part);
        }
    }
}