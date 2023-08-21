using System.Collections.Generic;
using UnityEngine;

namespace JustAssets.AtlasMapPacker.Rendering
{
    public interface IShaderInfo
    {
        string TargetShaderName { get; set; }

        List<AttributePointerPair> ShaderAttributesToTransfer { get; }

        List<KeywordTransferInstruction> KeywordTransferInstructions { get; }

        List<RenderTypeSelectionInstruction> RenderTypeMapping { get; }

        List<RenderQueueSelectionInstruction> RenderQueueMapping { get; }

        List<NamingRule> NamingRules { get; }

        List<string> EvaluateNaming(Material material);
    }
}