using System;
using UnityEngine;

namespace JustAssets.AtlasMapPacker.Rendering
{
    public readonly struct MaterialUsage
    {
        public Renderer Renderer { get; }

        public int SlotIndex { get; }

        public MeshFilter Filter { get; }

        public MaterialTextures MaterialTextures { get; }

        public Material Material => Renderer.sharedMaterials[SlotIndex];

        public MaterialUsage(Renderer renderer, int slotIndex, MeshFilter filter, float colorSimilarityThreshold)
        {
            Renderer = renderer;
            SlotIndex = slotIndex;
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
            MaterialTextures = new MaterialTextures(Renderer.sharedMaterials[SlotIndex], colorSimilarityThreshold);
        }
    }
}