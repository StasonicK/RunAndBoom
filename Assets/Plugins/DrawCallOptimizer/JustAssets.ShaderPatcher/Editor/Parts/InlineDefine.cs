namespace JustAssets.ShaderPatcher.Parts
{
    internal class InlineDefine : IParameter
    {
        public IPart Part { get; }
        public InlineDefine(IPart part)
        {
            Part = part;
        }
        
        public string Serialize()
        {
            return Part.Serialize();
        }
    }
}