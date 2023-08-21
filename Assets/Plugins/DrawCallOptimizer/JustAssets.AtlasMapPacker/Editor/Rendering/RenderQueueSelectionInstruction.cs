namespace JustAssets.AtlasMapPacker.Rendering
{
    public class RenderQueueSelectionInstruction:IConditionalInstruction
    {
        public RenderQueueSelectionInstruction(int targetRenderQueue, Condition condition)
        {
            TargetRenderQueue = targetRenderQueue;
            Condition = condition;
        }

        public RenderQueueSelectionInstruction()
        {
        }

        public int TargetRenderQueue { get; set; }

        public Condition Condition { get; set; }
    }
}