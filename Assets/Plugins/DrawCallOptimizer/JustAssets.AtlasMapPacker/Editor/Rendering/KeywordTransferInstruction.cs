namespace JustAssets.AtlasMapPacker.Rendering
{
    public class KeywordTransferInstruction : IConditionalInstruction
    {
        public string Keyword { get; set; }

        public Condition Condition { get; set; }

        public KeywordTransferInstruction(string keyword, Condition condition)
        {
            Keyword = keyword;
            Condition = condition;
        }

        public KeywordTransferInstruction()
        {
        }
    }
}