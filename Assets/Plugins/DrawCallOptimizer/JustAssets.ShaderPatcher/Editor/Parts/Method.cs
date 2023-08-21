using System;
using System.Collections.Generic;
using System.Linq;

namespace JustAssets.ShaderPatcher.Parts
{
    internal class Method : IPart
    {
        private readonly string _lineBreak;

        private readonly int _indentLevel;

        public Method(IReadOnlyDictionary<Type, List<IPart>> declaration, Content body, string lineBreak, int indentLevel) 
        {
            _lineBreak = lineBreak;
            _indentLevel = indentLevel;
            var parts = declaration[typeof(Variable)];
            Modifiers = parts.OfType<Variable>().Take(parts.Count - 2).ToArray();

            if (declaration.ContainsKey(typeof(SyntaxToken)))
            {
                ReturnRegister = (Variable) parts[parts.Count - 1];
                if (parts.Count - 2 >= 0 && parts.Count > parts.Count - 2)
                    ReturnType = (Variable) parts[parts.Count - 2];
                else
                    ReturnType = null;
            }
            else
                ReturnType = (Variable) parts[parts.Count - 1];

            var methodHeader = declaration[typeof(MethodHeader)].FirstOrDefault().Cast<MethodHeader>();
            MethodName = methodHeader.Name;
            Parameters = methodHeader.Parts.Select(Parameter.Create).ToList();
            Body = body;
        }

        public string MethodName { get; set; }

        public Content Body { get; }

        public List<IParameter> Parameters { get; }

        public Variable ReturnType { get; }

        public Variable ReturnRegister { get; }

        public Variable[] Modifiers { get; }

        public string Serialize()
        {
            var indentation = string.Empty.PadLeft(_indentLevel, '\t');
            var result = indentation;

            foreach (Variable modifier in Modifiers)
                result += modifier.Name + " ";

            result += $"{ReturnType?.Name} {MethodName}(";
            result += string.Join(", ", Parameters.Select(x => x.Serialize()));
            result += ")";

            if (ReturnRegister != null)
                result += $" : {ReturnRegister.Name}";

            result += _lineBreak;
            result += indentation + Body.Serialize() + _lineBreak;

            return result;
        }
    }
}