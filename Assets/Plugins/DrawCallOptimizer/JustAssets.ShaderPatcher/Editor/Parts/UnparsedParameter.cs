namespace JustAssets.ShaderPatcher.Parts
{
    public class UnparsedParameter : IParameter
    {
        public IPart Part { get; }

        public UnparsedParameter(IPart part)
        {
            Part = part;
        }
        
        public string Serialize()
        {
            return Part.Serialize();
        }
    }
}