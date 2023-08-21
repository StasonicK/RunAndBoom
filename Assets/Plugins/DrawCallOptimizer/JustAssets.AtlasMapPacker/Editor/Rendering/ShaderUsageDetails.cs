using System.Collections.Generic;
using UnityEngine;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering;
#else
using ShaderPropertyType = UnityEditor.ShaderUtil.ShaderPropertyType;
#endif

namespace JustAssets.AtlasMapPacker.Rendering
{
    public class MaterialTextures
    {
        public MaterialTextures(Material material, float colorSimilarityThreshold)
        {
            UnityEngine.Shader shader = material.shader;

            var propCount = shader.GetShaderPropertyCount();

            var textureNames = new List<string>();
            var colorNames = new List<string>();

            for (var i = 0; i < propCount; i++)
            {
                ShaderPropertyType type = shader.GetShaderPropertyType(i);
                var name = shader.GetShaderPropertyName(i);

                switch (type)
                {
#if UNITY_2019_3_OR_NEWER
                    case ShaderPropertyType.Texture:
#else
                    case ShaderPropertyType.TexEnv:
#endif
                        textureNames.Add(name);
                        break;
                    case ShaderPropertyType.Color:
                        colorNames.Add(name);
                        break;
                }
            }

            SetTextureSlots(material, textureNames);
            SetColorSlots(material, colorNames, colorSimilarityThreshold);
        }

        public Dictionary<string, TextureConfig> TextureSlots { get; } = new Dictionary<string, TextureConfig>();

        public Dictionary<string, ColorConfig> ColorSlots { get; } = new Dictionary<string, ColorConfig>();

        private void SetColorSlots(Material material, List<string> colorNames, float colorSimilarityThreshold)
        {
            foreach (var name in colorNames)
            {
                Color color = material.GetColor(name);

                ColorSlots[name] = new ColorConfig(material, color, colorSimilarityThreshold);
            }
        }

        private void SetTextureSlots(Material material, List<string> textureNames)
        {
            foreach (var name in textureNames)
            {
                Texture texture = material.GetTexture(name);

                var color = new Color();
                if (texture == null)
                {
#if UNITY_2019_3_OR_NEWER
                    var propertyIndexByName = material.shader.GetPropertyIndexByName(name);
                    var defaultTextureName = material.shader.GetPropertyTextureDefaultName(propertyIndexByName);
#else
                    var defaultTextureName = "gray";
#endif

                    switch (defaultTextureName)
                    {
                        case "white":
                            color = new Color(1, 1, 1, 1);
                            break;
                        case "black":
                            color = new Color(0, 0, 0, 0);
                            break;
                        case "gray":
                            color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                            break;
                        case "bump":
                            color = new Color(0.5f, 0.5f, 1, 0.5f);
                            break;
                        case "red":
                            color = new Color(1, 0, 0, 0);
                            break;
                    }
                }

                TextureSlots[name] = new TextureConfig(material, texture, color, material.GetTextureOffset(name), material.GetTextureScale(name));
            }
        }
    }
}