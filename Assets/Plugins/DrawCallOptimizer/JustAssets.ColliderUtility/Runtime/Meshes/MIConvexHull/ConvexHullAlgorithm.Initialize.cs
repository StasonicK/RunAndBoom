/******************************************************************************
 *
 * The MIT License (MIT)
 *
 * MIConvexHull, Copyright (c) 2015 David Sehnal, Matthew Campbell
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *  
 *****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace JustAssets.ColliderUtilityRuntime.MIConvexHull
{
    /*
     * This part of the implementation handles initialization of the convex hull algorithm:
     * - Constructor & Process initiation 
     * - Determine the dimension by looking at length of Position vector of 10 random data points from the input. 
     * - Identify bounding box points in each direction.
     * - Pick (Dimension + 1) points from the extremes and construct the initial simplex.
     */

    /// <summary>
    /// Class ConvexHullAlgorithm.
    /// </summary>
    internal partial class ConvexHullAlgorithm
    {
        #region Starting functions and constructor


        /// <summary>
        /// Initializes a new instance of the <see cref="ConvexHullAlgorithm" /> class.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="lift">if set to <c>true</c> [lift].</param>
        /// <param name="planeDistanceTolerance">The plane distance tolerance.</param>
        /// <exception cref="System.InvalidOperationException">Dimension of the input must be 2 or greater.</exception>
        /// <exception cref="System.ArgumentException">There are too few vertices (m) for the n-dimensional space. (m must be greater  +
        /// than the n, but m is  + NumberOfVertices +  and n is  + NumOfDimensions</exception>
        /// <exception cref="InvalidOperationException">PointTranslationGenerator cannot be null if PointTranslationType is enabled.
        /// or
        /// Dimension of the input must be 2 or greater.</exception>
        /// <exception cref="ArgumentException">There are too few vertices (m) for the n-dimensional space. (m must be greater " +
        /// "than the n, but m is " + NumberOfVertices + " and n is " + Dimension</exception>
        internal ConvexHullAlgorithm(IVertex[] vertices, bool lift, double planeDistanceTolerance)
        {
            _isLifted = lift;
            _vertices = vertices;
            _numberOfVertices = vertices.Length;

            NumOfDimensions = DetermineDimension();
            if (_isLifted) NumOfDimensions++;
            if (NumOfDimensions < 2) throw new ConvexHullGenerationException(ConvexHullCreationResultOutcome.DimensionSmallerTwo, "Dimension of the input must be 2 or greater.");
            if (NumOfDimensions == 2) throw new ConvexHullGenerationException(ConvexHullCreationResultOutcome.DimensionTwoWrongMethod, "Dimension of the input is 2. Thus you should use the Create2D method" +
                 " instead of the Create.");
            if (_numberOfVertices <= NumOfDimensions)
                throw new ConvexHullGenerationException(ConvexHullCreationResultOutcome.NotEnoughVerticesForDimension,
                    "There are too few vertices (m) for the n-dimensional space. (m must be greater " +
                    "than the n, but m is " + _numberOfVertices + " and n is " + NumOfDimensions);
            this._planeDistanceTolerance = planeDistanceTolerance;
            _unprocessedFaces = new FaceList();
            _convexFaces = new IndexBuffer();

            FacePool = new ConvexFaceInternal[(NumOfDimensions + 1) * 10]; // must be initialized before object manager
            AffectedFaceFlags = new bool[(NumOfDimensions + 1) * 10];
            _objectManager = new ObjectManager(this);

            _insidePoint = new double[NumOfDimensions];
            _traverseStack = new IndexBuffer();
            _updateBuffer = new int[NumOfDimensions];
            _updateIndices = new int[NumOfDimensions];
            _emptyBuffer = new IndexBuffer();
            _affectedFaceBuffer = new IndexBuffer();
            _coneFaceBuffer = new SimpleList<DeferredFace>();
            _singularVertices = new HashSet<int>();
            _beyondBuffer = new IndexBuffer();

            _connectorTable = new ConnectorList[Constants.ConnectorTableSize];
            for (int i = 0; i < Constants.ConnectorTableSize; i++) _connectorTable[i] = new ConnectorList();

            _vertexVisited = new bool[_numberOfVertices];
            _positions = new double[_numberOfVertices * NumOfDimensions];
            _boundingBoxPoints = new List<int>();
            _minima = new double[NumOfDimensions];
            _maxima = new double[NumOfDimensions];
            _mathHelper = new MathHelper(NumOfDimensions, _positions);
        }
        /// <summary>
        /// Check the dimensionality of the input data.
        /// </summary>
        /// <returns>System.Int32.</returns>
        /// <exception cref="ArgumentException">Invalid input data (non-uniform dimension).</exception>
        private int DetermineDimension()
        {
            Random r = new Random();
            List<int> dimensions = new List<int>();
            for (int i = 0; i < 10; i++)
                dimensions.Add(_vertices[r.Next(_numberOfVertices)].Position.Length);
            int dimension = dimensions.Min();
            if (dimension != dimensions.Max())
                throw new ConvexHullGenerationException(ConvexHullCreationResultOutcome.NonUniformDimension, "Invalid input data (non-uniform dimension).");
            return dimension;
        }


        /// <summary>
        /// Gets/calculates the convex hull. This is 
        /// </summary>
        internal void GetConvexHull()
        {
            // accessing a 1D array is quicker than a jagged array, so the first step is to make this array
            SerializeVerticesToPositions();
            // next the bounding box extremes are found. This is used to shift, scale and find the starting simplex.
            FindBoundingBoxPoints();
            // the positions are shifted to avoid divide by zero problems
            // and if Delaunay or Voronoi, then the parabola terms are scaled back to match the size of the other coords
            ShiftAndScalePositions();
            // Find the (dimension+1) initial points and create the simplexes.
            CreateInitialSimplex();

            // Now, the main loop. These initial faces of a simplex are replaced and expanded 
            // outwards to make the convex hull and faces.
            while (_unprocessedFaces.First != null)
            {
                ConvexFaceInternal currentFace = _unprocessedFaces.First;
                _currentVertex = currentFace.FurthestVertex;

                // The affected faces get tagged
                TagAffectedFaces(currentFace);

                // Create the cone from the currentVertex and the affected faces horizon.
                if (!_singularVertices.Contains(_currentVertex) && CreateCone()) CommitCone();
                else HandleSingular();

                // Need to reset the tags
                int count = _affectedFaceBuffer.Count;
                for (int i = 0; i < count; i++) AffectedFaceFlags[_affectedFaceBuffer[i]] = false;
            }
        }

        #endregion

        /// <summary>
        /// Serializes the vertices into the 1D array, Positions. The 1D array has much quicker access in C#.
        /// </summary>
        private void SerializeVerticesToPositions()
        {
            int index = 0;
            if (_isLifted) // "Lifted" means that the last dimension is the sum of the squares of the others.
            {
                foreach (IVertex v in _vertices)
                {
                    double parabolaTerm = 0.0; // the lifted term is a sum of squares.
                    int origNumDim = NumOfDimensions - 1;
                    for (int i = 0; i < origNumDim; i++)
                    {
                        double coordinate = v.Position[i];
                        _positions[index++] = coordinate;
                        parabolaTerm += coordinate * coordinate;
                    }
                    _positions[index++] = parabolaTerm;
                }
            }
            else
                foreach (IVertex v in _vertices)
                {
                    for (int i = 0; i < NumOfDimensions; i++)
                        _positions[index++] = v.Position[i];
                }
        }



        /// <summary>
        /// Finds the bounding box points.
        /// </summary>
        private void FindBoundingBoxPoints()
        {
            for (int i = 0; i < NumOfDimensions; i++)
            {
                List<int> minIndices = new List<int>();
                List<int> maxIndices = new List<int>();
                double min = double.PositiveInfinity, max = double.NegativeInfinity;
                for (int j = 0; j < _numberOfVertices; j++)
                {
                    double v = GetCoordinate(j, i);
                    if (v < min)
                    {
                        // you found a better solution than before, clear out the list and store new value
                        min = v;
                        minIndices.Clear();
                        minIndices.Add(j);
                    }
                    else if (v == min)
                    {
                        //same or almost as good as current limit, so store it
                        minIndices.Add(j);
                    }
                    if (v > max)
                    {
                        // you found a better solution than before, clear out the list and store new value
                        max = v;
                        maxIndices.Clear();
                        maxIndices.Add(j);
                    }
                    else if (v == max)
                    {
                        //same or almost as good as current limit, so store it
                        maxIndices.Add(j);
                    }
                }
                _minima[i] = min;
                _maxima[i] = max;
                _boundingBoxPoints.AddRange(minIndices);
                _boundingBoxPoints.AddRange(maxIndices);
            }
            _boundingBoxPoints = _boundingBoxPoints.Distinct().ToList();
        }

        /// <summary>
        /// Shifts and scales the Positions to avoid future errors. This does not alter the original data.
        /// </summary>
        private void ShiftAndScalePositions()
        {
            int positionsLength = _positions.Length;
            if (_isLifted)
            {
                int origNumDim = NumOfDimensions - 1;
                double parabolaScale = 2 / (_minima.Sum(x => Math.Abs(x)) + _maxima.Sum(x => Math.Abs(x))
                                            - Math.Abs(_maxima[origNumDim]) - Math.Abs(_minima[origNumDim]));
                // the parabola scale is 1 / average of the sum of the other dimensions.
                // multiplying this by the parabola will scale it back to be on near similar size to the
                // other dimensions. Without this, the term is much larger than the others, which causes
                // problems for roundoff error and finding the normal of faces.
                _minima[origNumDim] *= parabolaScale; // change the extreme values as well
                _maxima[origNumDim] *= parabolaScale;
                // it is done here because
                for (int i = origNumDim; i < positionsLength; i += NumOfDimensions)
                    _positions[i] *= parabolaScale;
            }
            double[] shiftAmount = new double[NumOfDimensions];
            for (int i = 0; i < NumOfDimensions; i++)
                // now the entire model is shifted to all positive numbers...plus some more.
                // why? 
                // 1) to avoid dealing with a point at the origin {0,0,...,0} which causes problems 
                //    for future normal finding
                // 2) note that weird shift that is used (max - min - min). This is to avoid scaling
                //    issues. this shift means that the minima in a dimension will always be a positive
                //    number (no points at zero), and the minima [in a given dimension] will always be
                //    half of the maxima. 'Half' is much preferred to 'thousands of times'
                //    Think of the first term as the range (max - min), then the second term avoids cases
                //    where there are both positive and negative numbers.
                if (_maxima[i] == _minima[i]) shiftAmount[i] = 0.0;
                else shiftAmount[i] = (_maxima[i] - _minima[i]) - _minima[i];
            for (int i = 0; i < positionsLength; i++)
                _positions[i] += shiftAmount[i % NumOfDimensions];
        }

        /// <summary>
        /// Find the (dimension+1) initial points and create the simplexes.
        /// Creates the initial simplex of n+1 vertices by using points from the bounding box.
        /// Special care is taken to ensure that the vertices chosen do not result in a degenerate shape
        /// where vertices are collinear (co-planar, etc). This would technically be resolved when additional
        /// vertices are checked in the main loop, but: 1) a degenerate simplex would not eliminate any other
        /// vertices (thus no savings there), 2) the creation of the face normal is prone to error.
        /// </summary>
        private void CreateInitialSimplex()
        {
            List<int> initialPoints = FindInitialPoints();
            #region Create the first faces from (dimension + 1) vertices.

            int[] faces = new int[NumOfDimensions + 1];

            for (int i = 0; i < NumOfDimensions + 1; i++)
            {
                int[] vertices = new int[NumOfDimensions];
                for (int j = 0, k = 0; j <= NumOfDimensions; j++)
                {
                    if (i != j) vertices[k++] = initialPoints[j];
                }
                ConvexFaceInternal newFace = FacePool[_objectManager.GetFace()];
                newFace.Vertices = vertices;
                Array.Sort(vertices);
                _mathHelper.CalculateFacePlane(newFace, _insidePoint);
                faces[i] = newFace.Index;
            }
            // update the adjacency (check all pairs of faces)
            for (int i = 0; i < NumOfDimensions; i++)
                for (int j = i + 1; j < NumOfDimensions + 1; j++) 
                    UpdateAdjacency(FacePool[faces[i]], FacePool[faces[j]]);
            foreach (int item in initialPoints)
                _vertexVisited[item] = true;
            #endregion

            #region Init the vertex beyond buffers.

            foreach (int faceIndex in faces)
            {
                ConvexFaceInternal face = FacePool[faceIndex];
                FindBeyondVertices(face);
                if (face.VerticesBeyond.Count == 0) _convexFaces.Add(face.Index); // The face is on the hull
                else _unprocessedFaces.Add(face);
            }

            #endregion

            // Set all vertices to false (unvisited).
            // foreach (var vertex in initialPoints) VertexVisited[vertex] = false;
        }
        const int NumberOfInitSimplicesToTest = 5;
        private List<int> FindInitialPoints()
        {
            // given the way that the algorithm works, points that are put on the convex hull are not
            // removed. So it is important that we start with a simplex of points guaranteed to be on the
            // convex hull. This is where the bounding box points come in.
            double negligibleVolume = Constants.FractionalNegligibleVolume;
            for (int i = 0; i < NumOfDimensions; i++)
            {
                negligibleVolume *= _maxima[i] - _minima[i];
            }
            List<int> bestVertexIndices = null;
            int numBbPoints = _boundingBoxPoints.Count;
            bool degenerate = true;
            if ((NumOfDimensions + 1) > numBbPoints)
            {  //if there are fewer bounding box points than what is needed for the simplex (this is quite
               //rare), then add the ones farthest form the centroid of the current bounding box points.
                _boundingBoxPoints = AddNVerticesFarthestToCentroid(_boundingBoxPoints, NumOfDimensions + 1);
            }
            if ((NumOfDimensions + 1) == numBbPoints)
            {   // if the number of points is the same then just go with these
                bestVertexIndices = _boundingBoxPoints;
                degenerate = _mathHelper.VolumeOfSimplex(_boundingBoxPoints) <= negligibleVolume;
            }
            else if ((NumOfDimensions + 1) < numBbPoints)
            {   //if there are more bounding box points than needed, call the following function to find a 
                // random one that has a large volume.
                double volume;
                bestVertexIndices = FindLargestRandomSimplex(_boundingBoxPoints, _boundingBoxPoints, out volume);
                degenerate = volume <= negligibleVolume;
            }
            if (degenerate)
            {   // if it turns out to still be degenerate, then increase the check to include all vertices.
                // this is potentially expensive, but we don't have a choice.
                double volume;
                bestVertexIndices = FindLargestRandomSimplex(_boundingBoxPoints, Enumerable.Range(0, _numberOfVertices), out volume);
                degenerate = volume <= negligibleVolume;
            }
            if (degenerate) throw new ConvexHullGenerationException(ConvexHullCreationResultOutcome.DegenerateData,
                  "Failed to find initial simplex shape with non-zero volume. While data appears to be in " + NumOfDimensions +
                  " dimensions, the data is all co-planar (or collinear, co-hyperplanar) and is representable by fewer dimensions.");
            _insidePoint = CalculateVertexCentriod(bestVertexIndices);
            return bestVertexIndices;
        }
        private List<int> FindLargestRandomSimplex(IList<int> bbPoints, IEnumerable<int> otherPoints, out double volume)
        {
            Random random = new Random(1);
            List<int> bestVertexIndices = null;
            double maxVolume = 0.0;
            volume = 0.0;
            int numBbPoints = bbPoints.Count;
            for (int i = 0; i < NumberOfInitSimplicesToTest; i++)
            {
                List<int> vertexIndices = new List<int>();
                HashSet<int> alreadyChosenIndices = new HashSet<int>();
                int numRandomIndices = (2 * NumOfDimensions <= numBbPoints)
                    ? NumOfDimensions : numBbPoints - NumOfDimensions;
                while (alreadyChosenIndices.Count < numRandomIndices)
                {
                    int index = random.Next(numBbPoints);
                    if (alreadyChosenIndices.Contains(index)) continue;
                    alreadyChosenIndices.Add(index);
                }
                if (2 * NumOfDimensions <= numBbPoints)
                    foreach (int index in alreadyChosenIndices)
                        vertexIndices.Add(bbPoints[index]);
                else
                {
                    for (int j = 0; j < numBbPoints; j++)
                        if (!alreadyChosenIndices.Contains(j))
                            vertexIndices.Add(bbPoints[j]);
                }
                ConvexFaceInternal plane = new ConvexFaceInternal(NumOfDimensions, 0, new IndexBuffer());
                plane.Vertices = vertexIndices.ToArray();
                _mathHelper.CalculateFacePlane(plane, new double[NumOfDimensions]);
                // this next line is the only difference between this subroutine and the one
                int newVertex = FindFarthestPoint(otherPoints, plane);
                if (newVertex == -1) continue;
                vertexIndices.Add(newVertex);
                volume = _mathHelper.VolumeOfSimplex(vertexIndices);
                if (maxVolume < volume)
                {
                    maxVolume = volume;
                    bestVertexIndices = vertexIndices;
                }
            }
            volume = maxVolume;
            return bestVertexIndices;
        }
        private int FindFarthestPoint(IEnumerable<int> vertexIndices, ConvexFaceInternal plane)
        {
            double maxDistance = 0.0;
            int farthestVertexIndex = -1;
            foreach (int v in vertexIndices)
            {
                double distance = Math.Abs(_mathHelper.GetVertexDistance(v, plane));
                if (maxDistance < distance)
                {
                    maxDistance = distance;
                    farthestVertexIndex = v;
                }
            }
            return farthestVertexIndex;
        }

        private List<int> AddNVerticesFarthestToCentroid(List<int> vertexIndices, int n)
        {
            List<int> newVertsList = new List<int>(vertexIndices);
            while (newVertsList.Count < n)
            {
                double[] centroid = CalculateVertexCentriod(newVertsList);
                double maxDistance = 0.0;
                int newVert = -1;
                for (int v = 0; v < _numberOfVertices; v++)
                {
                    if (newVertsList.Contains(v)) continue;
                    double distanceSquared = 0.0;
                    for (int i = 0; i < NumOfDimensions; i++)
                    {
                        double d = centroid[i] - _positions[i + NumOfDimensions * v];
                        distanceSquared = d * d;
                    }
                    if (maxDistance < distanceSquared)
                    {
                        maxDistance = distanceSquared;
                        newVert = v;
                    }
                }
                newVertsList.Add(newVert);
            }
            return newVertsList;
        }

        private double[] CalculateVertexCentriod(IList<int> vertexIndices)
        {
            int numPoints = vertexIndices.Count;
            double[] centroid = new double[NumOfDimensions];
            for (int i = 0; i < NumOfDimensions; i++)
            {
                for (int j = 0; j < numPoints; j++)
                    centroid[i] += this._positions[i + NumOfDimensions * vertexIndices[j]];
                centroid[i] /= numPoints;
            }
            return centroid;
        }
        /// <summary>
        /// Check if 2 faces are adjacent and if so, update their AdjacentFaces array.
        /// </summary>
        /// <param name="l">The l.</param>
        /// <param name="r">The r.</param>
        private void UpdateAdjacency(ConvexFaceInternal l, ConvexFaceInternal r)
        {
            int[] lv = l.Vertices;
            int[] rv = r.Vertices;
            int i;

            // reset marks on the 1st face
            for (i = 0; i < NumOfDimensions; i++) _vertexVisited[lv[i]] = false;

            // mark all vertices on the 2nd face
            for (i = 0; i < NumOfDimensions; i++) _vertexVisited[rv[i]] = true;

            // find the 1st false index
            for (i = 0; i < NumOfDimensions; i++) if (!_vertexVisited[lv[i]]) break;

            // no vertex was marked
            if (i == NumOfDimensions) return;

            // check if only 1 vertex wasn't marked
            for (int j = i + 1; j < lv.Length; j++) if (!_vertexVisited[lv[j]]) return;

            // if we are here, the two faces share an edge
            l.AdjacentFaces[i] = r.Index;

            // update the adj. face on the other face - find the vertex that remains marked
            for (i = 0; i < NumOfDimensions; i++) _vertexVisited[lv[i]] = false;
            for (i = 0; i < NumOfDimensions; i++)
            {
                if (_vertexVisited[rv[i]]) break;
            }
            r.AdjacentFaces[i] = l.Index;
        }

        /// <summary>
        /// Used in the "initialization" code.
        /// </summary>
        /// <param name="face">The face.</param>
        private void FindBeyondVertices(ConvexFaceInternal face)
        {
            IndexBuffer beyondVertices = face.VerticesBeyond;
            _maxDistance = double.NegativeInfinity;
            _furthestVertex = 0;
            for (int i = 0; i < _numberOfVertices; i++)
            {
                if (_vertexVisited[i]) continue;
                IsBeyond(face, beyondVertices, i);
            }

            face.FurthestVertex = _furthestVertex;
        }
        #region Fields

        /// <summary>
        /// Corresponds to the dimension of the data.
        /// When the "lifted" hull is computed, Dimension is automatically incremented by one.
        /// </summary>
        internal readonly int NumOfDimensions;

        /// <summary>
        /// Are we on a paraboloid?
        /// </summary>
        private readonly bool _isLifted;

        /// <summary>
        /// Explained in ConvexHullComputationConfig.
        /// </summary>
        private readonly double _planeDistanceTolerance;

        /*
         * Representation of the input vertices.
         * 
         * - In the algorithm, a vertex is represented by its index in the Vertices array.
         *   This makes the algorithm a lot faster (up to 30%) than using object reference everywhere.
         * - Positions are stored as a single array of values. Coordinates for vertex with index i
         *   are stored at indices <i * Dimension, (i + 1) * Dimension)
         * - VertexMarks are used by the algorithm to help identify a set of vertices that is "above" (or "beyond") 
         *   a specific face.
         */
        /// <summary>
        /// The vertices
        /// </summary>
        private readonly IVertex[] _vertices;
        /// <summary>
        /// The positions
        /// </summary>
        private double[] _positions;
        /// <summary>
        /// The vertex marks
        /// </summary>
        private readonly bool[] _vertexVisited;

        private readonly int _numberOfVertices;

        /*
         * The triangulation faces are represented in a single pool for objects that are being reused.
         * This allows for represent the faces as integers and significantly speeds up many computations.
         * - AffectedFaceFlags are used to mark affected faces/
         */
        /// <summary>
        /// The face pool
        /// </summary>
        internal ConvexFaceInternal[] FacePool;
        /// <summary>
        /// The affected face flags
        /// </summary>
        internal bool[] AffectedFaceFlags;

        /// <summary>
        /// A list of faces that that are not a part of the final convex hull and still need to be processed.
        /// </summary>
        private readonly FaceList _unprocessedFaces;

        /// <summary>
        /// A list of faces that form the convex hull.
        /// </summary>
        private readonly IndexBuffer _convexFaces;

        /// <summary>
        /// The vertex that is currently being processed.
        /// </summary>
        private int _currentVertex;

        /// <summary>
        /// A helper variable to determine the furthest vertex for a particular convex face.
        /// </summary>
        private double _maxDistance;

        /// <summary>
        /// A helper variable to help determine the index of the vertex that is furthest from the face that is currently being
        /// processed.
        /// </summary>
        private int _furthestVertex;

        /// <summary>
        /// The inside point is used to determine which side of the face is pointing inside
        /// and which is pointing outside. This may be relatively trivial for 3D, but it is
        /// unknown for higher dimensions. It is calculated as the average of the initial
        /// simplex points.
        /// </summary>
        private double[] _insidePoint;

        /*
         * Helper arrays to store faces for adjacency update.
         * This is just to prevent unnecessary allocations.
         */
        /// <summary>
        /// The update buffer
        /// </summary>
        private readonly int[] _updateBuffer;
        /// <summary>
        /// The update indices
        /// </summary>
        private readonly int[] _updateIndices;

        /// <summary>
        /// Used to determine which faces need to be updated at each step of the algorithm.
        /// </summary>
        private readonly IndexBuffer _traverseStack;

        /// <summary>
        /// Used for VerticesBeyond for faces that are on the convex hull.
        /// </summary>
        private readonly IndexBuffer _emptyBuffer;

        /// <summary>
        /// Used to determine which vertices are "above" (or "beyond") a face
        /// </summary>
        private IndexBuffer _beyondBuffer;

        /// <summary>
        /// Stores faces that are visible from the current vertex.
        /// </summary>
        private readonly IndexBuffer _affectedFaceBuffer;

        /// <summary>
        /// Stores faces that form a "cone" created by adding new vertex.
        /// </summary>
        private readonly SimpleList<DeferredFace> _coneFaceBuffer;

        /// <summary>
        /// Stores a list of "singular" (or "generate", "planar", etc.) vertices that cannot be part of the hull.
        /// </summary>
        private readonly HashSet<int> _singularVertices;

        /// <summary>
        /// The connector table helps to determine the adjacency of convex faces.
        /// Hashing is used instead of pairwise comparison. This significantly speeds up the computations,
        /// especially for higher dimensions.
        /// </summary>
        private readonly ConnectorList[] _connectorTable;

        /// <summary>
        /// Manages the memory allocations and storage of unused objects.
        /// Saves the garbage collector a lot of work.
        /// </summary>
        private readonly ObjectManager _objectManager;

        /// <summary>
        /// Helper class for handling math related stuff.
        /// </summary>
        private readonly MathHelper _mathHelper;
        private List<int> _boundingBoxPoints;
        private readonly double[] _minima;
        private readonly double[] _maxima;
        #endregion
    }
}