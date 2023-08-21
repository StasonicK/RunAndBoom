#region License
/*
MIT License

Copyright(c) 2017-2020 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System.Runtime.CompilerServices;
using JustAssets.ColliderUtilityRuntime.MeshSimplifier.Utility;

namespace JustAssets.ColliderUtilityRuntime.MeshSimplifier.Internal
{
    internal class UVChannels<TVec>
    {
        private static readonly int UVChannelCount = MeshUtils.UVChannelCount;

        private ResizableArray<TVec>[] _channels = null;
        private TVec[][] _channelsData = null;

        public TVec[][] Data
        {
            [MethodImpl(256)] // MethodImplOptions.AggressiveInlining
            get
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    if (_channels[i] != null)
                    {
                        _channelsData[i] = _channels[i].Data;
                    }
                    else
                    {
                        _channelsData[i] = null;
                    }
                }
                return _channelsData;
            }
        }

        /// <summary>
        /// Gets or sets a specific channel by index.
        /// </summary>
        /// <param name="index">The channel index.</param>
        public ResizableArray<TVec> this[int index]
        {
            [MethodImpl(256)] // MethodImplOptions.AggressiveInlining
            get { return _channels[index]; }
            [MethodImpl(256)] // MethodImplOptions.AggressiveInlining
            set { _channels[index] = value; }
        }

        public UVChannels()
        {
            _channels = new ResizableArray<TVec>[UVChannelCount];
            _channelsData = new TVec[UVChannelCount][];
        }

        /// <summary>
        /// Resizes all channels at once.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        /// <param name="trimExess">If exess memory should be trimmed.</param>
        public void Resize(int capacity, bool trimExess = false)
        {
            for (int i = 0; i < UVChannelCount; i++)
            {
                if (_channels[i] != null)
                {
                    _channels[i].Resize(capacity, trimExess);
                }
            }
        }
    }
}
