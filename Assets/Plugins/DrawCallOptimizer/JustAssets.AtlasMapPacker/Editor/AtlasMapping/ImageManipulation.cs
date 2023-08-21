using JustAssets.AtlasMapPacker.AtlasMapping.Resampling;
using UnityEditor;
using UnityEngine;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    internal static class ImageManipulation
    {
        public static Texture2D ReadTexture(Texture original)
        {
            var isNormalFormat = IsNormalFormat(original);
            
            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmp = RenderTexture.GetTemporary(original.width, original.height, 0, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);
            
            tmp.anisoLevel = 9;
            tmp.filterMode = FilterMode.Trilinear;

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(original, tmp);

            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;

            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it
            var myTexture2D = new Texture2D(original.width, original.height, TextureFormat.ARGB32, false, false);

            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();

            // Reset the active RenderTexture
            RenderTexture.active = previous;

            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(tmp);

            if (isNormalFormat)
                AdjustNormalmap(myTexture2D);

            return myTexture2D;
        }

        public static Texture2D Scale(Texture2D original, int targetWidth, int targetHeight, ResamplingFilters upScalingFilter = ResamplingFilters.Mitchell,
            ResamplingFilters downScalingFilter = ResamplingFilters.Lanczos3)
        {
            Texture2D result;
            if (targetHeight != original.height || targetWidth != original.width)
            {
                result = ImageResampling.ApplyFilter(original, targetWidth, targetHeight,
                    targetWidth * targetHeight > original.width * original.height ? upScalingFilter : downScalingFilter);
            }
            else
                result = original;

            return result;
        }

        private static void AdjustNormalmap(Texture2D result)
        {
            if (result == null)
                return;

            var colors = result.GetPixels();

            for (var index = 0; index < colors.Length; index++)
            {
                var normalX = colors[index].a * 2 - 1;
                var normalY = colors[index].g * 2 - 1;
                var normalZ = Mathf.Sqrt(1f - (normalX * normalX + normalY * normalY));
                var normalW = 1f;

                normalX = normalX / 2 + 0.5f;
                normalY = normalY / 2 + 0.5f;
                normalZ = normalZ / 2 + 0.5f;

                colors[index] = new Color(normalX, normalY, normalZ, normalW);
            }

            result.SetPixels(colors);
            result.Apply();
        }

        private static bool IsNormalFormat(Texture original)
        {
            var assetPath = AssetDatabase.GetAssetPath(original);

            if (string.IsNullOrEmpty(assetPath))
            {
                return original.graphicsFormat.ToString().Contains("Norm");
            }

            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            return textureImporter != null && textureImporter.textureType == TextureImporterType.NormalMap;
        }
    }
}