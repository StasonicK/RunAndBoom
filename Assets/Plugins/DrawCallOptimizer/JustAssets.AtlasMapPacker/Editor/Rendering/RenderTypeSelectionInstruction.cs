namespace JustAssets.AtlasMapPacker.Rendering
{
    public class RenderTypeSelectionInstruction:IConditionalInstruction
    {
        public RenderTypeSelectionInstruction(string targetRenderType, Condition condition)
        {
            TargetRenderType = targetRenderType;
            Condition = condition;
        }

        public RenderTypeSelectionInstruction()
        {
        }

        public string TargetRenderType { get; set; }

        public Condition Condition { get; set; }
    }
}