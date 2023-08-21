using System.Collections.Generic;
using JustAssets.ShaderPatcher.Parts;

namespace JustAssets.ShaderPatcher.Segments
{
    internal class CGSegment : Segment
    {
        private string _lineBreak;

        public CGSegment(string value, int indent, string lineBreak, ICollection<string> macroWords) : base(indent, value)
        {
            _lineBreak = lineBreak;

            var operationSpan = new StringSpan(value);
            operationSpan.ReadWhile(char.IsWhiteSpace);

            if (operationSpan.SubText.Length > 0)
            {
                var formulas = CGCommandParser.Parse(operationSpan, indent, lineBreak, macroWords);
                List<IPart> list = new List<IPart>();
                foreach (var formula in formulas)
                {
                    list.Add(formula);
                }
                Lines = list;
            }
        }

        public CGSegment() : base()
        {
        }

        public List<IPart> Lines { get; } = new List<IPart>();

        public override string Value
        {
            get
            {
                var result = string.Empty;

                foreach (IPart line in Lines)
                {
                    result += line.Serialize() + _lineBreak;
                }

                return result;
            }
        }

        public List<KeyValuePair<ICollectionPart, T>> Get<T>() where T: IPart
        {
            var result = new List<KeyValuePair<ICollectionPart, T>>();
            foreach (var formula in Lines)
            {
                if (formula is Method method)
                    result.AddRange(EnumerateParts<T>(method.Body));
            }

            return result;
        }

        private static List<KeyValuePair<ICollectionPart, T>> EnumerateParts<T>(ICollectionPart formula) where T : IPart
        {
            var parts = new List<KeyValuePair<ICollectionPart, T>>();
            foreach (IPart formulaPart in formula.Parts)
            {
                switch (formulaPart)
                {
                    case T correctTypePart:
                        parts.Add(new KeyValuePair<ICollectionPart, T>(formula, correctTypePart));
                        break;
                    case ICollectionPart collectionPart:
                    {
                        foreach (var collectionPartPart in collectionPart.Parts)
                        {
                            if (collectionPartPart is T correctTypeCollectionPartPart)
                                parts.Add(new KeyValuePair<ICollectionPart, T>(collectionPart, correctTypeCollectionPartPart));

                            if (collectionPartPart is ICollectionPart collectionPartCollectionPart)
                                parts.AddRange(EnumerateParts<T>(collectionPartCollectionPart));
                        }

                        break;
                    }
                }
            }

            return parts;
        }
    }
}