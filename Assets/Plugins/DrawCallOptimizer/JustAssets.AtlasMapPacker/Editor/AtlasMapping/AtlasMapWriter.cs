using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public class AtlasMapWriter
    {
        private static Dictionary<string, Stopwatch> _stopwatches = new Dictionary<string, Stopwatch>();

        /// <summary>
        ///     Returns a PNG memory stream.
        /// </summary>
        /// <returns>PNG stream.</returns>
        public static MemoryStream SaveAtlas(List<AtlasMapEntry> layerEntries, Vector2Int dimension, int margin, Func<AtlasMapEntry, Texture2D> readTexture2D)
        {
            Texture2D atlas = CreateAtlasTexture(layerEntries, dimension, margin, readTexture2D);

            var outStream = new MemoryStream();
            var encodedPng = atlas.EncodeToPNG();
            outStream.Write(encodedPng, 0, encodedPng.Length);
            outStream.Flush();

            return outStream;
        }

        public static Texture2D CreateAtlasTexture(List<AtlasMapEntry> layerEntries, Vector2Int dimension, int margin,
            Func<AtlasMapEntry, Texture2D> readTexture2D)
        {
            var dimensionX = dimension.x;
            var dimensionY = dimension.y;
            const int scale = 1;
            var atlas = new Texture2D(dimensionX * scale, dimensionY * scale, TextureFormat.ARGB32, false);

            StartStopwatch("Compose");
            atlas.ClearTo(Color.black); // Clear to black
            atlas.Apply();
            var atlasPixels = atlas.GetPixels();
            var atlasWidth = atlas.width;

            foreach (var layoutEntry in layerEntries)
            {
                var xOut = (int)layoutEntry.Rectangle.X * scale;
                var yOut = (int)layoutEntry.Rectangle.Y * scale;

                var imageFile = ReadTexture2D(readTexture2D, layoutEntry, scale);
                var pixels = imageFile.GetPixels();
                var imageWidth = imageFile.width;

                var scaledMargin = margin * scale;
                for (var y = -scaledMargin; y < imageFile.height + scaledMargin; y++)
                    for (var x = -scaledMargin; x < imageFile.width + scaledMargin; x++)
                    {
                        var clampX = MathUtil.Clamp(x, 0, imageWidth - 1);
                        var clampY = MathUtil.Clamp(y, 0, imageFile.height - 1);
                        var xWrite = MathUtil.Clamp(xOut + x, 0, atlasWidth - 1);
                        var yWrite = MathUtil.Clamp(yOut + y, 0, atlas.height - 1);

                        Color colorOut = pixels[clampX + clampY * imageWidth];

                        atlasPixels[xWrite + yWrite * atlasWidth] = colorOut;
                    }
            }

            StopStopwatch("Compose");

            atlas.SetPixels(atlasPixels);
            atlas.Apply();
            return atlas;
        }

        [Conditional("VERBOSE")]
        private static void StopStopwatch(string name)
        {
            if (!_stopwatches.TryGetValue(name, out var stopwatch))
                throw new ArgumentOutOfRangeException(nameof(name));

            stopwatch.Stop();

            Debug.Log($"Stopwatch '{name}' updated to {stopwatch.ElapsedMilliseconds}ms");
        }

        [Conditional("VERBOSE")]
        private static void StartStopwatch(string name)
        {
            if (!_stopwatches.TryGetValue(name, out var stopwatch))
                _stopwatches[name] = stopwatch = new Stopwatch();

            stopwatch.Start();
        }

        private static Texture2D ReadTexture2D(Func<AtlasMapEntry, Texture2D> readTexture2D, AtlasMapEntry layoutEntry,
            float scale)
        {
            StartStopwatch("ReadTexture");

            var image = readTexture2D.Invoke(layoutEntry);
            if (image == null)
            {
                image = new Texture2D(16, 16, TextureFormat.ARGB32, false);
            }

            var targetWidth = (int)(layoutEntry.Rectangle.Width * scale);
            var targetHeight = (int)(layoutEntry.Rectangle.Height * scale);
            if (image.width == targetWidth && image.height == targetHeight)
            {
                StopStopwatch("ReadTexture");

                return image;
            }

            StopStopwatch("ReadTexture");

            StartStopwatch("ScaleReadTexture");
            // Scaling code.
            Texture2D scaled = ImageManipulation.Scale(image, targetWidth, targetHeight);
            StopStopwatch("ScaleReadTexture");
            return scaled;
        }
    }
}