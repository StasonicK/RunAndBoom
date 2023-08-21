using System;
using JustAssets.AtlasMapPacker.AtlasMapping;
using UnityEngine;

namespace JustAssets.AtlasMapPacker
{
    [Serializable]
    internal class GUIData
    {
        public bool JustStatic = true;

        public int MaximumTextureSize = 2048;

        public string SaveFolderPath;

        public int ActiveMenu = 0;

        public bool IsDebugOn;
    
        public AtlasingStrategy Strategy = AtlasingStrategy.ScaleToClosestAndFill;

        public int LodLevel = 0;

        public int AtlasMarginInPixels = 8;

        public bool AddDisabledGameObjects;

        public UnityEngine.Shader SourceShader;

        public UnityEngine.Shader TargetShader;

        public TextAsset ShaderConfiguration;

        public bool CanAttributeAtlasBeShrunk = true;

        public float ColorSimilarityThreshold = 0.01f;

        public Vector2Int MinimalTextureSize = new Vector2Int(2, 2);
    }
}