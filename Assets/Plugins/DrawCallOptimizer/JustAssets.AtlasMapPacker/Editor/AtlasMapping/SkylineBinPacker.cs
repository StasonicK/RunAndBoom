using System;
using System.Collections.Generic;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    internal class SkylineBinPacker
    {
        private readonly long _binWidth;

        private readonly long _binHeight;

        private readonly bool _useWasteMap;

        private readonly Action<string, float> _progress;

        private long _usedSurfaceArea;

        private readonly List<SkylineNode> _skyLine = new List<SkylineNode>();

        private readonly GuillotineBinPacker _wasteMap = new GuillotineBinPacker();

#if _DEBUG
        private readonly DisjointRectCollection _disjointRects = new DisjointRectCollection();
#endif

        public SkylineBinPacker(long width, long height, bool useWasteMap, Action<string, float> progress)
        {
            _binWidth = width;
            _binHeight = height;

            _useWasteMap = useWasteMap;
            _progress = progress;

#if _DEBUG
            _disjointRects.Clear();
#endif

            _usedSurfaceArea = 0;
            SkylineNode node = new SkylineNode(0, 0, _binWidth);
            _skyLine.Add(node);
            if (_useWasteMap)
            {
                _wasteMap.Init(width, height);
                _wasteMap.FreeRectangles.Clear();
            }
        }

        public bool TryInsert(List<IAtlasTile> rects, List<(PixelRect, IAtlasTile)> dst, LevelChoiceHeuristic method)
        {
            dst.Clear();

            while (rects.Count > 0)
            {
                (PixelRect, IAtlasTile) bestNode = default;
                long bestScore1 = long.MaxValue;
                long bestScore2 = long.MaxValue;
                int bestSkylineIndex = -1;
                int bestRectIndex = -1;

                _progress?.Invoke($"Inserting {dst.Count} / {rects.Count+dst.Count}", dst.Count / (float)(rects.Count+dst.Count));

                for (int i = 0; i < rects.Count; ++i)
                {
                    long score1 = 0;
                    long score2 = 0;
                    int index = 0;
                    PixelRect newNodeRect = default;
                    switch (method)
                    {
                        case LevelChoiceHeuristic.LevelBottomLeft:
                            newNodeRect = FindPositionForNewNodeBottomLeft(rects[i].Size.Width, rects[i].Size.Height, out score1, out score2, out index);
#if _DEBUG
                            Debug.Assert(_disjointRects.Disjoint(newNodeBottomLeft));
#endif
                            break;
                        case LevelChoiceHeuristic.LevelMinWasteFit:
                            newNodeRect = FindPositionForNewNodeMinWaste(rects[i].Size.Width, rects[i].Size.Height, out score2, out score1, out index);
#if _DEBUG
                            Debug.Assert(_disjointRects.Disjoint(newNodeMinWaste));
#endif
                            break;
                        default:
#if _DEBUG
                            Debug.Assert(false);
#endif
                            break;
                    }

                    if (newNodeRect.Height == 0)
                        continue;

                    if (score1 >= bestScore1 && (score1 != bestScore1 || score2 >= bestScore2))
                        continue;

                    
                    var newNode = (newNodeRect, rects[i]);

                    bestNode = newNode;
                    bestScore1 = score1;
                    bestScore2 = score2;
                    bestSkylineIndex = index;
                    bestRectIndex = i;
                }

                if (bestRectIndex == -1)
                {
                    _progress?.Invoke($"Failed layouting, refitting...", 1f);
                    return false;
                }

                // Perform the actual packing.
#if _DEBUG
                Debug.Assert(_disjointRects.Disjoint(bestNode.Rectangle));
                _disjointRects.Add(bestNode.Rectangle);
#endif
                AddSkylineLevel(bestSkylineIndex, bestNode.Item1);
                _usedSurfaceArea += rects[bestRectIndex].Size.Width * rects[bestRectIndex].Size.Height;
                rects.RemoveAt(bestRectIndex);
                dst.Add(bestNode);
            }

            _progress?.Invoke($"Layouting succeeded.", 1f);
            return true;
        }

//        private AtlasRect Insert(long width, long height, LevelChoiceHeuristic method)
//        {
//            // First try to pack this rectangle into the waste map, if it fits.
//            var node = _wasteMap.Insert(width, height, true, FreeRectChoiceHeuristic.RectBestShortSideFit, GuillotineSplitHeuristic.SplitMaximizeArea);
//            #if DEBUG
//            Debug.Assert(disjointRects.Disjoint(node));
//            #endif
//            if (node.Height != 0)
//            {
//                var newNode = new AtlasRect(node.X, node.Y, node.Width, node.Height);
//                _usedSurfaceArea += width * height;
//#if DEBUG
//                Debug.Assert(disjointRects.Disjoint(newNode));
//                disjointRects.Add(newNode);
//#endif
//                return newNode;
//            }

//            switch (method)
//            {
//                case LevelChoiceHeuristic.LevelBottomLeft: return InsertBottomLeft(width, height);
//                case LevelChoiceHeuristic.LevelMinWasteFit: return InsertMinWaste(width, height);
//                default:
//                    Debug.Assert(false);
//                    return node;
//            }
//        }

        private bool RectangleFits(int skylineNodeIndex, long width, long height, out long y)
        {
            SkylineNode skylineNode = _skyLine[skylineNodeIndex];
            if (skylineNode.X + width > _binWidth)
            {
                y = 0;
                return false;
            }

            long widthLeft = width;
            int i = skylineNodeIndex;
            y = skylineNode.Y;
            while (widthLeft > 0)
            {
                SkylineNode node = _skyLine[i];
                var nodeY = node.Y;
                if (y <= nodeY)
                    y = nodeY;

                if (y + height > _binHeight)
                    return false;

                widthLeft -= node.Width;
                ++i;

#if _DEBUG
                Debug.Assert(i < _skyLine.Count || widthLeft <= 0);
#endif
            }

            return true;
        }

        private long ComputeWastedArea(int skylineNodeIndex, long width, long y) 
        {
            long wastedArea = 0L;
            long rectLeft = _skyLine[skylineNodeIndex].X;
            long rectRight = rectLeft + width;
            for (; skylineNodeIndex < _skyLine.Count && _skyLine[skylineNodeIndex].X < rectRight; ++skylineNodeIndex)
            {
                SkylineNode skylineNode = _skyLine[skylineNodeIndex];
                if (skylineNode.X >= rectRight || skylineNode.X + skylineNode.Width <= rectLeft)
                    break;

                long leftSide = skylineNode.X;
                long rightSide = Math.Min(rectRight, leftSide + skylineNode.Width);
#if _DEBUG
                Debug.Assert(y >= _skyLine[skylineNodeIndex].Y);
#endif
                wastedArea += (rightSide - leftSide) * (y - skylineNode.Y);
            }

            return wastedArea;
        }

        private bool RectangleFits(int skylineNodeIndex, long width, long height, out long y, out long wastedArea) 
        {
            wastedArea = 0;
            bool fits = RectangleFits(skylineNodeIndex, width, height, out y);
            if (fits)
                wastedArea = ComputeWastedArea(skylineNodeIndex, width, y);

            return fits;
        }

        private void AddWasteMapArea(int skylineNodeIndex, long width, long y)
        {
            // long wastedArea = 0; // unused
            long rectLeft = _skyLine[skylineNodeIndex].X;
            long rectRight = rectLeft + width;
            for (; skylineNodeIndex < _skyLine.Count && _skyLine[skylineNodeIndex].X < rectRight; ++skylineNodeIndex)
            {
                SkylineNode skylineNode = _skyLine[skylineNodeIndex];
                if (skylineNode.X >= rectRight || skylineNode.X + skylineNode.Width <= rectLeft)
                    break;

                long leftSide = skylineNode.X;
                long rightSide = Math.Min(rectRight, leftSide + skylineNode.Width);
#if _DEBUG
                Debug.Assert(y >= _skyLine[skylineNodeIndex].Y);
#endif

                PixelRect waste = new PixelRect(leftSide, skylineNode.Y, rightSide - leftSide, y - skylineNode.Y);
#if _DEBUG
                Debug.Assert(_disjointRects.Disjoint(waste));
#endif
                _wasteMap.FreeRectangles.Add(waste);
            }
        }

        private void AddSkylineLevel(int skylineNodeIndex, PixelRect rect)
        {
            // First track all wasted areas and mark them into the waste map if we're using one.
            if (_useWasteMap)
                AddWasteMapArea(skylineNodeIndex, rect.Width, rect.Y);

            SkylineNode newNode = new SkylineNode(rect.X, rect.Y + rect.Height, rect.Width);
            _skyLine.Insert(skylineNodeIndex, newNode);

#if _DEBUG
            Debug.Assert(newNode.X + newNode.Width <= _binWidth);
            Debug.Assert(newNode.Y <= _binHeight);
#endif

            for (int i = skylineNodeIndex + 1; i < _skyLine.Count; ++i)
            {
#if _DEBUG
                Debug.Assert(_skyLine[i - 1].X <= _skyLine[i].X);
#endif

                if (_skyLine[i].X < _skyLine[i - 1].X + _skyLine[i - 1].Width)
                {
                    long shrink = _skyLine[i - 1].X + _skyLine[i - 1].Width - _skyLine[i].X;

                    _skyLine[i] = new SkylineNode(_skyLine[i].X + shrink, _skyLine[i].Y, _skyLine[i].Width - shrink);
                    
                    if (_skyLine[i].Width <= 0)
                    {
                        _skyLine.RemoveAt(i);
                        --i;
                    }
                    else
                        break;
                }
                else
                    break;
            }

            MergeSkylines();
        }

        private void MergeSkylines()
        {
            for (int i = 0; i < _skyLine.Count - 1; ++i)
                if (_skyLine[i].Y == _skyLine[i + 1].Y)
                {
                    _skyLine[i] = new SkylineNode(_skyLine[i].X, _skyLine[i].Y, _skyLine[i].Width + _skyLine[i + 1].Width);
                    _skyLine.RemoveAt(i + 1);
                    --i;
                }
        }

//        private AtlasRect InsertBottomLeft(long width, long height)
//        {
//            long bestHeight;
//            long bestWidth;
//            var newNode = FindPositionForNewNodeBottomLeft(width, height, out bestHeight, out bestWidth, out var bestIndex);

//            if (bestIndex != -1)
//            {
//#if DEBUG
//                Debug.Assert(disjointRects.Disjoint(newNode));
//#endif
//                // Perform the actual packing.
//                AddSkylineLevel(bestIndex, newNode);

//                _usedSurfaceArea += width * height;
//#if DEBUG
//                disjointRects.Add(newNode);
//#endif
//            }
//            else
//                newNode = new AtlasRect();

//            return newNode;
//        }

        private PixelRect FindPositionForNewNodeBottomLeft(long width, long height, out long bestHeight, out long bestWidth, out int bestIndex) 
        {
            bestHeight = long.MaxValue;
            bestIndex = -1;

            // Used to break ties if there are nodes at the same level. Then pick the narrowest one.
            bestWidth = long.MaxValue;
            PixelRect newNode = new PixelRect();
            var skyLineLength = _skyLine.Count;
            for (int i = 0; i < skyLineLength; i++)
            {
                if (RectangleFits(i, width, height, out var y))
                {
                    var offsetY = y + height;
                    SkylineNode skylineNode = _skyLine[i];

                    if (offsetY < bestHeight || (offsetY == bestHeight && skylineNode.Width < bestWidth))
                    {
                        bestHeight = offsetY;
                        bestIndex = i;
                        bestWidth = skylineNode.Width;
                        newNode = new PixelRect(skylineNode.X, y, width, height);
#if _DEBUG
                        Debug.Assert(_disjointRects.Disjoint(newNode));
#endif
                    }
                }

#if CAN_ROTATE
                if (RectangleFits(i, height, width, out y))
                {
                    if (y + width < bestHeight || (y + width == bestHeight && skylineNode.Width < bestWidth))
                    {
                        bestHeight = y + width;
                        bestIndex = i;
                        bestWidth = skylineNode.Width;
                        newNode = new PixelRect(skylineNode.X, y, height, width);
#if _DEBUG
                        Debug.Assert(_disjointRects.Disjoint(newNode));
#endif
                    }
                }
#endif
            }

            return newNode;
        }

//        private AtlasRect InsertMinWaste(long width, long height)
//        {
//            var newNode = FindPositionForNewNodeMinWaste(width, height, out var bestHeight, out var bestWastedArea, out var bestIndex);

//            if (bestIndex != -1)
//            {
//#if DEBUG
//                Debug.Assert(disjointRects.Disjoint(newNode));
//#endif

//                // Perform the actual packing.
//                AddSkylineLevel(bestIndex, newNode);

//                _usedSurfaceArea += width * height;
//#if DEBUG
//                disjointRects.Add(newNode);
//#endif
//            }
//            else
//                newNode = new AtlasRect();

//            return newNode;
//        }

        private PixelRect FindPositionForNewNodeMinWaste(long width, long height, out long bestHeight, out long bestWastedArea, out int bestIndex)

        {
            bestHeight = long.MaxValue;
            bestWastedArea = long.MaxValue;
            bestIndex = -1;
            PixelRect newNode = new PixelRect();
            
            for (int i = 0; i < _skyLine.Count; ++i)
            {
                if (RectangleFits(i, width, height, out long y, out long wastedArea))
                {
                    if (wastedArea < bestWastedArea || (wastedArea == bestWastedArea && y + height < bestHeight))
                    {
                        bestHeight = y + height;
                        bestWastedArea = wastedArea;
                        bestIndex = i;
                        newNode = new PixelRect(_skyLine[i].X, y, width, height);
#if _DEBUG
                        Debug.Assert(_disjointRects.Disjoint(newNode));
#endif
                    }
                }

                if (RectangleFits(i, height, width, out y, out wastedArea))
                {
                    if (wastedArea < bestWastedArea || (wastedArea == bestWastedArea && y + width < bestHeight))
                    {
                        bestHeight = y + width;
                        bestWastedArea = wastedArea;
                        bestIndex = i;
                        newNode = new PixelRect(_skyLine[i].X, y, height, width);
#if _DEBUG
                        Debug.Assert(_disjointRects.Disjoint(newNode));
#endif
                    }
                }
            }

            return newNode;
        }

        /// Computes the ratio of used surface area.
        private long Occupancy() 

        {
            return _usedSurfaceArea / (_binWidth * _binHeight);
        }



        internal enum LevelChoiceHeuristic
        {
            LevelBottomLeft,

            LevelMinWasteFit
        }
    }
}
