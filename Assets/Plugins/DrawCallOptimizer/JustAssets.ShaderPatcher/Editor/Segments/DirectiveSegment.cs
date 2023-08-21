using System;
using System.Collections.Generic;

namespace JustAssets.ShaderPatcher.Segments
{
    internal class DirectiveSegment : Segment
    {
        private readonly string _lineBreak;

        public DirectiveSegment(string line, string lineBreak, int indent) : base(indent, line)
        {
            _lineBreak = lineBreak;
            var stringSeg = new StringSpan(line);

            stringSeg.ReadWhile(char.IsWhiteSpace);
            
            if (stringSeg.Read() != Keywords.Hash[0])
                throw new ArgumentException("Line needs to start with #");

            int match = 0;
            while (!stringSeg.IsAtEnd)
            {
                var read = stringSeg.Peek();
                if (char.IsWhiteSpace(read))
                {
                    stringSeg.Offset++;
                    continue;
                }

                string word = null;
                if (stringSeg.Peek() == '"')
                {
                    stringSeg.Read();
                    word = "\"" + stringSeg.ReadWhile(c => c != '"') + "\"";
                    stringSeg.Read();
                }
                else
                {
                    word = stringSeg.ReadWhile(c => c != ' ');
                }

                word = word.TrimEnd('\r', '\n');

                if (match == 0)
                    Keyword = word;
                else
                    Params.Add(word);

                match++;
            }

        }

        public List<string> Params { get; set; } = new List<string>();

        public string Keyword { get; set; }

        public override string Value => $"{Indentation}#{Keyword} {string.Join(" ", Params)}{_lineBreak}";
    }
}