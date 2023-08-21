using System;
using System.Collections.Generic;
using System.Linq;

namespace JustAssets.ShaderPatcher.Parts
{
    public class Struct : IPart
    {
        private readonly string _lineBreak;

        private readonly int _indentLevel;

        public Struct(Dictionary<Type, List<IPart>> declaration, string methodBodyText, List<string> macroWords, string lineBreak, int indentLevel)
        {
            _lineBreak = lineBreak;
            _indentLevel = indentLevel;

            var structName = declaration[typeof(Variable)][1] as Variable;
            Name = structName.Name;

            var operationSpan = new StringSpan(methodBodyText);
            operationSpan.ReadWhile(char.IsWhiteSpace).Without("\r\n");
            var parts = CGCommandParser.Parse(operationSpan, indentLevel+1, lineBreak, macroWords).ToList();
            Parameters = parts.Select(Parameter.Create).ToList();
        }

        public Struct(string lineBreak, int indentLevel, string name, List<IParameter> parameters)
        {
            _lineBreak = lineBreak;
            _indentLevel = indentLevel;
            Name = name;
            Parameters = parameters;
        }

        public string Name { get; }
        
        public List<IParameter> Parameters { get; }

        public string Serialize()
        {
            var indentation = string.Empty.PadLeft(_indentLevel, '\t');
            var indentationInner = string.Empty.PadLeft(_indentLevel+1, '\t');
            var result = indentation;

            result += "struct " + Name + _lineBreak;
            result += indentation + "{" + _lineBreak;

            foreach (var parameter in Parameters)
            {
                if (parameter is Parameter)
                    result += $"{indentationInner}{parameter.Serialize()};{_lineBreak}";
                else
                    result += parameter.Serialize() + _lineBreak;
            } 

            result += indentation + "};" + _lineBreak;

            
            return result;
        }
    }
}