using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public class DisjointRectCollection
    {
        private List<PixelRect> _rects = new List<PixelRect>();

        public bool Add(PixelRect r)
        {
            // Degenerate rectangles are ignored.
            if (r.Width == 0 || r.Height == 0)
                return true;

            if (!Disjoint(r))
                return false;

            _rects.Add(r);
            return true;
        }

        public void Clear()
        {
            _rects.Clear();
        }

        public bool Disjoint(PixelRect r)
        {
            // Degenerate rectangles are ignored.
            if (r.Width == 0 || r.Height == 0)
                return true;

            for (var i = 0; i < _rects.Count; ++i)
            {
                if (!Disjoint(_rects[i], r))
                    return false;
            }

            return true;
        }

        public static bool Disjoint(PixelRect a, PixelRect b)
        {
            if (a.X + a.Width <= b.X || b.X + b.Width <= a.X || a.Y + a.Height <= b.Y || b.Y + b.Height <= a.Y)
                return true;

            return false;
        }
    }

    internal class GuillotineBinPacker
    {
        private float _binHeight;

        private float _binWidth;

#if _DEBUG
        /// Used to track that the packer produces proper packings.
        private DisjointRectCollection _disjointRects = new DisjointRectCollection();
#endif

        public GuillotineBinPacker()
        {
            _binWidth = 0;
            _binHeight = 0;
        }

        private GuillotineBinPacker(long width, long height)
        {
            Init(width, height);
        }

        /// Returns the internal list of disjoint rectangles that track the free area of the bin. You may alter this vector
        /// any way desired, as long as the end result still is a list of disjoint rectangles.
        public List<PixelRect> FreeRectangles { get; } = new List<PixelRect>();

        /// Returns the list of packed rectangles. You may alter this vector at will, for example, you can move a Rect from
        /// this list to the Free Rectangles list to free up space on-the-fly, but notice that this causes fragmentation.
        public List<PixelRect> UsedRectangles { get; } = new List<PixelRect>();

        public void Init(long width, long height)
        {
            _binWidth = width;
            _binHeight = height;

#if _DEBUG
            _disjointRects.Clear();
#endif

            // Clear any memory of previously packed rectangles.
            UsedRectangles.Clear();

            // We start with a single big free rectangle that spans the whole bin.
            var n = new PixelRect(0, 0, width, height);

            FreeRectangles.Clear();
            FreeRectangles.Add(n);
        }

        public PixelRect Insert(long width, long height, bool merge, FreeRectChoiceHeuristic rectChoice, GuillotineSplitHeuristic splitMethod)
        {
            // Find where to put the new rectangle.
            var newRect = FindPositionForNewNode(width, height, rectChoice, out var freeNodeIndex);

            // Abort if we didn't have enough space in the bin.
            if (newRect.Height == 0)
                return newRect;

            // Remove the space that was just consumed by the new rectangle.
            SplitFreeRectByHeuristic(FreeRectangles[freeNodeIndex], newRect, splitMethod);
            FreeRectangles.RemoveAt(freeNodeIndex);

            // Perform a Rectangle Merge step if desired.
            if (merge)
                MergeFreeList();

            // Remember the new used rectangle.
            UsedRectangles.Add(newRect);

            // Check that we're really producing correct packings here.
#if _DEBUG
            Debug.Assert(_disjointRects.Add(newRect));
#endif
            return newRect;
        }

        private PixelRect FindPositionForNewNode(long width, long height, FreeRectChoiceHeuristic rectChoice, out int nodeIndex)
        {
            var bestNode = new PixelRect();
            nodeIndex = 0;

            var bestScore = float.MaxValue;

            /// Try each free rectangle to find the best one for placement.
            for (var i = 0; i < FreeRectangles.Count; ++i)
            {
                // If this is a perfect fit upright, choose it immediately.
                if (width == FreeRectangles[i].Width && height == FreeRectangles[i].Height)
                {
                    bestNode = new PixelRect(FreeRectangles[i].X, FreeRectangles[i].Y, width, height);
                    bestScore = float.MinValue;
                    nodeIndex = i;
#if _DEBUG
                    Debug.Assert(_disjointRects.Disjoint(bestNode));
#endif
                    break;
                }

                // If this is a perfect fit sideways, choose it.

                if (height == FreeRectangles[i].Width && width == FreeRectangles[i].Height)
                {
                    bestNode = new PixelRect(FreeRectangles[i].X, FreeRectangles[i].Y, height, width);
                    bestScore = float.MinValue;
                    nodeIndex = i;
#if _DEBUG
                    Debug.Assert(_disjointRects.Disjoint(bestNode));
#endif
                    break;
                }

                // Does the rectangle fit upright?

                if (width <= FreeRectangles[i].Width && height <= FreeRectangles[i].Height)
                {
                    var score = ScoreByHeuristic(width, height, FreeRectangles[i], rectChoice);

                    if (score < bestScore)
                    {
                        bestNode = new PixelRect(FreeRectangles[i].X, FreeRectangles[i].Y, width, height);
                        bestScore = score;
                        nodeIndex = i;
#if _DEBUG
                        Debug.Assert(_disjointRects.Disjoint(bestNode));
#endif
                    }
                }

                // Does the rectangle fit sideways?
                else if (height <= FreeRectangles[i].Width && width <= FreeRectangles[i].Height)
                {
                    var score = ScoreByHeuristic(height, width, FreeRectangles[i], rectChoice);

                    if (score < bestScore)
                    {
                        bestNode = new PixelRect(FreeRectangles[i].X, FreeRectangles[i].Y, height, width);
                        bestScore = score;
                        nodeIndex = i;
#if _DEBUG
                        Debug.Assert(_disjointRects.Disjoint(bestNode));
#endif
                    }
                }
            }

            return bestNode;
        }

        /// @return True if r fits inside freeRect (possibly rotated).
        private bool Fits(AtlasSize r, PixelRect freeRect)
        {
            return (r.Width <= freeRect.Width && r.Height <= freeRect.Height) || (r.Height <= freeRect.Width && r.Width <= freeRect.Height);
        }

        /// @return True if r fits perfectly inside freeRect, i.e. the leftover area is 0.
        private bool FitsPerfectly(AtlasSize r, PixelRect freeRect)
        {
            return (r.Width == freeRect.Width && r.Height == freeRect.Height) || (r.Height == freeRect.Width && r.Width == freeRect.Height);
        }

        private void Insert(List<PixelSize> rects, bool merge, FreeRectChoiceHeuristic rectChoice, GuillotineSplitHeuristic splitMethod)
        {
            // Remember variables about the best packing choice we have made so far during the iteration process.
            var bestFreeRect = 0;
            var bestRect = 0;
            var bestFlipped = false;

            // Pack rectangles one at a time until we have cleared the rects array of all rectangles.
            // rects will get destroyed in the process.
            while (rects.Count > 0)
            {
                // Stores the penalty score of the best rectangle placement - bigger=worse, smaller=better.
                var bestScore = float.MaxValue;

                for (var i = 0; i < FreeRectangles.Count; ++i)
                {
                    for (var j = 0; j < rects.Count; ++j)
                    {
                        // If this rectangle is a perfect match, we pick it instantly.
                        if (rects[j].Width == FreeRectangles[i].Width && rects[j].Height == FreeRectangles[i].Height)
                        {
                            bestFreeRect = i;
                            bestRect = j;
                            bestFlipped = false;
                            bestScore = float.MinValue;
                            i = FreeRectangles.Count; // Force a jump out of the outer loop as well - we got an instant fit.
                            break;
                        }

                        // If flipping this rectangle is a perfect match, pick that then.

                        if (rects[j].Height == FreeRectangles[i].Width && rects[j].Width == FreeRectangles[i].Height)
                        {
                            bestFreeRect = i;
                            bestRect = j;
                            bestFlipped = true;
                            bestScore = float.MinValue;
                            i = FreeRectangles.Count; // Force a jump out of the outer loop as well - we got an instant fit.
                            break;
                        }

                        // Try if we can fit the rectangle upright.

                        if (rects[j].Width <= FreeRectangles[i].Width && rects[j].Height <= FreeRectangles[i].Height)
                        {
                            var score = ScoreByHeuristic(rects[j].Width, rects[j].Height, FreeRectangles[i], rectChoice);
                            if (score < bestScore)
                            {
                                bestFreeRect = i;
                                bestRect = j;
                                bestFlipped = false;
                                bestScore = score;
                            }
                        }

                        // If not, then perhaps flipping sideways will make it fit?
                        else if (rects[j].Height <= FreeRectangles[i].Width && rects[j].Width <= FreeRectangles[i].Height)
                        {
                            var score = ScoreByHeuristic(rects[j].Height, rects[j].Width, FreeRectangles[i], rectChoice);
                            if (score < bestScore)
                            {
                                bestFreeRect = i;
                                bestRect = j;
                                bestFlipped = true;
                                bestScore = score;
                            }
                        }
                    }
                }

                // If we didn't manage to find any rectangle to pack, abort.
                if (bestScore == float.MaxValue)
                    return;

                // Otherwise, we're good to go and do the actual packing.
                var newNode = new PixelRect(FreeRectangles[bestFreeRect].X, FreeRectangles[bestFreeRect].Y, rects[bestRect].Width, rects[bestRect].Height);

                if (bestFlipped)
                    newNode = new PixelRect(newNode.X, newNode.Y, newNode.Height, newNode.Width);

                // Remove the free space we lost in the bin.
                SplitFreeRectByHeuristic(FreeRectangles[bestFreeRect], newNode, splitMethod);
                FreeRectangles.RemoveAt(bestFreeRect);

                // Remove the rectangle we just packed from the input list.
                rects.RemoveAt(bestRect);

                // Perform a Rectangle Merge step if desired.
                if (merge)
                    MergeFreeList();

                // Remember the new used rectangle.
                UsedRectangles.Add(newNode);

#if _DEBUG

                // Check that we're really producing correct packings here.
                Debug.Assert(_disjointRects.Add(newNode));
#endif
            }
        }

        private void MergeFreeList()
        {
#if _DEBUG
            var test = new DisjointRectCollection();
            foreach (PixelRect t in FreeRectangles)
                Debug.Assert(test.Add(t));
#endif

            // Do a Theta(n^2) loop to see if any pair of free rectangles could me merged into one.
            // Note that we miss any opportunities to merge three rectangles into one. (should call this function again to detect that)
            for (var i = 0; i < FreeRectangles.Count; ++i)
            for (var j = i + 1; j < FreeRectangles.Count; ++j)
            {
                PixelRect freeRectangleI = FreeRectangles[i];
                PixelRect freeRectangleJ = FreeRectangles[j];
                if (freeRectangleI.Width == freeRectangleJ.Width && freeRectangleI.X == freeRectangleJ.X)
                {
                    if (freeRectangleI.Y == freeRectangleJ.Y + freeRectangleJ.Height)
                    {
                        freeRectangleI = new PixelRect(freeRectangleI.X, freeRectangleI.Y - freeRectangleJ.Height, freeRectangleI.Width,
                            freeRectangleI.Height + freeRectangleJ.Height);

                        FreeRectangles.RemoveAt(j);
                        --j;
                    }
                    else if (freeRectangleI.Y + freeRectangleI.Height == freeRectangleJ.Y)
                    {
                        freeRectangleI = new PixelRect(freeRectangleI.X, freeRectangleI.Y, freeRectangleI.Width, freeRectangleI.Height + freeRectangleJ.Height);

                        FreeRectangles.RemoveAt(j);
                        --j;
                    }
                }
                else if (freeRectangleI.Height == freeRectangleJ.Height && freeRectangleI.Y == freeRectangleJ.Y)
                {
                    if (freeRectangleI.X == freeRectangleJ.X + freeRectangleJ.Width)
                    {
                        freeRectangleI = new PixelRect(freeRectangleI.X - freeRectangleJ.Width, freeRectangleI.Y, freeRectangleI.Width + freeRectangleJ.Width,
                            freeRectangleI.Height);

                        FreeRectangles.RemoveAt(j);
                        --j;
                    }
                    else if (freeRectangleI.X + freeRectangleI.Width == freeRectangleJ.X)
                    {
                        freeRectangleI = new PixelRect(freeRectangleI.X, freeRectangleI.Y, freeRectangleI.Width + freeRectangleJ.Width, freeRectangleI.Height);

                        FreeRectangles.RemoveAt(j);
                        --j;
                    }
                }

                FreeRectangles[i] = freeRectangleI;
            }

#if _DEBUG
            test.Clear();
            for (var i = 0; i < FreeRectangles.Count; ++i)
                Debug.Assert(test.Add(FreeRectangles[i]));
#endif
        }

        /// Computes the ratio of used surface area to the total bin area.
        private float Occupancy()
        {
            ///\todo The occupancy rate could be cached/tracked incrementally instead
            ///      of looping through the list of packed rectangles here.
            float usedSurfaceArea = 0;
            for (var i = 0; i < UsedRectangles.Count; ++i)
                usedSurfaceArea += UsedRectangles[i].Width * UsedRectangles[i].Height;

            return usedSurfaceArea / (_binWidth * _binHeight);
        }

        private float ScoreBestAreaFit(float width, float height, PixelRect freeRect)
        {
            return freeRect.Width * freeRect.Height - width * height;
        }

        private float ScoreBestLongSideFit(float width, float height, PixelRect freeRect)
        {
            var leftoverHoriz = Math.Abs(freeRect.Width - width);
            var leftoverVert = Math.Abs(freeRect.Height - height);
            var leftover = Math.Max(leftoverHoriz, leftoverVert);
            return leftover;
        }

        private float ScoreBestShortSideFit(float width, float height, PixelRect freeRect)
        {
            var leftoverHoriz = Math.Abs(freeRect.Width - width);
            var leftoverVert = Math.Abs(freeRect.Height - height);
            var leftover = Math.Min(leftoverHoriz, leftoverVert);
            return leftover;
        }

        /// Returns the heuristic score value for placing a rectangle of size Width*Height into freeRect. Does not try to rotate.
        private float ScoreByHeuristic(float width, float height, PixelRect freeRect, FreeRectChoiceHeuristic rectChoice)
        {
            switch (rectChoice)
            {
                case FreeRectChoiceHeuristic.RectBestAreaFit: return ScoreBestAreaFit(width, height, freeRect);
                case FreeRectChoiceHeuristic.RectBestShortSideFit: return ScoreBestShortSideFit(width, height, freeRect);
                case FreeRectChoiceHeuristic.RectBestLongSideFit: return ScoreBestLongSideFit(width, height, freeRect);
                case FreeRectChoiceHeuristic.RectWorstAreaFit: return ScoreWorstAreaFit(width, height, freeRect);
                case FreeRectChoiceHeuristic.RectWorstShortSideFit: return ScoreWorstShortSideFit(width, height, freeRect);
                case FreeRectChoiceHeuristic.RectWorstLongSideFit: return ScoreWorstLongSideFit(width, height, freeRect);
                default:
                    Debug.Assert(false);
                    return float.MaxValue;
            }
        }

        private float ScoreWorstAreaFit(float width, float height, PixelRect freeRect)
        {
            return -ScoreBestAreaFit(width, height, freeRect);
        }

        private float ScoreWorstLongSideFit(float width, float height, PixelRect freeRect)
        {
            return -ScoreBestLongSideFit(width, height, freeRect);
        }

        private float ScoreWorstShortSideFit(float width, float height, PixelRect freeRect)
        {
            return -ScoreBestShortSideFit(width, height, freeRect);
        }

        /// This function will add the two generated rectangles into the freeRectangles array. The caller is expected to
        /// remove the original rectangle from the freeRectangles array after that.
        private void SplitFreeRectAlongAxis(PixelRect freeRect, PixelRect placedRect, bool SplitHorizontal)
        {
            // Form the two new rectangles.
            var bottom = new PixelRect(freeRect.X, freeRect.Y + placedRect.Height, 0, freeRect.Height - placedRect.Height);
            var right = new PixelRect(freeRect.X + placedRect.Width, freeRect.Y, freeRect.Width - placedRect.Width, 0);

            if (SplitHorizontal)
            {
                bottom = new PixelRect(bottom.X, bottom.Y, freeRect.Width, bottom.Height);
                right = new PixelRect(right.X, right.Y, right.Width, placedRect.Height);
            }
            else // Split vertically
            {
                bottom = new PixelRect(bottom.X, bottom.Y, placedRect.Width, bottom.Height);
                right = new PixelRect(right.X, right.Y, right.Width, freeRect.Height);
            }

            // Add the new rectangles into the free rectangle pool if they weren't degenerate.
            if (bottom.Width > 0 && bottom.Height > 0)
                FreeRectangles.Add(bottom);
            if (right.Width > 0 && right.Height > 0)
                FreeRectangles.Add(right);

#if _DEBUG
            Debug.Assert(_disjointRects.Disjoint(bottom));
            Debug.Assert(_disjointRects.Disjoint(right));
#endif
        }

        private void SplitFreeRectByHeuristic(PixelRect freeRect, PixelRect placedRect, GuillotineSplitHeuristic method)
        {
            // Compute the lengths of the leftover area.
            var w = freeRect.Width - placedRect.Width;
            var h = freeRect.Height - placedRect.Height;

            // Placing placedRect into freeRect results in an L-shaped free area, which must be split into
            // two disjoint rectangles. This can be achieved with by splitting the L-shape using a single line.
            // We have two choices: horizontal or vertical.	

            // Use the given heuristic to decide which choice to make.

            bool splitHorizontal;
            switch (method)
            {
                case GuillotineSplitHeuristic.SplitShorterLeftoverAxis:
                    // Split along the shorter leftover axis.
                    splitHorizontal = w <= h;
                    break;
                case GuillotineSplitHeuristic.SplitLongerLeftoverAxis:
                    // Split along the longer leftover axis.
                    splitHorizontal = w > h;
                    break;
                case GuillotineSplitHeuristic.SplitMinimizeArea:
                    // Maximize the larger area == minimize the smaller area.
                    // Tries to make the single bigger rectangle.
                    splitHorizontal = placedRect.Width * h > w * placedRect.Height;
                    break;
                case GuillotineSplitHeuristic.SplitMaximizeArea:
                    // Maximize the smaller area == minimize the larger area.
                    // Tries to make the rectangles more even-sized.
                    splitHorizontal = placedRect.Width * h <= w * placedRect.Height;
                    break;
                case GuillotineSplitHeuristic.SplitShorterAxis:
                    // Split along the shorter total axis.
                    splitHorizontal = freeRect.Width <= freeRect.Height;
                    break;
                case GuillotineSplitHeuristic.SplitLongerAxis:
                    // Split along the longer total axis.
                    splitHorizontal = freeRect.Width > freeRect.Height;
                    break;
                default:
                    splitHorizontal = true;
                    Debug.Assert(false);
                    break;
            }

            // Perform the actual split.
            SplitFreeRectAlongAxis(freeRect, placedRect, splitHorizontal);
        }
    }

    internal enum GuillotineSplitHeuristic
    {
        SplitShorterLeftoverAxis,

        SplitLongerLeftoverAxis,

        SplitMinimizeArea,

        SplitMaximizeArea,

        SplitShorterAxis,

        SplitLongerAxis
    }

    internal enum FreeRectChoiceHeuristic
    {
        RectBestAreaFit,

        RectBestShortSideFit,

        RectBestLongSideFit,

        RectWorstAreaFit,

        RectWorstShortSideFit,

        RectWorstLongSideFit
    }
}