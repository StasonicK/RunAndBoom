using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JustAssets.ShaderPatcher
{
    public class StringSpan
    {
        private string _text;

        private List<char> _lineEndChars = new List<char> {'\r', '\n'};

        public int Offset { get; set; } = 0;

        public StringSpan(string text)
        {
            _text = text;
        }

        public override string ToString()
        {
            return SubText;
        }

        public string SubText => _text.Substring(Offset);

        public bool IsAtEnd => Offset >= _text.Length;

        private int FindLineEnd()
        {
            var nextLineEnd = _text.IndexOfAny(_lineEndChars.ToArray(), Offset);
            if (_text.Length > (nextLineEnd+1) && _text[nextLineEnd + 1] == '\n')
                nextLineEnd++;
            return nextLineEnd;
        }

        /// <summary>
        /// Progresses to the start of the next word using a word separator char, reads it and puts the pointer on the last letter of the word.
        /// </summary>
        /// <returns>The next word.</returns>
        public string ReadWord(string endChars = " \r\n")
        {
            ReadToNextWord();

            int startIndex = Offset;
            int endIndex = _text.IndexOfAny(endChars.ToCharArray(), startIndex);

            if (endIndex < 0)
                endIndex = _text.Length;

            Offset = endIndex;
            return _text.Substring(startIndex, endIndex - startIndex);
        }

        private static readonly Regex MatchNumber =
            new Regex("[+-]?(\\d+([.]\\d*)?([eE][+-]?\\d+)?|[.]\\d+([eE][+-]?\\d+)?)[fFdD]?", RegexOptions.Compiled);

        private static readonly Regex ValidChars = new Regex("[a-zA-Z0-9_]+", RegexOptions.Compiled);

        public string ReadWhile(string regex = null)
        {
            var result = "";
            var regexToUse = regex != null ? new Regex(regex) : ValidChars;

            while (Offset < _text.Length)
            {
                var next = Peek();

                if (regexToUse.IsMatch(new string(next, 1)))
                {
                    result += next;
                    Offset++;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        public string ReadToNextWord(string whitespaceChars = " \t\r\n")
        {
            if (Offset == _text.Length)
                return string.Empty;

            string emptySpace = "";
            while (whitespaceChars.ToCharArray().Contains(Peek()))
            {
                emptySpace += Peek();
                Offset++;

                if (Offset == _text.Length)
                    break;
            }

            return emptySpace;
        }

        public string ReadWhile(Func<char, bool> continueMatching)
        {
            if (Offset == _text.Length)
                return string.Empty;

            string emptySpace = "";
            while (continueMatching(Peek()))
            {
                emptySpace += Peek();
                Offset++;

                if (Offset == _text.Length)
                    break;
            }

            return emptySpace;
        }


        public char Peek()
        {
            return _text[Offset];
        }

        /// <summary>
        ///   Reads to the end of the line including the line break.
        /// </summary>
        /// <returns>The content from current position to the end of the line including the line break.</returns>
        public string ReadToLineEnd()
        {
            var startIndex = Offset;
            var nextLineEnd = FindLineEnd();

            if (nextLineEnd < 0)
                nextLineEnd = _text.Length-1;

            Offset = nextLineEnd+1;
            return _text.Substring(startIndex, nextLineEnd - startIndex+1);
        }

        /// <summary>
        ///  Reads to given character inclusive.
        /// </summary>
        /// <param name="readTo"></param>
        /// <param name="recursionDepthIncrease"></param>
        /// <returns></returns>
        public string ReadBalanced(char readTo, char recursionDepthIncrease)
        {
            var result = "";
            var recursionDepth = 0;
            do
            {
                var current = Peek();
                Offset++;

                if (current == recursionDepthIncrease)
                    recursionDepth++;

                if (current == readTo)
                    recursionDepth--;

                result += current;

            } while (recursionDepth >= 0);

            return result;
        }

        public bool TryReadNumeric(out string numeric)
        {
            Match match = MatchNumber.Match(SubText);
            if (match.Success && match.Index == 0)
            {
                Offset += match.Length;
                numeric = match.Value;
                return true;
            }

            numeric = "";
            return false;
        }

        public char Read()
        {
            var peek = Peek();
            Offset++;
            return peek;
        }

        public string ReadBeforeAndStay(Func<char, bool> continueMatching)
        {
            if (Offset == 0)
                return string.Empty;

            var offset = Offset;
            string emptySpace = "";
            
            Offset--;
            while(continueMatching(Peek()))
            {
                emptySpace = Peek() + emptySpace;
                
                if (Offset == 0)
                    break;

                Offset--;
            }

            Offset = offset;
            return emptySpace;
        }

        public string ReadLine()
        {
            var lineContent = ReadWhile(c => c != '\r' && c != '\n');

            if (IsAtEnd)
                return lineContent;
            
            var lineEndChar = Read();
            lineContent += lineEndChar;

            if (IsAtEnd)
                return lineContent;
            
            var nextChar = Peek();
            if ((nextChar == '\r' || nextChar == '\n') && nextChar != lineEndChar)
                lineContent += Read();

            return lineContent;
        }

        public string ComputeLineEnding()
        {
            int countCarriageReturn = 0;
            int countLineFeed = 0;

            foreach (var c in _text)
            {
                switch (c)
                {
                    case '\n':
                        countLineFeed++;
                        break;
                    case '\r':
                        countCarriageReturn++;
                        break;
                }
            }

            if (countCarriageReturn > countLineFeed * 2) return "\r";
            return countLineFeed > countCarriageReturn * 2 ? "\n" : "\r\n";
        }

        public void ConsumeLineEnding()
        {
            var peek = Peek();
            if (!IsAtEnd && (peek == '\r' || peek == '\n'))
                Offset++;

            var peek2 = Peek();
            if (!IsAtEnd && peek != peek2 && (peek2 == '\r' || peek2 == '\n'))
                Offset++;
        }

        public int PeekWhile(Func<char, bool> isTrue)
        {
            int i = 0;
            while (_text.Length > Offset+i && isTrue.Invoke(_text[Offset + i]))
                i++;

            return i;
        }
    }
}