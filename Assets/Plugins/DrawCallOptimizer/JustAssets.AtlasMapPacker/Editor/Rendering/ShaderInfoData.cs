using System.Collections.Generic;

namespace JustAssets.AtlasMapPacker.Rendering
{
    public class ShaderInfoData
    {
        public string TargetShaderName { get; set; }

        public List<AttributePointerPair> AttributeTransferInstructions { get; set; } =
            new List<AttributePointerPair>();

        public List<KeywordTransferInstruction> KeywordTransferInstructions { get; set; } =
            new List<KeywordTransferInstruction>();

        public List<RenderTypeSelectionInstruction> RenderTypeMapping { get; set; } =
            new List<RenderTypeSelectionInstruction>();

        public List<RenderQueueSelectionInstruction> RenderQueueMapping { get; set; } =
            new List<RenderQueueSelectionInstruction>();

        public List<NamingRule> NamingRule { get; set; } = new List<NamingRule>();
    }
}