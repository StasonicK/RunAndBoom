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

namespace JustAssets.ColliderUtilityRuntime.MIConvexHull
{
    /// <summary>
    /// A helper class mostly for normal computation. If convex hulls are computed
    /// in higher dimensions, it might be a good idea to add a specific
    /// FindNormalVectorND function.
    /// </summary>
    internal class MathHelper
    {
        /// <summary>
        /// The dimension
        /// </summary>
        private readonly int _dimension;
        /// <summary>
        /// The matrix pivots
        /// </summary>
        private readonly int[] _matrixPivots;
        /// <summary>
        /// The n d matrix
        /// </summary>
        private readonly double[] _nDMatrix;
        /// <summary>
        /// The n d normal helper vector
        /// </summary>
        private readonly double[] _nDNormalHelperVector;

        /// <summary>
        /// The nt x
        /// </summary>
        private readonly double[] _ntX;
        /// <summary>
        /// The nt y
        /// </summary>
        private readonly double[] _ntY;
        /// <summary>
        /// The nt z
        /// </summary>
        private readonly double[] _ntZ;

        /// <summary>
        /// The position data
        /// </summary>
        private readonly double[] _positionData;

        /// <summary>
        /// Initializes a new instance of the <see cref="MathHelper"/> class.
        /// </summary>
        /// <param name="dimension">The dimension.</param>
        /// <param name="positions">The positions.</param>
        internal MathHelper(int dimension, double[] positions)
        {
            _positionData = positions;
            _dimension = dimension;

            _ntX = new double[_dimension];
            _ntY = new double[_dimension];
            _ntZ = new double[_dimension];

            _nDNormalHelperVector = new double[_dimension];
            _nDMatrix = new double[_dimension * _dimension];
            _matrixPivots = new int[_dimension];
        }

        /// <summary>
        /// Calculates the normal and offset of the hyper-plane given by the face's vertices.
        /// </summary>
        /// <param name="face">The face.</param>
        /// <param name="center">The center.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool CalculateFacePlane(ConvexFaceInternal face, double[] center)
        {
            int[] vertices = face.Vertices;
            double[] normal = face.Normal;
            FindNormalVector(vertices, normal);

            if (double.IsNaN(normal[0]))
            {
                return false;
            }

            double offset = 0.0;
            double centerDistance = 0.0;
            int fi = vertices[0] * _dimension;
            for (int i = 0; i < _dimension; i++)
            {
                double n = normal[i];
                offset += n * _positionData[fi + i];
                centerDistance += n * center[i];
            }
            face.Offset = -offset;
            centerDistance -= offset;

            if (centerDistance > 0)
            {
                for (int i = 0; i < _dimension; i++) normal[i] = -normal[i];
                face.Offset = offset;
                face.IsNormalFlipped = true;
            }
            else face.IsNormalFlipped = false;

            return true;
        }

        /// <summary>
        /// Check if the vertex is "visible" from the face.
        /// The vertex is "over face" if the return value is &gt; Constants.PlaneDistanceTolerance.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="f">The f.</param>
        /// <returns>The vertex is "over face" if the result is positive.</returns>
        internal double GetVertexDistance(int v, ConvexFaceInternal f)
        {
            double[] normal = f.Normal;
            int x = v * _dimension;
            double distance = f.Offset;
            for (int i = 0; i < normal.Length; i++)
                distance += normal[i] * _positionData[x + i];
            return distance;
        }

        /// <summary>
        /// Returns the vector the between vertices.
        /// </summary>
        /// <param name="toIndex">To index.</param>
        /// <param name="fromIndex">From index.</param>
        /// <returns>System.Double[].</returns>
        internal double[] VectorBetweenVertices(int toIndex, int fromIndex)
        {
            double[] target = new double[_dimension];
            VectorBetweenVertices(toIndex, fromIndex, target);
            return target;
        }
        /// <summary>
        /// Returns the vector the between vertices.
        /// </summary>
        /// <param name="fromIndex">From index.</param>
        /// <param name="toIndex">To index.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        private void VectorBetweenVertices(int toIndex, int fromIndex, double[] target)
        {
            int u = toIndex * _dimension, v = fromIndex * _dimension;
            for (int i = 0; i < _dimension; i++)
            {
                target[i] = _positionData[u + i] - _positionData[v + i];
            }
        }

