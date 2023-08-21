using System.Threading.Tasks;
using UnityEngine;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    internal static class Texture2DExtensions
    {
        public static void ClearTo(this Texture2D inputTexture, Color color)
        {
            var pixels = new Color32[inputTexture.width * inputTexture.height];
            Color32 color32 = color;

            for (var index = 0; index < pixels.Length; index++)
                pixels[index] = color32;

            inputTexture.SetPixels32(pixels);
            inputTexture.Apply();
        }

        public static Texture2D ReorderChannels(this Texture2D input, Channel red = Channel.Red, Channel green = Channel.Green, Channel blue = Channel.Blue,
            Channel alpha = Channel.Alpha)
        {
            var inPixels = input.GetPixels();

            var outPixels = new Color[inPixels.Length];

            Parallel.ForEach(inPixels,
                (x, state, i) => outPixels[i] = new Color(inPixels[i][(int) red], inPixels[i][(int) green], inPixels[i][(int) blue], inPixels[i][(int) alpha]));

            var outTex = new Texture2D(input.width, input.height, TextureFormat.ARGB32, false);
            outTex.SetPixels(outPixels);
            outTex.Apply();

            return outTex;
        }
    }

}