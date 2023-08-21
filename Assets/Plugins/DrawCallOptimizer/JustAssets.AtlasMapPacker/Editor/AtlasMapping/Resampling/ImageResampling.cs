using System;
using UnityEngine;

namespace JustAssets.AtlasMapPacker.AtlasMapping.Resampling
{
    /// <summary>
    ///     A class for image resampling with custom filters.
    /// </summary>
    public class ImageResampling
    {

        /// <summary>
        ///     Resamples input array to a new array using current resampling filter.
        /// </summary>
        /// <param name="input">Input array.</param>
        /// <param name="nWidth">Width of the output array.</param>
        /// <param name="nHeight">Height of the output array.</param>
        /// <returns>Output array.</returns>
        public byte[][,] Resample(byte[][,] input, int nWidth, int nHeight)
        {
            if (input == null || input.Length == 0 || nWidth < 1 || nHeight < 1)
                return null;

            var filter = ResamplingFilter.Create(Filter);

            var width = input[0].GetLength(0);
            var height = input[0].GetLength(1);
            var planes = input.Length;

            Covered = 0;
            Total = nWidth + height;

            // create bitmaps
            var work = new byte[planes][,];
            var output = new byte[planes][,];
            int c;

            for (c = 0; c < planes; c++)
            {
                work[c] = new byte[nWidth, height];
                output[c] = new byte[nWidth, nHeight];
            }

            var xScale = (double) nWidth / width;
            var yScale = (double) nHeight / height;

            var contrib = new ContributorEntry[nWidth];

            double wdth;
            double center;
            double weight;
            double intensity;
            int left;
            int right;
            int i;
            int j;
            int k;

            // horizontal downsampling
            if (xScale < 1.0)
            {
                // scales from bigger to smaller width
                wdth = filter.defaultFilterRadius / xScale;

                for (i = 0; i < nWidth; i++)
                {
                    contrib[i].n = 0;
                    contrib[i].p = new Contributor[(int) Math.Floor(2 * wdth + 1)];
                    contrib[i].wsum = 0;
                    center = (i + 0.5) / xScale;
                    left = (int) (center - wdth);
                    right = (int) (center + wdth);

                    for (j = left; j <= right; j++)
                    {
                        weight = filter.GetValue((center - j - 0.5) * xScale);

                        if (weight == 0 || j < 0 || j >= width)
                            continue;

                        contrib[i].p[contrib[i].n].pixel = j;
                        contrib[i].p[contrib[i].n].weight = weight;
                        contrib[i].wsum += weight;
                        contrib[i].n++;
                    }

                    if (Aborting)
                        return output;
                }
            }
            else
            {
                // horizontal upsampling
                // scales from smaller to bigger width
                for (i = 0; i < nWidth; i++)
                {
                    contrib[i].n = 0;
                    contrib[i].p = new Contributor[(int) Math.Floor(2 * filter.defaultFilterRadius + 1)];
                    contrib[i].wsum = 0;
                    center = (i + 0.5) / xScale;
                    left = (int) Math.Floor(center - filter.defaultFilterRadius);
                    right = (int) Math.Ceiling(center + filter.defaultFilterRadius);

                    for (j = left; j <= right; j++)
                    {
                        weight = filter.GetValue(center - j - 0.5);

                        if (weight == 0 || j < 0 || j >= width)
                            continue;

                        contrib[i].p[contrib[i].n].pixel = j;
                        contrib[i].p[contrib[i].n].weight = weight;
                        contrib[i].wsum += weight;
                        contrib[i].n++;
                    }

                    if (Aborting)
                        return output;
                }
            }

            // filter horizontally from input to work
            for (c = 0; c < planes; c++)
            {
                for (k = 0; k < height; k++)
                {
                    for (i = 0; i < nWidth; i++)
                    {
                        intensity = 0;

                        for (j = 0; j < contrib[i].n; j++)
                        {
                            weight = contrib[i].p[j].weight;

                            if (weight == 0)
                                continue;

                            intensity += input[c][contrib[i].p[j].pixel, k] * weight;
                        }

                        work[c][i, k] = (byte) Math.Min(Math.Max(intensity / contrib[i].wsum + 0.5, byte.MinValue), byte.MaxValue);
                    }

                    if (Aborting)
                        return output;

                    Covered++;
                }
            }

            // pre-calculate filter contributions for a column
            contrib = new ContributorEntry[nHeight];

            // vertical downsampling
            if (yScale < 1.0)
            {
                // scales from bigger to smaller height
                wdth = filter.defaultFilterRadius / yScale;

                for (i = 0; i < nHeight; i++)
                {
                    contrib[i].n = 0;
                    contrib[i].p = new Contributor[(int) Math.Floor(2 * wdth + 1)];
                    contrib[i].wsum = 0;
                    center = (i + 0.5) / yScale;
                    left = (int) (center - wdth);
                    right = (int) (center + wdth);

                    for (j = left; j <= right; j++)
                    {
                        weight = filter.GetValue((center - j - 0.5) * yScale);

                        if (weight == 0 || j < 0 || j >= height)
                            continue;

                        contrib[i].p[contrib[i].n].pixel = j;
                        contrib[i].p[contrib[i].n].weight = weight;
                        contrib[i].wsum += weight;
                        contrib[i].n++;
                    }

                    if (Aborting)
                        return output;
                }
            }
            else
            {
                // vertical upsampling
                // scales from smaller to bigger height
                for (i = 0; i < nHeight; i++)
                {
                    contrib[i].n = 0;
                    contrib[i].p = new Contributor[(int) Math.Floor(2 * filter.defaultFilterRadius + 1)];
                    contrib[i].wsum = 0;
                    center = (i + 0.5) / yScale;
                    left = (int) (center - filter.defaultFilterRadius);
                    right = (int) (center + filter.defaultFilterRadius);

                    for (j = left; j <= right; j++)
                    {
                        weight = filter.GetValue(center - j - 0.5);

                        if (weight == 0 || j < 0 || j >= height)
                            continue;

                        contrib[i].p[contrib[i].n].pixel = j;
                        contrib[i].p[contrib[i].n].weight = weight;
                        contrib[i].wsum += weight;
                        contrib[i].n++;
                    }

                    if (Aborting)
                        return output;
                }
            }

            // filter vertically from work to output
            for (c = 0; c < planes; c++)
            {
                for (k = 0; k < nWidth; k++)
                {
                    for (i = 0; i < nHeight; i++)
                    {
                        intensity = 0;

                        for (j = 0; j < contrib[i].n; j++)
                        {
                            weight = contrib[i].p[j].weight;

                            if (weight == 0)
                                continue;

                            intensity += work[c][k, contrib[i].p[j].pixel] * weight;
                        }

                        output[c][k, i] = (byte) Math.Min(Math.Max(intensity / contrib[i].wsum + 0.5, byte.MinValue), byte.MaxValue);
                    }

                    if (Aborting)
                        return output;

                    Covered++;
                }
            }
            
            return output;
        }

