using System;
using System.Linq;

namespace JustAssets.ShaderPatcher
{
    internal static class StringUtils
    {
        public static string PadIfSet(string @operator)
        {
            return String.IsNullOrWhiteSpace(@operator) ? @operator : " " + @operator.Trim() + " ";
        }

        public static string Without(this string text, string excludeChars)
        {
            var exclusion = excludeChars.ToCharArray().ToList();
            return new string(text.Where(c => !exclusion.Contains(c)).ToArray());
        }

        public static string RemoveLineEnding(this string text)
        {
            var cutoff = 0;

            char peek = '\0';
            if (text.Length > 0)
                peek = text[text.Length - 1];

            if (peek == '\r' || peek == '\n')
                cutoff++;

            if (text.Length > 1)
            {
                var peek2 = text[text.Length - 2];
                if (peek != peek2 && (peek2 == '\r' || peek2 == '\n'))
                    cutoff++;
            }

            return text.Substring(0, text.Length - cutoff);
        }

        public static int CountOccurrences(this string that, string searchTerm)
        {
            int count = 0, n = 0;

            if(searchTerm != "")
            {
                while ((n = that.IndexOf(searchTerm, n, StringComparison.InvariantCulture)) != -1)
                {
                    n += searchTerm.Length;
                    ++count;
                }
            }

            return count;
        }
    }
}