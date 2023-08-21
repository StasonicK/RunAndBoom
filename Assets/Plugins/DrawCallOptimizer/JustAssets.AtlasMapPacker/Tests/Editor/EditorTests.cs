using System;
using JustAssets.AtlasMapPacker.AtlasMapping;
using JustAssets.AtlasMapPacker.AtlasMapping.Resampling;
using System.Collections.Generic;
using NUnit.Framework;

namespace JustAssets.AtlasMapPacker.Tests
{
    internal class EditorTests
    {
        // When creating a new render pipeline asset it should not log any errors or throw exceptions.
        [Test]
        public void ResampleTextureToMinimum_Is2by2()
        {
            var testData = CreateWhite16By16Texture();

            var resampler = new ImageResampling();
            var newSize = 2;
            var resample = resampler.Resample(testData, newSize, newSize);
            Assert.IsNotNull(resample);
            Assert.AreEqual(resample[0].GetLength(0), newSize);
            Assert.AreEqual(resample[1].GetLength(1), newSize);
        }

        [Test]
        public void ResampleTextureToMinimum_Is1by1()
        {
            var testData = CreateWhite16By16Texture();

            var resampler = new ImageResampling();
            var newSize = 1;
            var resample = resampler.Resample(testData, newSize, newSize);
            Assert.IsNotNull(resample);
            Assert.AreEqual(resample[0].GetLength(0), newSize);
            Assert.AreEqual(resample[1].GetLength(1), newSize);
        }

        private static byte[][,] CreateWhite16By16Texture()
        {
            var testData = new byte[4][,];
            for (var index = 0; index < testData.Length; index++)
            {
                var channelData = new byte[16, 16];

                for (var i = 0; i < channelData.GetLength(0); i++)
                    for (var j = 0; j < channelData.GetLength(1); j++)
                        channelData[i, j] = 255;

                testData[index] = channelData;
            }

            return testData;
        }
    }
}