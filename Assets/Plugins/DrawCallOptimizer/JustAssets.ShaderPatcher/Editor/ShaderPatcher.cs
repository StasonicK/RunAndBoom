using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JustAssets.ShaderPatcher.Parts;
using JustAssets.ShaderPatcher.Segments;

namespace JustAssets.ShaderPatcher
{
    public partial class ShaderPatcher
    {
        private static int _atlasChannel = 2;

        private ShaderParser _shaderParser;

        private readonly Action<int, string> _log;

        public ShaderPatcher(ShaderParser shaderParser, Action<int, string> log, int atlasChannel = 2)
        {
            _shaderParser = shaderParser;
            _log = log;
            _atlasChannel = atlasChannel;
        }

        public string AddAtlasRegisterIfMissing(string returnVariableType, List<IParameter> methodParameters, List<Struct> list)
        {
            var atlasRegisterName = "atlas";
            if (TryFindTexCoord(_atlasChannel, methodParameters, out var parameterPath, atlasRegisterName, list))
                return parameterPath;

            var atlasTexCoord = $"TEXCOORD{_atlasChannel}";
            if (methodParameters.OfType<Parameter>().All(x => x.Register != null))
            {
                methodParameters.Add(new Parameter(new Variable("float4"), new Variable(atlasTexCoord),
                    new Variable("atlas"), null));
            }
            else
            {
                Struct structSegment = list.FirstOrDefault(x => x.Name == returnVariableType);

                if (TryFindTexCoord(_atlasChannel, methodParameters, out var path, atlasRegisterName, list))
                    return path;

                if (structSegment != null)
                {
                    var newParam = new Parameter(new Variable("float4"), new Variable(atlasTexCoord),
                        new Variable("atlas"), null);
                    InjectParameter(structSegment.Parameters, newParam);
                }
                else
                {
                    _log(2, $"Cannot find struct '{returnVariableType}' to patch in atlas variable.");
                }
            }

            if (TryFindTexCoord(_atlasChannel, methodParameters, out var buildPath, atlasRegisterName, list))
                return buildPath;

            return parameterPath;
        }

        public Dictionary<string, (string Name, int Component)> PatchInAtlasSupport()
        {
            RenameShader();

            var replacements = ConvertScalarPropertiesToTextures();
            AddAtlasVariables();
            InjectCode(InjectAtlasMappingMethods);

            var fragmentProgramNames = FindProgramNames("fragment").Union(FindProgramNames("surface")).ToList();
            var vertexProgramNames = FindProgramNames("vertex").Union(FindProgramNames("surface", "vertex")).ToList();

            var fragmentPrograms = fragmentProgramNames.Select(FindProgram).Where(x => x != null).ToList();
            var vertexPrograms = vertexProgramNames.Select(FindProgram).Where(x => x != null).ToList();

            if (vertexPrograms.Count == 0)
            {
                AddVertexProgramIfMissing(fragmentPrograms);
                vertexProgramNames = FindProgramNames("vertex").Union(FindProgramNames("surface", "vertex")).ToList();
                vertexPrograms = vertexProgramNames.Select(FindProgram).Where(x => x != null).ToList();
            }

            PatchVertexPrograms(vertexPrograms);
            PatchFragmentPrograms(replacements, fragmentPrograms);
            PatchTex2DUsages();

            return replacements.ToDictionary(dict => dict.Key, dict => (dict.Value.Name, (int) dict.Value.Component));
        }

