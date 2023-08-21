using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JustAssets.ShaderPatcher.Segments;

namespace JustAssets.ShaderPatcher
{
    public class ShaderParser
    {
        private Dictionary<string, string> _defines = new Dictionary<string, string>();

        private Dictionary<string, string> _includes = new Dictionary<string, string>();

        private readonly CGParser _cgParser;

        public ShaderParser(string filePath, string internalShadersPath)
        {
            _cgParser = new CGParser(_defines, _includes, Path.GetDirectoryName(filePath), internalShadersPath);

            var text = new StringSpan(File.ReadAllText(filePath));
            LineBreak = text.ComputeLineEnding();
            Parsed = Parse(text, 0).ToList();
        }

        public ShaderParser(string allText)
        {
            _cgParser = new CGParser(_defines,_includes, null, null);
            var text = new StringSpan(allText);
            LineBreak = text.ComputeLineEnding();
            Parsed = Parse(text, 0).ToList();
        }

        public List<Segment> Parsed { set; get; }

        public string LineBreak { set; get; }

        private IEnumerable<Segment> Parse(StringSpan text, int indent)
        {
            var cache = text.ReadToNextWord();
            while (true)
            {
                var isComment = text.PeekWhile(c => c == '/') >= 2;

                if (isComment)
                    cache += text.ReadLine();
                else
                    break;
            }

            yield return new Segment(indent, cache);

            foreach (Segment segment in ReadTo(text, w => w == Keywords.Shader, 
                (word, buffer)=>new BeginSegment(word, buffer, indent, ""),
                text1 => ParseShader(text1, indent), indent))
                yield return segment;
        }

        private IEnumerable<Segment> ParseShader(StringSpan text, int indent)
        {
            var shaderName = text.ReadWhile(x => x != '"') + text.Read() + text.ReadWhile(x => x != '"');
            var shaderStart = text.ReadWhile(c => c != '{') + text.Read();

            yield return new Segment(indent, shaderName + shaderStart);

            foreach (Segment span in ReadTo(text, w => w == Keywords.Properties, 
                (word, buffer)=>new BeginSegment(word, buffer, indent, ""),
                contents => ParseProperties(contents, indent+2), indent+1))
                yield return span;

            while (!text.IsAtEnd)
            {
                foreach (Segment segment in ReadTo(text, w => w == Keywords.BeginCG || w == Keywords.BeginProgram, 
                    (word, buffer)=>new BeginSegment(word, buffer, indent, LineBreak),
                    span => _cgParser.Parse(span, indent+2), indent+1))
                    yield return segment;
            }
        }

        private IEnumerable<Segment> ReadTo(StringSpan text, Func<string, bool> found, Func<string, string, Segment> createBeginSegment,
            Func<StringSpan, IEnumerable<Segment>> processSection, int indent)
        {
            string buffer = "";
            while (!text.IsAtEnd)
            {
                var whitespace = text.ReadToNextWord();
                buffer += whitespace;

                var word = text.ReadWord();
                buffer += word;

                if (!found(word))
                    continue;

                yield return createBeginSegment.Invoke(word, buffer);
                buffer = "";

                foreach (Segment span in processSection(text))
                    yield return span;

                break;
            }

            if (!string.IsNullOrEmpty(buffer))
                yield return new Segment(indent, buffer);
        }

        private IEnumerable<Segment> ParseProperties(StringSpan contents, int indent)
        {
            var openBracket = contents.ReadWhile(c => c != '{') + contents.Read();
            yield return new Segment(indent, openBracket);

            do
            {
                var offsetBefore = contents.Offset;
                var line = contents.ReadToLineEnd();
                Match match = Regex.Match(line, RegularExpressions.Property);

                if (match.Success)
                {
                    var matchedProperty = match.Captures[0].Value;
                    contents.Offset = offsetBefore + matchedProperty.Length;
                    contents.ConsumeLineEnding();
                    yield return new PropertySegment(matchedProperty, match.Groups, LineBreak, indent);
                }
                else
                {
                    yield return new Segment(indent, line);

                    Match closeMatch = Regex.Match(line, RegularExpressions.ClosingBracket);
                    if (closeMatch.Success)
                        break;
                }
            } while (true);
        }

        public void Write(string file)
        {
            File.WriteAllText(file, Parsed.Select(x => x.Value).Aggregate((a, b) => $"{a}{b}"), Encoding.UTF8);
        }
    }
}