        public static Texture2D ApplyFilter(Texture2D input, int width, int height, ResamplingFilters resamplingFilter)
        {
            var resampler = new ImageResampling {Filter = resamplingFilter};
            
            byte[][,] inputData = ToData(input.GetPixels32(), input.width, input.height);
            byte[][,] output = resampler.Resample(inputData, width, height);
            
            return ToTex(output, width, height);
        }

        private static Texture2D ToTex(byte[][,] output, int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.ARGB32, false);

            var pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[x + y * width] = new Color32(output[0][x, y], output[1][x, y], output[2][x, y], output[3][x, y]);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        private static byte[][,] ToData(Color32[] data, int width, int height)
        {
            var result = new byte[4][, ];
            for (var index = 0; index < result.Length; index++)
            {
                result[index] = new byte[width, height];
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var inPixel = data[x + y * width];
                    result[0][x, y] = inPixel.r;
                    result[1][x, y] = inPixel.g;
                    result[2][x, y] = inPixel.b;
                    result[3][x, y] = inPixel.a;
                }
            }

            return result;
        }

        #region Properties

        /// <summary>
        ///     Gets or sets the resampling filter.
        /// </summary>
        public ResamplingFilters Filter { get; set; } = ResamplingFilters.Box;

        /// <summary>
        ///     Gets or sets wheter the resampling process is stopping.
        /// </summary>
        public bool Aborting { get; set; } = false;

        /// <summary>
        ///     Covered units. Progress can be computed in combination with ResamplingService.Total property.
        /// </summary>
        public int Covered { get; private set; }

        /// <summary>
        ///     Total units. Progress can be computer in combination with ResamplingService.Covered property.
        /// </summary>
        public int Total { get; private set; }

        #endregion

        #region Structures

        internal struct Contributor
        {
            public int pixel;

            public double weight;
        }

        internal struct ContributorEntry
        {
            public int n;

            public Contributor[] p;

            public double wsum;
        }

        #endregion

        #region Private Fields

        #endregion
    }
}