        #region Find the normal vector of the face
        /// <summary>
        /// Finds normal vector of a hyper-plane given by vertices.
        /// Stores the results to normalData.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="normalData">The normal data.</param>
        private void FindNormalVector(int[] vertices, double[] normalData)
        {
            switch (_dimension)
            {
                case 2:
                    FindNormalVector2D(vertices, normalData);
                    break;
                case 3:
                    FindNormalVector3D(vertices, normalData);
                    break;
                case 4:
                    FindNormalVector4D(vertices, normalData);
                    break;
                default:
                    FindNormalVectorNd(vertices, normalData);
                    break;
            }
        }
        /// <summary>
        /// Finds 2D normal vector.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="normal">The normal.</param>
        private void FindNormalVector2D(int[] vertices, double[] normal)
        {
            VectorBetweenVertices(vertices[1], vertices[0], _ntX);

            double nx = -_ntX[1];
            double ny = _ntX[0];

            double norm = Math.Sqrt(nx * nx + ny * ny);

            double f = 1.0 / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
        }
        /// <summary>
        /// Finds 3D normal vector.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="normal">The normal.</param>
        private void FindNormalVector3D(int[] vertices, double[] normal)
        {
            VectorBetweenVertices(vertices[1], vertices[0], _ntX);
            VectorBetweenVertices(vertices[2], vertices[1], _ntY);

            double nx = _ntX[1] * _ntY[2] - _ntX[2] * _ntY[1];
            double ny = _ntX[2] * _ntY[0] - _ntX[0] * _ntY[2];
            double nz = _ntX[0] * _ntY[1] - _ntX[1] * _ntY[0];

            double norm = Math.Sqrt(nx * nx + ny * ny + nz * nz);

            double f = 1.0 / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
            normal[2] = f * nz;
        }
        /// <summary>
        /// Finds 4D normal vector.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="normal">The normal.</param>
        private void FindNormalVector4D(int[] vertices, double[] normal)
        {
            VectorBetweenVertices(vertices[1], vertices[0], _ntX);
            VectorBetweenVertices(vertices[2], vertices[1], _ntY);
            VectorBetweenVertices(vertices[3], vertices[2], _ntZ);

            double[] x = _ntX;
            double[] y = _ntY;
            double[] z = _ntZ;

            // This was generated using Mathematica
            double nx = x[3] * (y[2] * z[1] - y[1] * z[2])
                        + x[2] * (y[1] * z[3] - y[3] * z[1])
                        + x[1] * (y[3] * z[2] - y[2] * z[3]);
            double ny = x[3] * (y[0] * z[2] - y[2] * z[0])
                        + x[2] * (y[3] * z[0] - y[0] * z[3])
                        + x[0] * (y[2] * z[3] - y[3] * z[2]);
            double nz = x[3] * (y[1] * z[0] - y[0] * z[1])
                        + x[1] * (y[0] * z[3] - y[3] * z[0])
                        + x[0] * (y[3] * z[1] - y[1] * z[3]);
            double nw = x[2] * (y[0] * z[1] - y[1] * z[0])
                        + x[1] * (y[2] * z[0] - y[0] * z[2])
                        + x[0] * (y[1] * z[2] - y[2] * z[1]);

            double norm = Math.Sqrt(nx * nx + ny * ny + nz * nz + nw * nw);

            double f = 1.0 / norm;
            normal[0] = f * nx;
            normal[1] = f * ny;
            normal[2] = f * nz;
            normal[3] = f * nw;
        }

        /// <summary>
        /// Finds the normal vector nd.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="normal">The normal.</param>
        private void FindNormalVectorNd(int[] vertices, double[] normal)
        {
            /* We need to solve the matrix A n = B where
             *  - A contains coordinates of vertices as columns
             *  - B is vector with all 1's. Really, it should be the distance of 
             *      the plane from the origin, but - since we're not worried about that
             *      here and we will normalize the normal anyway - all 1's suffices.
             */
            int[] iPiv = _matrixPivots;
            double[] data = _nDMatrix;
            double norm = 0.0;

            // Solve determinants by replacing x-th column by all 1.
            for (int x = 0; x < _dimension; x++)
            {
                for (int i = 0; i < _dimension; i++)
                {
                    int offset = vertices[i] * _dimension;
                    for (int j = 0; j < _dimension; j++)
                    {
                        // maybe I got the i/j mixed up here regarding the representation Math.net uses...
                        // ...but it does not matter since Det(A) = Det(Transpose(A)).
                        data[_dimension * i + j] = j == x ? 1.0 : _positionData[offset + j];
                    }
                }
                LuFactor(data, _dimension, iPiv, _nDNormalHelperVector);
                double coord = 1.0;
                for (int i = 0; i < _dimension; i++)
                {
                    if (iPiv[i] != i) coord *= -data[_dimension * i + i]; // the determinant sign changes on row swap.
                    else coord *= data[_dimension * i + i];
                }
                normal[x] = coord;
                norm += coord * coord;
            }

            // Normalize the result
            double f = 1.0 / Math.Sqrt(norm);
            for (int i = 0; i < normal.Length; i++) normal[i] *= f;
        }
        #endregion

