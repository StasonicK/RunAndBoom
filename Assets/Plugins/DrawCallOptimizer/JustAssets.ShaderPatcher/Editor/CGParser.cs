using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JustAssets.ShaderPatcher.Segments;

namespace JustAssets.ShaderPatcher
{
    public class CGParser
    {
        public static CGParser CreateInstance()
        {
            return new CGParser();
        }

        private string _lineBreak;

        private Dictionary<string, string> _defines;

        private Dictionary<string, string> _includes;

        private string _workingDir;

        private string _internalShadersPath;

        private CGParser() : this(new Dictionary<string, string>(), new Dictionary<string, string>(), null, null)
        {
        }

        public CGParser(Dictionary<string, string> defines, Dictionary<string, string> includes, string workingDir, string internalShadersPath)
        {
            _defines = defines;
            _includes = includes;
            _workingDir = workingDir;
            _internalShadersPath = internalShadersPath;
        }

        public IEnumerable<Segment> Parse(StringSpan text, int indent)
        {
            _lineBreak = text.ComputeLineEnding();

            string word;
            do
            {
                var emptySpace = text.ReadToNextWord();
                word = text.ReadWord();

                var fullLine = emptySpace + word + text.ReadToLineEnd();

                if (emptySpace.CountOccurrences(_lineBreak) >=2)
                    yield return new Segment(indent, _lineBreak);

                switch (word)
                {
                    case Keywords.EndCG:
                    case Keywords.EndProgram:
                        yield return new EndCGSegment(fullLine, indent);
                        break;
                    default:
                        if (word.StartsWith(Keywords.Hash) &&
                            TryParseDirective(fullLine, out DirectiveSegment directive, indent))
                            yield return directive;
                        else if (word.StartsWith(Keywords.FullLineComment))
                            yield return new Segment(indent, fullLine.RemoveLineEnding());
                        else if (TryParseVariableDefinition(indent, fullLine, out var definitionSegments))
                            foreach (var variableDefinitionSegment in definitionSegments)
                                yield return variableDefinitionSegment;
                        else if (TryParseCommand(indent, fullLine, text, out CGSegment command))
                            yield return command;
                        else
                            yield return new Segment(indent, fullLine);
                        break;
                }
            } while (word != Keywords.EndCG && word != Keywords.EndProgram && !text.IsAtEnd);
        }

        private bool TryParseVariableDefinition(int indent, string fullLine,
            out List<VariableDefinitionSegment> variableDefinitionSegment)
        {
            fullLine = fullLine.Replace("\r", "").Replace("\n", "");
            var validChars = new[] {';', '_', ',', ' ', '\t'};
            if (!fullLine.All(c => char.IsLetterOrDigit(c) || validChars.Contains(c)))
            {
                variableDefinitionSegment = null;
                return false;
            }

            var stringSpan = new StringSpan(fullLine);
            stringSpan.ReadToNextWord();
            fullLine = stringSpan.SubText.RemoveLineEnding().Replace(";", " ;");
            var words = fullLine.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);

            var type = string.Empty;
            var variableNames = new List<string>();
            for (var index = 2; index < words.Length; index++)
            {
                type = words[index - 2];
                variableNames = words[index - 1].Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (words[index] == ";")
                    break;
            }

            variableDefinitionSegment = variableNames.Select(x => new VariableDefinitionSegment(indent, type, x, _lineBreak)).ToList();

            if (variableDefinitionSegment.Count == 0)
            {
                variableDefinitionSegment = null;
                return false;
            }

            return true;
        }

        private bool TryParseDirective(string fullLine, out DirectiveSegment directiveSegment, int indent)
        {
            var line = fullLine.Trim('\r', '\n', '\t', ' ');

            if (line.StartsWith(Keywords.Hash))
            {
                directiveSegment = new DirectiveSegment(fullLine, _lineBreak, indent);

                RegisterDirective(directiveSegment);

                return true;
            }

            directiveSegment = null;
            return false;
        }

        private void RegisterDirective(DirectiveSegment directiveSegment)
        {
            switch (directiveSegment.Keyword.ToLower())
            {
                case "define":
                    var value = directiveSegment.Params.Count > 1
                        ? directiveSegment.Params.Skip(1).Aggregate((a, b) => a + " " + b)
                        : null;
                    _defines[directiveSegment.Params[0]] = value;
                    break;

                case "include":
                    var includeFile = directiveSegment.Params.Aggregate((a, b) => a + " " + b);
                    includeFile = includeFile.Substring(1);
                    includeFile = includeFile.Substring(0, includeFile.IndexOf("\""));
                    var findInclude = FindInclude(includeFile);

                    var wasKnown = _includes.ContainsValue(findInclude);
                    _includes[directiveSegment.Params[0]] = findInclude;

                    if (findInclude != null && !wasKnown)
                        ParseInclude(findInclude);

                    break;
            }
        }

        private void ParseInclude(string findInclude)
        {
            var text = new StringSpan(File.ReadAllText(findInclude));
            Parse(text, 0).ToList();
        }

        private string FindInclude(string localPath)
        {
            if (_internalShadersPath == null)
                return null;

            var fullIncludePath = Path.Combine(_workingDir, localPath);

            if (File.Exists(fullIncludePath))
                return fullIncludePath;

            fullIncludePath = Path.Combine(_internalShadersPath, localPath);

            if (File.Exists(fullIncludePath))
                return fullIncludePath;
            
            return null;
        }

        private bool TryParseCommand(int indent, string fullLine, StringSpan text, out CGSegment result) 
        {
            if (!ParseCode(fullLine, text, out var line))
            {
                result = null;
                return false;
            }

            result = new CGSegment(line, indent, _lineBreak, _defines.Keys);
            return true;
        }

        private static bool ParseCode(string fullLine, StringSpan text, out string line) 
        {
            var offset = text.Offset;
            line = fullLine.Without("\r\n");

            if (!line.EndsWith(";"))
            {
                var isMethodStart = line.EndsWith("{");
                if (!isMethodStart)
                    line += text.ReadWhile(c => c != ';' && c != '{');

                if (text.IsAtEnd)
                {
                    text.Offset = offset;
                    return false;
                }

                bool isMethodBody = isMethodStart || text.Peek() == '{';

                if (!isMethodStart)
                    line += text.Read();

                if (isMethodBody)
                    line += text.ReadBalanced('}', '{');

                if (!text.IsAtEnd && text.Peek() == ';')
                    line += text.Read();
            }

            return true;
        }
    }
}