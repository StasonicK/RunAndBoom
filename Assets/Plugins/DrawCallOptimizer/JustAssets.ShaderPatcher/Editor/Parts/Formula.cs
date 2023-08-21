using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace JustAssets.ShaderPatcher.Parts
{
    [DebuggerDisplay("{Serialize()}")]
    internal class Formula : ICollectionPart
    {
        public Formula(List<IPart> parts, string lineEnding, int indentLevel)
        {
            if (parts.Any(x => x is LineBreak))
                throw new ArgumentException("There are no line breaks allowed in here.");

            LineBreak = lineEnding;
            IndentLevel = indentLevel;
            Parts = new List<IPart>(parts);
        }

        public IList<IPart> Parts { get; }

        public string LineBreak { get; set; }

        public virtual string Serialize()
        {
            var indenting = string.Empty.PadLeft(IndentLevel, '\t');
            if (Parts.Count == 1)
                return indenting + Parts[0].Serialize();

            string result = indenting;

            IPart part = null;
            for (var index = 1; index < Parts.Count; index++)
            {
                IPart part1 = Parts[index-1];
                part = Parts[index];

                result += part1.Serialize();

                if (JoinWithOptionalSpace(part1, part))
                    result += " ";
            }

            if (part != null)
                result += part.Serialize();

            return result;
        }

        public int IndentLevel { get; set; }

        private bool JoinWithOptionalSpace(IPart a, IPart b)
        {
            if (b is CommandEndToken)
                return false;

            if (a is MemberAccessor || b is MemberAccessor)
                return false;

            return !(a is SyntaxToken) && !(b is SyntaxToken);
        }

        public IPart this[int i] => Parts[i];
    }
}