        #region Simplex Volume

        /// <summary>
        /// Determinants the volume of the simplex.
        /// </summary>
        /// <param name="A">a.</param>
        /// <returns>System.Double.</returns>
        internal double VolumeOfSimplex(IList<int> vertexIndices)
        {
            //DebugPrintVertices(vertexIndices);
            // this is the Cayley-Menger determinant, so a matrix is defined that is numDimensions+2
            int numRowCol = _dimension + 2;
            double[] a = new double[numRowCol * numRowCol];
            for (int i = 1; i < numRowCol; i++)
            {
                a[i] = 1;
                a[i * numRowCol] = 1;
            }
            for (int i = 0; i <= _dimension; i++)
                for (int j = i + 1; j <= _dimension; j++)
                {
                    double[] d = VectorBetweenVertices(vertexIndices[i], vertexIndices[j]);
                    double distanceSquared = 0.0;
                    for (int k = 0; k < _dimension; k++)
                        distanceSquared += d[k] * d[k];
                    a[(i + 1) + (j + 1) * numRowCol] = distanceSquared;
                    a[(j + 1) + (i + 1) * numRowCol] = distanceSquared;
                }
            int[] iPiv = new int[2 + _dimension];
            double[] helper = new double[2 + _dimension];
            // determinant(A, 2 + Dimension);  //, iPiv, helper);
            LuFactor(a, 2 + _dimension, iPiv, helper);
            double det = 1.0;
            for (int i = 0; i < iPiv.Length; i++)
            {
                det *= a[(2 + _dimension) * i + i];
                if (iPiv[i] != i) det *= -1; // the determinant sign changes on row swap.
            }
            double denom = Math.Pow(2, _dimension);
            int m = 1;
            while (m++ < _dimension) denom *= m;
            det /= denom;
            if (_dimension % 2 == 0) det *= -1;
            return det;
        }

        #endregion


            // Modified from Math.NET
            // Copyright (c) 2009-2013 Math.NET
            /// <summary>
            /// Lus the factor.
            /// </summary>
            /// <param name="data">The data.</param>
            /// <param name="order">The order.</param>
            /// <param name="ipiv">The ipiv.</param>
            /// <param name="vecLUcolj">The vec l ucolj.</param>
            private static void LuFactor(IList<double> data, int order, int[] ipiv, double[] vecLUcolj)
            {
                // Initialize the pivot matrix to the identity permutation.
                for (int i = 0; i < order; i++)
                {
                    ipiv[i] = i;
                }

                // Outer loop.
                for (int j = 0; j < order; j++)
                {
                    int indexj = j * order;
                    int indexjj = indexj + j;

                    // Make a copy of the j-th column to localize references.
                    for (int i = 0; i < order; i++)
                    {
                        vecLUcolj[i] = data[indexj + i];
                    }

                    // Apply previous transformations.
                    for (int i = 0; i < order; i++)
                    {
                        // Most of the time is spent in the following dot product.
                        int kmax = Math.Min(i, j);
                        double s = 0.0;
                        for (int k = 0; k < kmax; k++)
                        {
                            s += data[k * order + i] * vecLUcolj[k];
                        }

                        data[indexj + i] = vecLUcolj[i] -= s;
                    }

                    // Find pivot and exchange if necessary.
                    int p = j;
                    for (int i = j + 1; i < order; i++)
                    {
                        if (Math.Abs(vecLUcolj[i]) > Math.Abs(vecLUcolj[p]))
                        {
                            p = i;
                        }
                    }

                    if (p != j)
                    {
                        for (int k = 0; k < order; k++)
                        {
                            int indexk = k * order;
                            int indexkp = indexk + p;
                            int indexkj = indexk + j;
                            double temp = data[indexkp];
                            data[indexkp] = data[indexkj];
                            data[indexkj] = temp;
                        }

                        ipiv[j] = p;
                    }

                    // Compute multipliers.
                    if (j < order & data[indexjj] != 0.0)
                    {
                        for (int i = j + 1; i < order; i++)
                        {
                            data[indexj + i] /= data[indexjj];
                        }
                    }
                }
            }
        }
    }