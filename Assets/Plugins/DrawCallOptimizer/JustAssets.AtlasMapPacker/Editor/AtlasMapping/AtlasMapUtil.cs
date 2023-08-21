using System;
using System.Collections.Generic;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public static class AtlasMapUtil
    {
        private class FloatBounds
        {
            public FloatBounds(float min, float max)
            {
                Min = min;
                Max = max;
            }

            public float Min { get; }
            public float Max { get; }

            public float Middle => (Min + Max) / 2f;

            public FloatBounds Lower => new FloatBounds(Min, Middle);

            public FloatBounds Upper => new FloatBounds(Middle, Max);
        }

        public static List<AtlasMapEntry> Match(List<IAtlasTile> textureDimensions, uint atlasMarginInPixels, PixelSize atlasSize,
            Action<string, string, float> progress)
        {
            if (atlasSize.Width <= atlasMarginInPixels * 2 || atlasSize.Height <= atlasMarginInPixels * 2)
                throw new ArgumentException("The atlas size has to be larger than twice the margin.");

            if (atlasMarginInPixels < 1)
                throw new ArgumentException("Must be at least 1 or larger.", nameof(atlasMarginInPixels));

            textureDimensions.Sort((a, b) => b.Size.SqrMagnitude.CompareTo(a.Size.SqrMagnitude));

            FloatBounds scale = new FloatBounds(0, 4); 
            float scaleMax = 4;

            progress?.Invoke("Matching UVs...", "Finding best scale...", 0f);
            float epsilon = 0.01f;
            int i = 0;
            int imax = 50;
            IAtlasMapLayer validLayer = null;
            while (scale.Max - scale.Min > epsilon && i <= imax)
            {
                Console.WriteLine($"Running placement with scaling factor of {scale.Middle:F2}");

                if (CanAllBePlaced(atlasMarginInPixels, atlasSize, textureDimensions, scale.Middle, out var layer,
                    (msg, p) => progress?.Invoke($"Matching UVs - Pass {i + 1}...", msg, p)))
                {
                    validLayer = layer;
                    scale = scale.Upper;
                }
                else
                {
                    scale = scale.Lower;
                }

                i++;
                progress?.Invoke("Matching UVs...", "Finding best scale...", i / (float)imax);
            }

            progress?.Invoke(null, null, 0f);

            return validLayer?.Tiles;
        }

        private static long ComputeRequiredSurfaceSideLength(IEnumerable<PixelSize> textureDimensions, uint atlasMarginInPixels)
        {
            var currentSize = 0L;

            try
            {
                checked
                {
                    foreach (var textureDimension in textureDimensions)
                        currentSize += (uint)(textureDimension.Width * textureDimension.Height) +
                                       atlasMarginInPixels * atlasMarginInPixels * 2L;
                }
            }
            catch (OverflowException)
            {
                return Int32.MaxValue;
            }

            currentSize = (uint) (Math.Sqrt(currentSize) + 2 * atlasMarginInPixels);
            return currentSize;
        }

        public static PixelSize ComputePOTSize(IEnumerable<PixelSize> textureDimensions, uint atlasMarginInPixels, AtlasingStrategy strategy)
        {
            var sideLength = ComputeRequiredSurfaceSideLength(textureDimensions, atlasMarginInPixels);

            long sideLengthPOT = 0;
            switch (strategy)
            {
                case AtlasingStrategy.ScaleToClosestAndFill:
                    sideLengthPOT = MathUtil.ToNearestPowerOfTwo(sideLength);
                    break;
                case AtlasingStrategy.ScaleUpAndFill:
                    sideLengthPOT = MathUtil.ToNextPowerOfTwo(sideLength);
                    break;
                case AtlasingStrategy.ScaleDownAndFill:
                    sideLengthPOT = MathUtil.ToNextPowerOfTwo(sideLength) >> 1;
                    break;
                case AtlasingStrategy.NPOT:
                    sideLengthPOT = sideLength;
                    break;
            }

            var sideLengthPOTWithoutMargin = sideLengthPOT - atlasMarginInPixels * 2;
            var potSurfaceSize = sideLengthPOTWithoutMargin * sideLengthPOTWithoutMargin;
            var sideLengthWithoutMargin = sideLength - atlasMarginInPixels * 2;
            var surfaceSize = sideLengthWithoutMargin * sideLengthWithoutMargin;

            var coverage = surfaceSize / (double) potSurfaceSize;

            if (coverage > 0.25 && coverage < 0.75 && sideLengthPOT / 2 > atlasMarginInPixels * 2)
                return new PixelSize(sideLengthPOT / 2, sideLengthPOT);

            return new PixelSize(sideLengthPOT, sideLengthPOT);
        }

        private static bool CanAllBePlaced(uint atlasMarginInPixels, PixelSize currentSize, List<IAtlasTile> entries, float tileScale,
            out IAtlasMapLayer result, Action<string, float> progress)
        {
            var layouter = new SkylineAtlasLayouter(entries, currentSize, atlasMarginInPixels, currentSize, tileScale, progress);
            return layouter.TryLayoutEntries(out result);
        }
    }
}