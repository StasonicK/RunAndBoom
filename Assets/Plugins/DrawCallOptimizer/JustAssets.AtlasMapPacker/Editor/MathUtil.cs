using System;

namespace JustAssets.AtlasMapPacker
{
    public static class MathUtil
    {
        public static int Clamp(int value, int min, int max)
        {
            return Math.Min(max, Math.Max(min, value));
        }

        public static int MipCount(long width)
        {
            long nextPowerOf2 = ToNextPowerOfTwo(width);

            int idx = 0;
            while (nextPowerOf2 > 0)
            {
                nextPowerOf2 /= 2;
                idx++;
            }

            return idx;
        }

        public static long ToNextPowerOfTwo(long x)
        {
            if (x <= 0)
            {
                return 0;
            }

            --x;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            x |= x >> 32;
            return x + 1;
        }

        public static long ToNearestPowerOfTwo(long number)
        {
            long nextPowerOfTwo = ToNextPowerOfTwo(number);
            long previousPowerOfTwo = nextPowerOfTwo >> 1; 

            return nextPowerOfTwo - number > number - previousPowerOfTwo ? previousPowerOfTwo : nextPowerOfTwo;
        }
    }
}