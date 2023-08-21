using System.Diagnostics;

namespace JustAssets.ShaderPatcher.Segments
{
    [DebuggerDisplay("{Value}")]
    public class Segment
    {
        public Segment(int indent, string value)
        {
            Indent = indent;
            Value = value;
        }

        public Segment()
        {
        }

        public virtual string Value { get; set; }

        public int Indent { get; set; }

        protected string Indentation => string.Empty.PadLeft(Indent, '\t');
    }
}