        public static bool TryFindTexCoord(int texCoord, List<IParameter> parameters, out string result, string coordName, List<Struct> structs)
        {
            foreach (IParameter genericParameter in parameters)
            {
                if (!(genericParameter is Parameter parameter))
                    continue;

                if (parameter.Register?.Name != null && (coordName == null || parameter.Name.Name == coordName))
                {
                    result = parameter.Name.Name;
                    return true;
                }

                Struct structData = structs.FirstOrDefault(x => x.Name == parameter.Type.Name);

                if (structData == null)
                    continue;

                for (var index = 0; index < structData.Parameters.Count; index++)
                {
                    var newParameter = structData.Parameters[index] as Parameter;

                    if (newParameter == null)
                        continue;

                    if (newParameter.Register?.Name != null && (coordName == null || newParameter.Name.Name.StartsWith(coordName)))
                    {
                        result = $"{parameter.Name.Name}.{newParameter.Name.Name}";
                        return true;
                    }

                    if (newParameter.Register == null)
                        continue;

                    if (index != texCoord)
                        continue;

                    if (coordName != null && !newParameter.Name.Name.StartsWith(coordName))
                        continue;

                    result = $"{parameter.Name.Name}.{newParameter.Name.Name}";
                    return true;
                }

                foreach (IParameter x in structData.Parameters)
                {
                    if (x is Parameter p && p.Name.Name.StartsWith(coordName))
                    {
                        result = $"{parameter.Name.Name}.{p.Name.Name}";
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        private string AddAtlasRegisterIfMissing(string returnVariableType, List<IParameter> methodParameters)
        {
            var list = _shaderParser.Parsed.OfType<CGSegment>().SelectMany(x => x.Lines.OfType<Struct>()).ToList();
            return AddAtlasRegisterIfMissing(returnVariableType, methodParameters, list);
        }

        private void AddAtlasVariables()
        {
            var index = _shaderParser.Parsed.FindLastIndex(x => x is PropertySegment);

            if (index >= 0)
            {
                var lastEntry = _shaderParser.Parsed[index].Cast<PropertySegment>();

                _shaderParser.Parsed.Insert(index + 1,
                    new PropertySegment(lastEntry.LineBreak, lastEntry.Indent, "Atlas Width", "2048", "float", "_AtlasWidth", ""));
                _shaderParser.Parsed.Insert(index + 1,
                    new PropertySegment(lastEntry.LineBreak, lastEntry.Indent, "Atlas Height", "2048", "float", "_AtlasHeight", ""));
                _shaderParser.Parsed.Insert(index + 1,
                    new PropertySegment(lastEntry.LineBreak, lastEntry.Indent, "Max Mip Level", "3", "float", "_MaxMipLevel", ""));
            }
        }

        private void AddVertexProgramIfMissing(List<Method> fragmentPrograms)
        {
            if (fragmentPrograms.Count == 0)
            {
                _log(2, "Shader contains no fragment programs, this shader is not supported.");
                return;
            }

            var structName = fragmentPrograms.First().Parameters.First(x => x is Parameter p && !p.Modifiers.Any()).As<Parameter>().Type.Name;
            var vertexProgramTemplate = VertexProgramTemplate.Replace(InputStructDefaultName, structName);

            var surfacePragma = GetPragmas("surface");
            surfacePragma.ForEach(x => x.Params.Add("vertex:vert"));

            for (var i = 0; i < _shaderParser.Parsed.Count; i++)
            {
                Segment segment = _shaderParser.Parsed[i];

                if (segment is CGSegment cg && cg.Lines.Any(x => x is Struct))
                {
                    var parsed = CGParser.CreateInstance().Parse(new StringSpan(vertexProgramTemplate), segment.Indent).OfType<CGSegment>().ToList();
                    _shaderParser.Parsed.InsertRange(i + 1, parsed);
                    i += parsed.Count + 1;
                }
            }
        }

        private class AttributeDefinition
        {
            public AttributeDefinition(int lineIndexProperty, PropertySegment property)
            {
                Property = property;
                LineIndexProperty = lineIndexProperty;
            }

            public AttributeDefinition(int lineIndexDefinition, VariableDefinitionSegment variable)
            {
                Definition = variable;
                LineIndexDefinition = lineIndexDefinition;
            }

            public int LineIndexProperty { get; set; } = -1;

            public int LineIndexDefinition { get; set; } = -1;

            public PropertySegment Property { get; set; }

            public VariableDefinitionSegment Definition { get; set; }
        }

        private Dictionary<string, ParamName> ConvertScalarPropertiesToTextures()
        {
            var propertySegments = new Dictionary<string, AttributeDefinition>();

            var replacedScalars = new Dictionary<string, ParamName>();

            var defaultToOldNameToNewName = new Dictionary<string, List<KeyValuePair<string, AttributeDefinition>>>();

            for (var index = 0; index < _shaderParser.Parsed.Count; index++)
            {
                switch (_shaderParser.Parsed[index])
                {
                    case PropertySegment propertySegment:
                        if (!propertySegments.TryGetValue(propertySegment.Name, out AttributeDefinition attributeDefinitionA))
                            propertySegments[propertySegment.Name] = attributeDefinitionA = new AttributeDefinition(index, propertySegment);
                        attributeDefinitionA.Property = propertySegment;
                        attributeDefinitionA.LineIndexProperty = index;
                        break;
                    case VariableDefinitionSegment definitionSegment:
                        if (!propertySegments.TryGetValue(definitionSegment.Name, out AttributeDefinition attributeDefinitionB))
                            propertySegments[definitionSegment.Name] = attributeDefinitionB = new AttributeDefinition(index, definitionSegment);
                        attributeDefinitionB.Definition = definitionSegment;
                        attributeDefinitionB.LineIndexDefinition = index;
                        break;
                }
            }

            foreach (AttributeDefinition t in propertySegments.Values)
            {
                PropertySegment propertySegment = t.Property;

                if (propertySegment == null)
                    continue;

                VariableDefinitionSegment variableSegment = t.Definition;
                var oldName = propertySegment.Name;
                var wasScalar = false;

                var segmentDefaultValue = propertySegment.DefaultValue;
                if (propertySegment.AttributeType.StartsWith("float") || propertySegment.AttributeType.StartsWith("Range") ||
                    propertySegment.AttributeType.StartsWith("int"))
                {
                    wasScalar = true;
                    var isFloat = TryParseFloat(segmentDefaultValue, out var parsedFloat);
                    propertySegment.DefaultValue = GetDefaultTextureForScalarValue(isFloat, parsedFloat);
                    propertySegment.Attributes = "";
                }
                else if (propertySegment.AttributeType.StartsWith("Color") || propertySegment.AttributeType.StartsWith("Vector"))
                { 
                    var res = 0f;
                    var cnt = 0;
                    var numbers = segmentDefaultValue.Split(new[] {'(', ')', ','}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var number in numbers)
                    {
                        if (!TryParseFloat(number, out var parsedFloat))
                            continue;

                        res += parsedFloat;
                        cnt++;
                    }

                    propertySegment.DefaultValue = GetDefaultTextureForScalarValue(cnt > 0, cnt > 0 ? res / cnt : 0f);
                    propertySegment.Attributes = "";
                }
                else
                    continue;

                propertySegment.Name += "Map";
                propertySegment.AttributeType = "2D";

                if (variableSegment != null)
                {
                    variableSegment.Name += "Map";
                    variableSegment.Type = "sampler2D";
                }

                if (wasScalar)
                {
                    var defaultValue = propertySegment.DefaultValue;
                    if (!defaultToOldNameToNewName.TryGetValue(defaultValue, out var list))
                        defaultToOldNameToNewName[defaultValue] = list = new List<KeyValuePair<string, AttributeDefinition>>();

                    list.Add(new KeyValuePair<string, AttributeDefinition>(oldName, t));
                }
                else
                    replacedScalars[oldName] = new ParamName(propertySegment.Name, ValueComponent.All);
            }

            var removeLines = new List<int>();
            foreach (var oldNameToNewNames in defaultToOldNameToNewName.Values)
            {
                for (var index = 0; index < oldNameToNewNames.Count; index += 4)
                {
                    var newPropName = "";
                    var description = "";
                    for (var c = 0; c < 4 && c + index >= 0 && oldNameToNewNames.Count > c + index; c++)
                    {
                        newPropName += oldNameToNewNames[c + index].Key;
                        description += (description != "" ? "|" : "") + oldNameToNewNames[c + index].Value.Property.Description;
                    }

                    for (var c = 0; c < 4 && c + index >= 0 && oldNameToNewNames.Count > c + index; c++)
                        replacedScalars[oldNameToNewNames[c + index].Key] = new ParamName(newPropName, (ValueComponent) (int) Math.Pow(2, c));

                    AttributeDefinition attributeDefinition = oldNameToNewNames[index].Value;
                    attributeDefinition.Property.Name = newPropName;
                    attributeDefinition.Property.Description = description;

                    if (attributeDefinition.Definition != null)
                        attributeDefinition.Definition.Name = newPropName;
                    else
                    {
                        _log(2, $"Could not find attribute definition with name '{attributeDefinition.Property.Name}'. Usages in fallback shader are not yet supported.");
                    }

                    for (var c = 1; c < 4 && c + index >= 0 && oldNameToNewNames.Count > c + index; c++)
                    {
                        removeLines.Add(oldNameToNewNames[c + index].Value.LineIndexProperty);

                        if (attributeDefinition.Definition != null)
                            removeLines.Add(oldNameToNewNames[c + index].Value.LineIndexDefinition);
                    }
                }
            }

            removeLines.Sort();
            for (var index = removeLines.Count - 1; index >= 0; index--)
            {
                var lineIndex = removeLines[index];
                _shaderParser.Parsed.RemoveAt(lineIndex);
            }

            return replacedScalars;
        }

        private Method FindProgram(string programName)
        {
            var cgSegements = _shaderParser.Parsed.OfType<CGSegment>().ToList();
            Method method = cgSegements.Select(x => x.Lines[0]).OfType<Method>().FirstOrDefault(m => m.MethodName == programName);
            return method;
        }

        private List<string> FindProgramNames(string programType, string valueLookup = null)
        {
            var matchingPragmas = GetPragmas(programType);

            if (valueLookup == null)
                return matchingPragmas.Select(x => x.Params[1]).Distinct().ToList();

            return matchingPragmas.SelectMany(x => x.Params.Skip(1).Where(p => p.StartsWith(valueLookup + ":")))
                .Select(s => s.Substring(valueLookup.Length + 1)).Distinct().ToList();
        }

        private static string GetDefaultTextureForScalarValue(bool isFloat, float parsedFloat)
        {
            return isFloat ? parsedFloat >= 0.5f ? "\"white\" {}" : "\"black\" {}" : "\"white\" = {}";
        }

        private List<DirectiveSegment> GetPragmas(string programType)
        {
            var directiveSegments = _shaderParser.Parsed.OfType<DirectiveSegment>().ToList();
            var matchingPragmas = directiveSegments.Where(x => x.Keyword == "pragma" && x.Params.FirstOrDefault() == programType).ToList();
            return matchingPragmas;
        }

        private int InjectAtlasMappingMethods(Segment segment, int index)
        {
            var indent = segment.Indent;
            var parsed = CGParser.CreateInstance().Parse(new StringSpan(AtlasCode), indent).OfType<CGSegment>().ToList();
            _shaderParser.Parsed.InsertRange(index, parsed);

            if (index >= 0)
            {
                _shaderParser.Parsed.Insert(index, new VariableDefinitionSegment(indent, "float", "_AtlasWidth", _shaderParser.LineBreak));
                _shaderParser.Parsed.Insert(index, new VariableDefinitionSegment(indent, "float", "_AtlasHeight", _shaderParser.LineBreak));
                _shaderParser.Parsed.Insert(index, new VariableDefinitionSegment(indent, "float", "_MaxMipLevel", _shaderParser.LineBreak));
            }

            index += parsed.Count;

            return index;
        }

        private void InjectCode(Func<Segment, int, int> _inject)
        {
            var isInsideOfShader = false;
            var injected = false;
            for (var index = 0; index < _shaderParser.Parsed.Count; index++)
            {
                Segment segment = _shaderParser.Parsed[index];

                if (!isInsideOfShader)
                {
                    if (segment is BeginSegment beginning && (beginning.Word == Keywords.BeginCG || beginning.Word == Keywords.BeginProgram))
                    {
                        isInsideOfShader = true;
                        continue;
                    }
                }
                else
                {
                    if (segment is EndCGSegment)
                    {
                        isInsideOfShader = false;
                        injected = false;
                        continue;
                    }
                }

                if (isInsideOfShader && !injected && !(segment is DirectiveSegment))
                {
                    index += _inject.Invoke(segment, index);
                    injected = true;
                }
            }
        }

        private static void InjectParameter(IList<IParameter> parameters, Parameter newParam)
        {
            var insertPos = -1;
            for (var i = 0; i < parameters.Count; i++)
            {
                IParameter formula = parameters[i];
                if (formula is Parameter parameter)
                {
                    if (parameter.Register == null || !parameter.Register.Name.Contains("TEXCOORD"))
                        continue;

                    var stringNumber = string.Join("", parameter.Register.Name.Where(char.IsDigit));
                    var number = int.TryParse(stringNumber, out var n) ? n : -1;

                    if (number < _atlasChannel)
                        continue;

                    parameter.Register.Name = $"TEXCOORD{number + 1}";

                    if (insertPos < 0)
                        insertPos = i;
                }
                else if (formula is UnparsedParameter unparsed)
                {
                    var collectionPart = unparsed.Part.As<ICollectionPart>();

                    if (collectionPart.Parts.Count > 0)
                    {
                        if (collectionPart.Parts[0] is MethodHeader method)
                        {
                            foreach (IPart part in method.Parts)
                            {
                                if (part is Numeric numericHolder && int.TryParse(numericHolder.Number, out var numeric) && numeric >= _atlasChannel)
                                    numericHolder.Number = (numeric + 1).ToString();
                            }
                        }
                    }
                }
            }

            if (insertPos < 0)
                insertPos = parameters.Count;
            
            var anyHasRegister = parameters.Any(x => x is Parameter p && p.Register != null);
            if (!anyHasRegister)
                newParam.Register = null;

            while (insertPos > 0 && parameters[insertPos-1] is InlineDefine)
                insertPos--;
            
            parameters.Insert(insertPos, newParam);
        }

        private readonly struct ParamName
        {
            public ParamName(string name, ValueComponent component)
            {
                Name = name;
                Component = component;
            }

            public string Name { get; }

            public ValueComponent Component { get; }
        }

        private void PatchFragmentPrograms(IReadOnlyDictionary<string, ParamName> replacements, IEnumerable<Method> fragmentPrograms)
        {
            foreach (Method fragmentProgram in fragmentPrograms)
            {
                var inVariables = fragmentProgram.Parameters.OfType<Parameter>().Where(x => x.Modifiers.Count == 0);
                foreach (Parameter inVariable in inVariables)
                    AddAtlasRegisterIfMissing(inVariable.Type.Name, fragmentProgram.Parameters);
            }

            const string unknown = "TODO";

            if (replacements.Count == 0)
                return;

            foreach (CGSegment cgSegement in GetCGSegmentsFiltered())
            {
                if (!(cgSegement.Lines[0] is Method method))
                    continue;

                var allSegments = _shaderParser.Parsed.OfType<CGSegment>().SelectMany(x => x.Lines.OfType<Struct>()).ToList();
                var foundUv0 = TryFindTexCoord(0, method.Parameters, out var uvParamName, "", allSegments);
                var allSegments1 = _shaderParser.Parsed.OfType<CGSegment>().SelectMany(x => x.Lines.OfType<Struct>()).ToList();
                var foundAtlasUv = TryFindTexCoord(_atlasChannel, method.Parameters, out var uvAtlasName, "atlas", allSegments1);
                var paramsPlus = $", {uvParamName ?? unknown}, {uvAtlasName ?? unknown}";

                var contents = cgSegement.Get<Variable>().ToList();
                foreach (var formula in contents)
                {
                    if (!replacements.TryGetValue(formula.Value?.Name ?? unknown, out ParamName newName))
                        continue;

                    var index = formula.Key.Parts.IndexOf(formula.Value);
                    formula.Key.Parts.RemoveAt(index);

                    var parts = new List<IPart> {new MethodHeader("tex2Datlas", $"{newName.Name}{paramsPlus}", "", new List<string>(), 0)};
                    if (newName.Component != ValueComponent.All)
                    {
                        parts.AddRange(new IPart[]
                        {
                            new MemberAccessor(), new Variable(newName.Component.ToString().ToLower())
                        });
                    }

                    formula.Key.Parts.Insert(index, new Formula(parts, "", 0));
                }
            }
        }

        private void PatchTex2DUsages()
        {
            foreach (CGSegment cgSegement in GetCGSegmentsFiltered())
            {
                if (!(cgSegement.Lines[0] is Method method))
                    continue;

                var structs = _shaderParser.Parsed.OfType<CGSegment>().SelectMany(x => x.Lines.OfType<Struct>()).ToList();
                TryFindTexCoord(_atlasChannel, method.Parameters, out var uvAtlasName, "atlas", structs);

                var contents = cgSegement.Get<MethodHeader>().ToList();
                foreach (var texMethod in contents.Where(x => x.Value.Name == "tex2D"))
                {
                    IPart textureName = texMethod.Value.Parts[0];
                    IPart textureUV = texMethod.Value.Parts[1];

                    texMethod.Value.Name = "tex2Datlas";
                    texMethod.Value.Parts.Clear();
                    texMethod.Value.Parts.Add(textureName);
                    texMethod.Value.Parts.Add(textureUV);
                    texMethod.Value.Parts.Add(new Variable(uvAtlasName));
                }
            }
        }

        private List<CGSegment> GetCGSegmentsFiltered()
        {
            return _shaderParser.Parsed.OfType<CGSegment>().Where(x => !_ignoredMethods.Contains(x.Lines[0].As<Method>()?.MethodName)).ToList();
        }

        private void PatchVertexPrograms(List<Method> vertexPrograms)
        {
            foreach (Method method in vertexPrograms.Where(x => x != null))
            {
                var lines = (List<IPart>) method.Body.Parts;
                var returnLineIndex = lines.FindLastIndex(p => p is Formula f && f.Parts.OfType<Variable>().Any(v => v.Name == "return"));

                if (returnLineIndex >= 0)
                {
                    string returnVariableName;

                    var returnVariableType = method.ReturnType.Name;
                    var returnLine = lines[returnLineIndex].Cast<Formula>();

                    // Return value is not a variable then create one.
                    var variable = returnLine.Parts[1] as Variable;
                    if (variable == null)
                    {
                        IPart returnExpression = returnLine.Parts[1];
                        var tempValueName = "tempReturnValue";
                        returnLine.Parts[1] = new Variable(tempValueName);
                        lines.Insert(returnLineIndex, new Formula(new List<IPart>
                        {
                            new Variable(returnVariableType), new Variable(tempValueName), new SyntaxToken("="), returnExpression, new CommandEndToken()
                        }, returnLine.LineBreak, returnLine.IndentLevel));
                        returnVariableName = tempValueName;
                    }
                    else
                        returnVariableName = variable.Name;

                    var inParam = method.Parameters.First().As<Parameter>();
                    var meshToVertex = AddAtlasRegisterIfMissing(inParam.Type.Name, method.Parameters);

                    lines.Insert(returnLineIndex, new Formula(new List<IPart>
                    {
                        new Variable(returnVariableName), new MemberAccessor(), new Variable("atlas"), new SyntaxToken("="), new Variable(meshToVertex),
                        new CommandEndToken()
                    }, returnLine.LineBreak, returnLine.IndentLevel));
                }
                else
                {
                    Parameter parameterOut = method.Parameters.OfType<Parameter>().FirstOrDefault(x => x.Modifiers.Contains("out"));
                    Parameter parameterIn = method.Parameters.OfType<Parameter>().FirstOrDefault(x => x.Modifiers.Contains("inout"));

                    var target = AddAtlasRegisterIfMissing(parameterOut.Type.Name, method.Parameters);

                    var returnLine = lines.Last().Cast<Formula>();
                    lines.Insert(lines.Count, new Formula(new List<IPart>
                    {
                        new Variable(target), new SyntaxToken("="), new Variable(parameterIn.Name.Name), new MemberAccessor(),
                        new Variable($"texcoord{_atlasChannel}"),
                        new CommandEndToken()
                    }, returnLine.LineBreak, returnLine.IndentLevel));
                }
            }
        }

        private void RenameShader()
        {
            var shaderStartIndex = _shaderParser.Parsed.FindIndex(x => x is BeginSegment bs && bs.Word == Keywords.Shader);
            Segment shaderName = _shaderParser.Parsed[shaderStartIndex + 1];
            var closingHyphen = shaderName.Value.LastIndexOf("\"");
            shaderName.Value = shaderName.Value.Insert(closingHyphen, " (Atlas)");
        }

        private static bool TryParseFloat(string segmentDefaultValue, out float parsedFloat)
        {
            var isFloat = float.TryParse(segmentDefaultValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out parsedFloat);
            return isFloat;
        }
    }

    [Flags]
    internal enum ValueComponent
    {
        X = 0b0001,

        Y = 0b0010,

        Z = 0b0100,

        W = 0b1000,

        All = X | Y | Z | W
    }
}