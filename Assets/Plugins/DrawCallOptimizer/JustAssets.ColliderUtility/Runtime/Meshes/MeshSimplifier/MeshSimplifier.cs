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

#region Original License
/////////////////////////////////////////////
//
// Mesh Simplification Tutorial
//
// (C) by Sven Forstmann in 2014
//
// License : MIT
// http://opensource.org/licenses/MIT
//
//https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification
#endregion

#if UNITY_2018_2 || UNITY_2018_3 || UNITY_2018_4 || UNITY_2019
#define UNITY_8UV_SUPPORT
#endif

#if UNITY_2017_3 || UNITY_2017_4 || UNITY_2018 || UNITY_2019
#define UNITY_MESH_INDEXFORMAT_SUPPORT
#endif

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JustAssets.ColliderUtilityRuntime.MeshSimplifier.Internal;
using JustAssets.ColliderUtilityRuntime.MeshSimplifier.Math;
using JustAssets.ColliderUtilityRuntime.MeshSimplifier.Utility;
using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime.MeshSimplifier
{
    /// <summary>
    /// The mesh simplifier.
    /// Deeply based on https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification but rewritten completely in C#.
    /// </summary>
    public sealed class MeshSimplifier
    {
        #region Consts & Static Read-Only
        private const int TriangleEdgeCount = 3;
        private const int TriangleVertexCount = 3;
        private const double DoubleEpsilon = 1.0E-3;
        private static readonly int UVChannelCount = MeshUtils.UVChannelCount;
        #endregion

        #region Fields

        private bool _enableSmartLink = true;
        private int _maxIterationCount = 100;
        private double _agressiveness = 7.0;

        private double _vertexLinkDistanceSqr = double.Epsilon;

        private int[] _subMeshOffsets;
        private ResizableArray<Triangle> _triangles;
        private ResizableArray<Vertex> _vertices;
        private ResizableArray<Ref> _refs;

        private ResizableArray<Vector3> _vertNormals;
        private ResizableArray<Vector4> _vertTangents;
        private UVChannels<Vector2> _vertUV2D;
        private UVChannels<Vector3> _vertUV3D;
        private UVChannels<Vector4> _vertUV4D;
        private ResizableArray<Color> _vertColors;
        private ResizableArray<BoneWeight> _vertBoneWeights;

        private Matrix4x4[] _bindposes;

        // Pre-allocated buffers
        private readonly double[] _errArr = new double[3];
        private readonly int[] _attributeIndexArr = new int[3];
        private readonly HashSet<Triangle> _triangleHashSet1 = new HashSet<Triangle>();
        private readonly HashSet<Triangle> _triangleHashSet2 = new HashSet<Triangle>();
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets if the border edges should be preserved.
        /// Default value: false
        /// </summary>
        public bool PreserveBorderEdges { get; set; }

        /// <summary>
        /// Gets or sets if the UV seam edges should be preserved.
        /// Default value: false
        /// </summary>
        public bool PreserveUVSeamEdges { get; set; }

        /// <summary>
        /// Gets or sets if the UV foldover edges should be preserved.
        /// Default value: false
        /// </summary>
        public bool PreserveUVFoldoverEdges { get; set; }

        /// <summary>
        /// Gets or sets if the discrete curvature of the mesh surface be taken into account during simplification.
        /// Default value: false
        /// </summary>
        public bool PreserveSurfaceCurvature { get; set; }

        /// <summary>
        /// Gets or sets if a feature for smarter vertex linking should be enabled, reducing artifacts in the
        /// decimated result at the cost of a slightly more expensive initialization by treating vertices at
        /// the same position as the same vertex while separating the attributes.
        /// Default value: true
        /// </summary>
        public bool EnableSmartLink
        {
            get { return _enableSmartLink; }
            set { _enableSmartLink = value; }
        }

        /// <summary>
        /// Gets or sets the maximum iteration count. Higher number is more expensive but can bring you closer to your target quality.
        /// Sometimes a lower maximum count might be desired in order to lower the performance cost.
        /// Default value: 100
        /// </summary>
        public int MaxIterationCount
        {
            get { return _maxIterationCount; }
            set { _maxIterationCount = value; }
        }

        /// <summary>
        /// Gets or sets the agressiveness of the mesh simplification. Higher number equals higher quality, but more expensive to run.
        /// Default value: 7.0
        /// </summary>
        public double Agressiveness
        {
            get { return _agressiveness; }
            set { _agressiveness = value; }
        }

        /// <summary>
        /// Gets or sets if verbose information should be printed to the console.
        /// Default value: false
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Gets or sets the vertex positions.
        /// </summary>
        public Vector3[] Vertices
        {
            get
            {
                int vertexCount = _vertices.Length;
                Vector3[] vertices = new Vector3[vertexCount];
                Vertex[] vertArr = _vertices.Data;
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i] = (Vector3)vertArr[i].p;
                }
                return vertices;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _bindposes = null;
                _vertices.Resize(value.Length);
                Vertex[] vertArr = _vertices.Data;
                for (int i = 0; i < value.Length; i++)
                {
                    vertArr[i] = new Vertex(i, value[i]);
                }
            }
        }

        /// <summary>
        /// Gets the count of sub-meshes.
        /// </summary>
        public int SubMeshCount { get; private set; }

        /// <summary>
        /// Gets or sets the vertex normals.
        /// </summary>
        public Vector3[] Normals
        {
            get { return (_vertNormals != null ? _vertNormals.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref _vertNormals, "normals");
            }
        }

        /// <summary>
        /// Gets or sets the vertex tangents.
        /// </summary>
        public Vector4[] Tangents
        {
            get { return (_vertTangents != null ? _vertTangents.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref _vertTangents, "tangents");
            }
        }

        /// <summary>
        /// Gets or sets the vertex colors.
        /// </summary>
        public Color[] Colors
        {
            get { return (_vertColors != null ? _vertColors.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref _vertColors, "colors");
            }
        }

        /// <summary>
        /// Gets or sets the vertex bone weights.
        /// </summary>
        public BoneWeight[] BoneWeights
        {
            get { return (_vertBoneWeights != null ? _vertBoneWeights.Data : null); }
            set
            {
                InitializeVertexAttribute(value, ref _vertBoneWeights, "boneWeights");
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new mesh simplifier.
        /// </summary>
        public MeshSimplifier()
        {
            SubMeshCount = 0;
            Verbose = false;
            PreserveSurfaceCurvature = false;
            PreserveUVFoldoverEdges = false;
            PreserveUVSeamEdges = false;
            PreserveBorderEdges = false;
            _triangles = new ResizableArray<Triangle>(0);
            _vertices = new ResizableArray<Vertex>(0);
            _refs = new ResizableArray<Ref>(0);
        }

        /// <summary>
        /// Creates a new mesh simplifier.
        /// </summary>
        /// <param name="mesh">The original mesh to simplify.</param>
        public MeshSimplifier(Mesh mesh)
            : this()
        {
            if (mesh != null)
            {
                Initialize(mesh);
            }
        }
        #endregion

        #region Private Methods
        #region Initialize Vertex Attribute
        private void InitializeVertexAttribute<T>(T[] attributeValues, ref ResizableArray<T> attributeArray, string attributeName)
        {
            if (attributeValues != null && attributeValues.Length == _vertices.Length)
            {
                if (attributeArray == null)
                {
                    attributeArray = new ResizableArray<T>(attributeValues.Length, attributeValues.Length);
                }
                else
                {
                    attributeArray.Resize(attributeValues.Length);
                }

                T[] arrayData = attributeArray.Data;
                Array.Copy(attributeValues, 0, arrayData, 0, attributeValues.Length);
            }
            else
            {
                if (attributeValues != null && attributeValues.Length > 0)
                {
                    Debug.LogErrorFormat("Failed to set vertex attribute '{0}' with {1} length of array, when {2} was needed.", attributeName, attributeValues.Length, _vertices.Length);
                }
                attributeArray = null;
            }
        }
        #endregion

        #region Calculate Error
        [MethodImpl(256)] // MethodImplOptions.AggressiveInlining
        private static double VertexError(ref SymmetricMatrix q, double x, double y, double z)
        {
            return q.m0 * x * x + 2 * q.m1 * x * y + 2 * q.m2 * x * z + 2 * q.m3 * x + q.m4 * y * y
                + 2 * q.m5 * y * z + 2 * q.m6 * y + q.m7 * z * z + 2 * q.m8 * z + q.m9;
        }

        [MethodImpl(256)] // MethodImplOptions.AggressiveInlining
        private double CurvatureError(ref Vertex vert0, ref Vertex vert1)
        {
            double diffVector = (vert0.p - vert1.p).Magnitude;

            HashSet<Triangle> trianglesWithViOrVjOrBoth = _triangleHashSet1;
            trianglesWithViOrVjOrBoth.Clear();
            GetTrianglesContainingVertex(ref vert0, trianglesWithViOrVjOrBoth);
            GetTrianglesContainingVertex(ref vert1, trianglesWithViOrVjOrBoth);

            HashSet<Triangle> trianglesWithViAndVjBoth = _triangleHashSet2;
            trianglesWithViAndVjBoth.Clear();
            GetTrianglesContainingBothVertices(ref vert0, ref vert1, trianglesWithViAndVjBoth);

            double maxDotOuter = 0;
            foreach (Triangle triangleWithViOrVjOrBoth in trianglesWithViOrVjOrBoth)
            {
                double maxDotInner = 0;
                Vector3d normVecTriangleWithViOrVjOrBoth = triangleWithViOrVjOrBoth.n;

                foreach (Triangle triangleWithViAndVjBoth in trianglesWithViAndVjBoth)
                {
                    Vector3d normVecTriangleWithViAndVjBoth = triangleWithViAndVjBoth.n;
                    double dot = Vector3d.Dot(ref normVecTriangleWithViOrVjOrBoth, ref normVecTriangleWithViAndVjBoth);
                    
                    if (dot > maxDotInner)
                        maxDotInner = dot;
                }

                if (maxDotInner > maxDotOuter)
                    maxDotOuter = maxDotInner;
            }

            return diffVector * maxDotOuter;
        }

        private double CalculateError(ref Vertex vert0, ref Vertex vert1, out Vector3d result)
        {
            // compute interpolated vertex
            SymmetricMatrix q = (vert0.q + vert1.q);
            bool borderEdge = (vert0.borderEdge && vert1.borderEdge);
            double error = 0.0;
            double det = q.Determinant1();
            if (det != 0.0 && !borderEdge)
            {
                // q_delta is invertible
                result = new Vector3d(
                    -1.0 / det * q.Determinant2(),  // vx = A41/det(q_delta)
                    1.0 / det * q.Determinant3(),   // vy = A42/det(q_delta)
                    -1.0 / det * q.Determinant4()); // vz = A43/det(q_delta)

                double curvatureError = 0;
                if (PreserveSurfaceCurvature)
                {
                    curvatureError = CurvatureError(ref vert0, ref vert1);
                }

                error = VertexError(ref q, result.x, result.y, result.z) + curvatureError;
            }
            else
            {
                // det = 0 -> try to find best result
                Vector3d p1 = vert0.p;
                Vector3d p2 = vert1.p;
                Vector3d p3 = (p1 + p2) * 0.5f;
                double error1 = VertexError(ref q, p1.x, p1.y, p1.z);
                double error2 = VertexError(ref q, p2.x, p2.y, p2.z);
                double error3 = VertexError(ref q, p3.x, p3.y, p3.z);

                if (error1 < error2)
                {
                    if (error1 < error3)
                    {
                        error = error1;
                        result = p1;
                    }
                    else
                    {
                        error = error3;
                        result = p3;
                    }
                }
                else if (error2 < error3)
                {
                    error = error2;
                    result = p2;
                }
                else
                {
                    error = error3;
                    result = p3;
                }
            }
            return error;
        }
        #endregion

        #region Calculate Barycentric Coordinates
        private static void CalculateBarycentricCoords(ref Vector3d point, ref Vector3d a, ref Vector3d b, ref Vector3d c, out Vector3 result)
        {
            Vector3 v0 = (Vector3)(b - a), v1 = (Vector3)(c - a), v2 = (Vector3)(point - a);
            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);
            float denom = d00 * d11 - d01 * d01;
            float v = (d11 * d20 - d01 * d21) / denom;
            float w = (d00 * d21 - d01 * d20) / denom;
            float u = 1f - v - w;
            result = new Vector3(u, v, w);
        }
        #endregion

        #region Normalize Tangent
        [MethodImpl(256)] // MethodImplOptions.AggressiveInlining
        private static Vector4 NormalizeTangent(Vector4 tangent)
        {
            Vector3 tangentVec = new Vector3(tangent.x, tangent.y, tangent.z);
            tangentVec.Normalize();
            return new Vector4(tangentVec.x, tangentVec.y, tangentVec.z, tangent.w);
        }
        #endregion

        #region Flipped
        /// <summary>
        /// Check if a triangle flips when this edge is removed
        /// </summary>
        private bool Flipped(ref Vector3d p, int i0, int i1, ref Vertex v0, bool[] deleted)
        {
            int tcount = v0.tcount;
            Ref[] refs = _refs.Data;
            Triangle[] triangles = _triangles.Data;
            Vertex[] vertices = _vertices.Data;
            for (int k = 0; k < tcount; k++)
            {
                Ref r = refs[v0.tstart + k];
                if (triangles[r.tid].deleted)
                    continue;

                int s = r.tvertex;
                int id1 = triangles[r.tid][(s + 1) % 3];
                int id2 = triangles[r.tid][(s + 2) % 3];
                if (id1 == i1 || id2 == i1)
                {
                    deleted[k] = true;
                    continue;
                }

                Vector3d d1 = vertices[id1].p - p;
                d1.Normalize();
                Vector3d d2 = vertices[id2].p - p;
                d2.Normalize();
                double dot = Vector3d.Dot(ref d1, ref d2);
                if (System.Math.Abs(dot) > 0.999)
                    return true;

                Vector3d n;
                Vector3d.Cross(ref d1, ref d2, out n);
                n.Normalize();
                deleted[k] = false;
                dot = Vector3d.Dot(ref n, ref triangles[r.tid].n);
                if (dot < 0.2)
                    return true;
            }

            return false;
        }
        #endregion

        #region Update Triangles
        /// <summary>
        /// Update triangle connections and edge error after a edge is collapsed.
        /// </summary>
        private void UpdateTriangles(int i0, int ia0, ref Vertex v, ResizableArray<bool> deleted, ref int deletedTriangles)
        {
            Vector3d p;
            int tcount = v.tcount;
            Triangle[] triangles = _triangles.Data;
            Vertex[] vertices = _vertices.Data;
            for (int k = 0; k < tcount; k++)
            {
                Ref r = _refs[v.tstart + k];
                int tid = r.tid;
                Triangle t = triangles[tid];
                if (t.deleted)
                    continue;

                if (deleted[k])
                {
                    triangles[tid].deleted = true;
                    ++deletedTriangles;
                    continue;
                }

                t[r.tvertex] = i0;
                if (ia0 != -1)
                {
                    t.SetAttributeIndex(r.tvertex, ia0);
                }

                t.dirty = true;
                t.err0 = CalculateError(ref vertices[t.v0], ref vertices[t.v1], out p);
                t.err1 = CalculateError(ref vertices[t.v1], ref vertices[t.v2], out p);
                t.err2 = CalculateError(ref vertices[t.v2], ref vertices[t.v0], out p);
                t.err3 = MathHelper.Min(t.err0, t.err1, t.err2);
                triangles[tid] = t;
                _refs.Add(r);
            }
        }
        #endregion

        #region Interpolate Vertex Attributes
        private void InterpolateVertexAttributes(int dst, int i0, int i1, int i2, ref Vector3 barycentricCoord)
        {
            if (_vertNormals != null)
            {
                _vertNormals[dst] = Vector3.Normalize((_vertNormals[i0] * barycentricCoord.x) + (_vertNormals[i1] * barycentricCoord.y) + (_vertNormals[i2] * barycentricCoord.z));
            }
            if (_vertTangents != null)
            {
                _vertTangents[dst] = NormalizeTangent((_vertTangents[i0] * barycentricCoord.x) + (_vertTangents[i1] * barycentricCoord.y) + (_vertTangents[i2] * barycentricCoord.z));
            }
            if (_vertUV2D != null)
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    ResizableArray<Vector2> vertUV = _vertUV2D[i];
                    if (vertUV != null)
                    {
                        vertUV[dst] = (vertUV[i0] * barycentricCoord.x) + (vertUV[i1] * barycentricCoord.y) + (vertUV[i2] * barycentricCoord.z);
                    }
                }
            }
            if (_vertUV3D != null)
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    ResizableArray<Vector3> vertUV = _vertUV3D[i];
                    if (vertUV != null)
                    {
                        vertUV[dst] = (vertUV[i0] * barycentricCoord.x) + (vertUV[i1] * barycentricCoord.y) + (vertUV[i2] * barycentricCoord.z);
                    }
                }
            }
            if (_vertUV4D != null)
            {
                for (int i = 0; i < UVChannelCount; i++)
                {
                    ResizableArray<Vector4> vertUV = _vertUV4D[i];
                    if (vertUV != null)
                    {
                        vertUV[dst] = (vertUV[i0] * barycentricCoord.x) + (vertUV[i1] * barycentricCoord.y) + (vertUV[i2] * barycentricCoord.z);
                    }
                }
            }
            if (_vertColors != null)
            {
                _vertColors[dst] = (_vertColors[i0] * barycentricCoord.x) + (_vertColors[i1] * barycentricCoord.y) + (_vertColors[i2] * barycentricCoord.z);
            }
        }
        #endregion

        #region Are UVs The Same
        private bool AreUVsTheSame(int channel, int indexA, int indexB)
        {
            if (_vertUV2D != null)
            {
                ResizableArray<Vector2> vertUV = _vertUV2D[channel];
                if (vertUV != null)
                {
                    Vector2 uvA = vertUV[indexA];
                    Vector2 uvB = vertUV[indexB];
                    return uvA == uvB;
                }
            }

            if (_vertUV3D != null)
            {
                ResizableArray<Vector3> vertUV = _vertUV3D[channel];
                if (vertUV != null)
                {
                    Vector3 uvA = vertUV[indexA];
                    Vector3 uvB = vertUV[indexB];
                    return uvA == uvB;
                }
            }

            if (_vertUV4D != null)
            {
                ResizableArray<Vector4> vertUV = _vertUV4D[channel];
                if (vertUV != null)
                {
                    Vector4 uvA = vertUV[indexA];
                    Vector4 uvB = vertUV[indexB];
                    return uvA == uvB;
                }
            }

            return false;
        }
        #endregion

        #region Remove Vertex Pass
        /// <summary>
        /// Remove vertices and mark deleted triangles
        /// </summary>
        private void RemoveVertexPass(int startTrisCount, int targetTrisCount, double threshold, ResizableArray<bool> deleted0, ResizableArray<bool> deleted1, ref int deletedTris)
        {
            Triangle[] triangles = _triangles.Data;
            int triangleCount = _triangles.Length;
            Vertex[] vertices = _vertices.Data;

            Vector3d p;
            Vector3 barycentricCoord;
            for (int tid = 0; tid < triangleCount; tid++)
            {
                if (triangles[tid].dirty || triangles[tid].deleted || triangles[tid].err3 > threshold)
                    continue;

                triangles[tid].GetErrors(_errArr);
                triangles[tid].GetAttributeIndices(_attributeIndexArr);
                for (int edgeIndex = 0; edgeIndex < TriangleEdgeCount; edgeIndex++)
                {
                    if (_errArr[edgeIndex] > threshold)
                        continue;

                    int nextEdgeIndex = ((edgeIndex + 1) % TriangleEdgeCount);
                    int i0 = triangles[tid][edgeIndex];
                    int i1 = triangles[tid][nextEdgeIndex];

                    // Border check
                    if (vertices[i0].borderEdge != vertices[i1].borderEdge)
                        continue;
                    // Seam check
                    if (vertices[i0].uvSeamEdge != vertices[i1].uvSeamEdge)
                        continue;
                    // Foldover check
                    if (vertices[i0].uvFoldoverEdge != vertices[i1].uvFoldoverEdge)
                        continue;
                    // If borders should be preserved
                    if (PreserveBorderEdges && vertices[i0].borderEdge)
                        continue;
                    // If seams should be preserved
                    if (PreserveUVSeamEdges && vertices[i0].uvSeamEdge)
                        continue;
                    // If foldovers should be preserved
                    if (PreserveUVFoldoverEdges && vertices[i0].uvFoldoverEdge)
                        continue;

                    // Compute vertex to collapse to
                    CalculateError(ref vertices[i0], ref vertices[i1], out p);
                    deleted0.Resize(vertices[i0].tcount); // normals temporarily
                    deleted1.Resize(vertices[i1].tcount); // normals temporarily

                    // Don't remove if flipped
                    if (Flipped(ref p, i0, i1, ref vertices[i0], deleted0.Data))
                        continue;
                    if (Flipped(ref p, i1, i0, ref vertices[i1], deleted1.Data))
                        continue;

                    // Calculate the barycentric coordinates within the triangle
                    int nextNextEdgeIndex = ((edgeIndex + 2) % 3);
                    int i2 = triangles[tid][nextNextEdgeIndex];
                    CalculateBarycentricCoords(ref p, ref vertices[i0].p, ref vertices[i1].p, ref vertices[i2].p, out barycentricCoord);

                    // Not flipped, so remove edge
                    vertices[i0].p = p;
                    vertices[i0].q += vertices[i1].q;

                    // Interpolate the vertex attributes
                    int ia0 = _attributeIndexArr[edgeIndex];
                    int ia1 = _attributeIndexArr[nextEdgeIndex];
                    int ia2 = _attributeIndexArr[nextNextEdgeIndex];
                    InterpolateVertexAttributes(ia0, ia0, ia1, ia2, ref barycentricCoord);

                    if (vertices[i0].uvSeamEdge)
                    {
                        ia0 = -1;
                    }

                    int tstart = _refs.Length;
                    UpdateTriangles(i0, ia0, ref vertices[i0], deleted0, ref deletedTris);
                    UpdateTriangles(i0, ia0, ref vertices[i1], deleted1, ref deletedTris);

                    int tcount = _refs.Length - tstart;
                    if (tcount <= vertices[i0].tcount)
                    {
                        // save ram
                        if (tcount > 0)
                        {
                            Ref[] refsArr = _refs.Data;
                            Array.Copy(refsArr, tstart, refsArr, vertices[i0].tstart, tcount);
                        }
                    }
                    else
                    {
                        // append
                        vertices[i0].tstart = tstart;
                    }

                    vertices[i0].tcount = tcount;
                    break;
                }

                // Check if we are already done
                if ((startTrisCount - deletedTris) <= targetTrisCount)
                    break;
            }
        }
        #endregion

        #region Update Mesh
        /// <summary>
        /// Compact triangles, compute edge error and build reference list.
        /// </summary>
        /// <param name="iteration">The iteration index.</param>
        private void UpdateMesh(int iteration)
        {
            Triangle[] triangles = _triangles.Data;
            Vertex[] vertices = _vertices.Data;

            int triangleCount = _triangles.Length;
            int vertexCount = _vertices.Length;
            if (iteration > 0) // compact triangles
            {
                int dst = 0;
                for (int i = 0; i < triangleCount; i++)
                {
                    if (!triangles[i].deleted)
                    {
                        if (dst != i)
                        {
                            triangles[dst] = triangles[i];
                            triangles[dst].index = dst;
                        }
                        dst++;
                    }
                }
                _triangles.Resize(dst);
                triangles = _triangles.Data;
                triangleCount = dst;
            }

            UpdateReferences();

            // Identify boundary : vertices[].border=0,1
            if (iteration == 0)
            {
                Ref[] refs = _refs.Data;

                List<int> vcount = new List<int>(8);
                List<int> vids = new List<int>(8);
                int vsize = 0;
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i].borderEdge = false;
                    vertices[i].uvSeamEdge = false;
                    vertices[i].uvFoldoverEdge = false;
                }

                int ofs;
                int id;
                int borderVertexCount = 0;
                double borderMinX = double.MaxValue;
                double borderMaxX = double.MinValue;
                for (int i = 0; i < vertexCount; i++)
                {
                    int tstart = vertices[i].tstart;
                    int tcount = vertices[i].tcount;
                    vcount.Clear();
                    vids.Clear();
                    vsize = 0;

                    for (int j = 0; j < tcount; j++)
                    {
                        int tid = refs[tstart + j].tid;
                        for (int k = 0; k < TriangleVertexCount; k++)
                        {
                            ofs = 0;
                            id = triangles[tid][k];
                            while (ofs < vsize)
                            {
                                if (vids[ofs] == id)
                                    break;

                                ++ofs;
                            }

                            if (ofs == vsize)
                            {
                                vcount.Add(1);
                                vids.Add(id);
                                ++vsize;
                            }
                            else
                            {
                                ++vcount[ofs];
                            }
                        }
                    }

                    for (int j = 0; j < vsize; j++)
                    {
                        if (vcount[j] == 1)
                        {
                            id = vids[j];
                            vertices[id].borderEdge = true;
                            ++borderVertexCount;

                            if (_enableSmartLink)
                            {
                                if (vertices[id].p.x < borderMinX)
                                {
                                    borderMinX = vertices[id].p.x;
                                }
                                if (vertices[id].p.x > borderMaxX)
                                {
                                    borderMaxX = vertices[id].p.x;
                                }
                            }
                        }
                    }
                }

                if (_enableSmartLink)
                {
                    // First find all border vertices
                    BorderVertex[] borderVertices = new BorderVertex[borderVertexCount];
                    int borderIndexCount = 0;
                    double borderAreaWidth = borderMaxX - borderMinX;
                    for (int i = 0; i < vertexCount; i++)
                    {
                        if (vertices[i].borderEdge)
                        {
                            int vertexHash = (int)(((((vertices[i].p.x - borderMinX) / borderAreaWidth) * 2.0) - 1.0) * int.MaxValue);
                            borderVertices[borderIndexCount] = new BorderVertex(i, vertexHash);
                            ++borderIndexCount;
                        }
                    }

                    // Sort the border vertices by hash
                    Array.Sort(borderVertices, 0, borderIndexCount, BorderVertexComparer.Instance);

                    // Calculate the maximum hash distance based on the maximum vertex link distance
                    double vertexLinkDistance = System.Math.Sqrt(_vertexLinkDistanceSqr);
                    int hashMaxDistance = System.Math.Max((int)((vertexLinkDistance / borderAreaWidth) * int.MaxValue), 1);

                    // Then find identical border vertices and bind them together as one
                    for (int i = 0; i < borderIndexCount; i++)
                    {
                        int myIndex = borderVertices[i].index;
                        if (myIndex == -1)
                            continue;

                        Vector3d myPoint = vertices[myIndex].p;
                        for (int j = i + 1; j < borderIndexCount; j++)
                        {
                            int otherIndex = borderVertices[j].index;
                            if (otherIndex == -1)
                                continue;
                            if ((borderVertices[j].hash - borderVertices[i].hash) > hashMaxDistance) // There is no point to continue beyond this point
                                break;

                            Vector3d otherPoint = vertices[otherIndex].p;
                            double sqrX = ((myPoint.x - otherPoint.x) * (myPoint.x - otherPoint.x));
                            double sqrY = ((myPoint.y - otherPoint.y) * (myPoint.y - otherPoint.y));
                            double sqrZ = ((myPoint.z - otherPoint.z) * (myPoint.z - otherPoint.z));
                            double sqrMagnitude = sqrX + sqrY + sqrZ;

                            if (sqrMagnitude <= _vertexLinkDistanceSqr)
                            {
                                borderVertices[j].index = -1; // NOTE: This makes sure that the "other" vertex is not processed again
                                vertices[myIndex].borderEdge = false;
                                vertices[otherIndex].borderEdge = false;

                                if (AreUVsTheSame(0, myIndex, otherIndex))
                                {
                                    vertices[myIndex].uvFoldoverEdge = true;
                                    vertices[otherIndex].uvFoldoverEdge = true;
                                }
                                else
                                {
                                    vertices[myIndex].uvSeamEdge = true;
                                    vertices[otherIndex].uvSeamEdge = true;
                                }

                                int otherTriangleCount = vertices[otherIndex].tcount;
                                int otherTriangleStart = vertices[otherIndex].tstart;
                                for (int k = 0; k < otherTriangleCount; k++)
                                {
                                    Ref r = refs[otherTriangleStart + k];
                                    triangles[r.tid][r.tvertex] = myIndex;
                                }
                            }
                        }
                    }

                    // Update the references again
                    UpdateReferences();
                }

                // Init Quadrics by Plane & Edge Errors
                //
                // required at the beginning ( iteration == 0 )
                // recomputing during the simplification is not required,
                // but mostly improves the result for closed meshes
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i].q = new SymmetricMatrix();
                }

                int v0, v1, v2;
                Vector3d n, p0, p1, p2, p10, p20, dummy;
                SymmetricMatrix sm;
                for (int i = 0; i < triangleCount; i++)
                {
                    v0 = triangles[i].v0;
                    v1 = triangles[i].v1;
                    v2 = triangles[i].v2;

                    p0 = vertices[v0].p;
                    p1 = vertices[v1].p;
                    p2 = vertices[v2].p;
                    p10 = p1 - p0;
                    p20 = p2 - p0;
                    Vector3d.Cross(ref p10, ref p20, out n);
                    n.Normalize();
                    triangles[i].n = n;

                    sm = new SymmetricMatrix(n.x, n.y, n.z, -Vector3d.Dot(ref n, ref p0));
                    vertices[v0].q += sm;
                    vertices[v1].q += sm;
                    vertices[v2].q += sm;
                }

                for (int i = 0; i < triangleCount; i++)
                {
                    // Calc Edge Error
                    Triangle triangle = triangles[i];
                    triangles[i].err0 = CalculateError(ref vertices[triangle.v0], ref vertices[triangle.v1], out dummy);
                    triangles[i].err1 = CalculateError(ref vertices[triangle.v1], ref vertices[triangle.v2], out dummy);
                    triangles[i].err2 = CalculateError(ref vertices[triangle.v2], ref vertices[triangle.v0], out dummy);
                    triangles[i].err3 = MathHelper.Min(triangles[i].err0, triangles[i].err1, triangles[i].err2);
                }
            }
        }
        #endregion

        #region Update References
        private void UpdateReferences()
        {
            int triangleCount = _triangles.Length;
            int vertexCount = _vertices.Length;
            Triangle[] triangles = _triangles.Data;
            Vertex[] vertices = _vertices.Data;

            // Init Reference ID list
            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i].tstart = 0;
                vertices[i].tcount = 0;
            }

            for (int i = 0; i < triangleCount; i++)
            {
                ++vertices[triangles[i].v0].tcount;
                ++vertices[triangles[i].v1].tcount;
                ++vertices[triangles[i].v2].tcount;
            }

            int tstart = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i].tstart = tstart;
                tstart += vertices[i].tcount;
                vertices[i].tcount = 0;
            }

            // Write References
            _refs.Resize(tstart);
            Ref[] refs = _refs.Data;
            for (int i = 0; i < triangleCount; i++)
            {
                int v0 = triangles[i].v0;
                int v1 = triangles[i].v1;
                int v2 = triangles[i].v2;
                int start0 = vertices[v0].tstart;
                int count0 = vertices[v0].tcount;
                int start1 = vertices[v1].tstart;
                int count1 = vertices[v1].tcount;
                int start2 = vertices[v2].tstart;
                int count2 = vertices[v2].tcount;

                refs[start0 + count0].Set(i, 0);
                refs[start1 + count1].Set(i, 1);
                refs[start2 + count2].Set(i, 2);

                ++vertices[v0].tcount;
                ++vertices[v1].tcount;
                ++vertices[v2].tcount;
            }
        }
        #endregion

        #region Compact Mesh
        /// <summary>
        /// Finally compact mesh before exiting.
        /// </summary>
        private void CompactMesh()
        {
            int dst = 0;
            Vertex[] vertices = _vertices.Data;
            int vertexCount = _vertices.Length;
            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i].tcount = 0;
            }

            Vector3[] vertNormals = (_vertNormals != null ? _vertNormals.Data : null);
            Vector4[] vertTangents = (_vertTangents != null ? _vertTangents.Data : null);
            Vector2[][] vertUV2D = (_vertUV2D != null ? _vertUV2D.Data : null);
            Vector3[][] vertUV3D = (_vertUV3D != null ? _vertUV3D.Data : null);
            Vector4[][] vertUV4D = (_vertUV4D != null ? _vertUV4D.Data : null);
            Color[] vertColors = (_vertColors != null ? _vertColors.Data : null);
            BoneWeight[] vertBoneWeights = (_vertBoneWeights != null ? _vertBoneWeights.Data : null);

            int lastSubMeshIndex = -1;
            _subMeshOffsets = new int[SubMeshCount];

            Triangle[] triangles = _triangles.Data;
            int triangleCount = _triangles.Length;
            for (int i = 0; i < triangleCount; i++)
            {
                Triangle triangle = triangles[i];
                if (!triangle.deleted)
                {
                    if (triangle.va0 != triangle.v0)
                    {
                        int iDest = triangle.va0;
                        int iSrc = triangle.v0;
                        vertices[iDest].p = vertices[iSrc].p;
                        if (vertBoneWeights != null)
                        {
                            vertBoneWeights[iDest] = vertBoneWeights[iSrc];
                        }
                        triangle.v0 = triangle.va0;
                    }
                    if (triangle.va1 != triangle.v1)
                    {
                        int iDest = triangle.va1;
                        int iSrc = triangle.v1;
                        vertices[iDest].p = vertices[iSrc].p;
                        if (vertBoneWeights != null)
                        {
                            vertBoneWeights[iDest] = vertBoneWeights[iSrc];
                        }
                        triangle.v1 = triangle.va1;
                    }
                    if (triangle.va2 != triangle.v2)
                    {
                        int iDest = triangle.va2;
                        int iSrc = triangle.v2;
                        vertices[iDest].p = vertices[iSrc].p;
                        if (vertBoneWeights != null)
                        {
                            vertBoneWeights[iDest] = vertBoneWeights[iSrc];
                        }
                        triangle.v2 = triangle.va2;
                    }
                    int newTriangleIndex = dst++;
                    triangles[newTriangleIndex] = triangle;
                    triangles[newTriangleIndex].index = newTriangleIndex;

                    vertices[triangle.v0].tcount = 1;
                    vertices[triangle.v1].tcount = 1;
                    vertices[triangle.v2].tcount = 1;

                    if (triangle.subMeshIndex > lastSubMeshIndex)
                    {
                        for (int j = lastSubMeshIndex + 1; j < triangle.subMeshIndex; j++)
                        {
                            _subMeshOffsets[j] = newTriangleIndex;
                        }
                        _subMeshOffsets[triangle.subMeshIndex] = newTriangleIndex;
                        lastSubMeshIndex = triangle.subMeshIndex;
                    }
                }
            }

            triangleCount = dst;
            for (int i = lastSubMeshIndex + 1; i < SubMeshCount; i++)
            {
                _subMeshOffsets[i] = triangleCount;
            }

            _triangles.Resize(triangleCount);
            triangles = _triangles.Data;

            dst = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                Vertex vert = vertices[i];
                if (vert.tcount > 0)
                {
                    vertices[i].tstart = dst;

                    if (dst != i)
                    {
                        vertices[dst].index = dst;
                        vertices[dst].p = vert.p;
                        if (vertNormals != null) vertNormals[dst] = vertNormals[i];
                        if (vertTangents != null) vertTangents[dst] = vertTangents[i];
                        if (vertUV2D != null)
                        {
                            for (int j = 0; j < UVChannelCount; j++)
                            {
                                Vector2[] vertUV = vertUV2D[j];
                                if (vertUV != null)
                                {
                                    vertUV[dst] = vertUV[i];
                                }
                            }
                        }
                        if (vertUV3D != null)
                        {
                            for (int j = 0; j < UVChannelCount; j++)
                            {
                                Vector3[] vertUV = vertUV3D[j];
                                if (vertUV != null)
                                {
                                    vertUV[dst] = vertUV[i];
                                }
                            }
                        }
                        if (vertUV4D != null)
                        {
                            for (int j = 0; j < UVChannelCount; j++)
                            {
                                Vector4[] vertUV = vertUV4D[j];
                                if (vertUV != null)
                                {
                                    vertUV[dst] = vertUV[i];
                                }
                            }
                        }
                        if (vertColors != null) vertColors[dst] = vertColors[i];
                        if (vertBoneWeights != null) vertBoneWeights[dst] = vertBoneWeights[i];

                    }
                    ++dst;
                }
            }

            for (int i = 0; i < triangleCount; i++)
            {
                Triangle triangle = triangles[i];
                triangle.v0 = vertices[triangle.v0].tstart;
                triangle.v1 = vertices[triangle.v1].tstart;
                triangle.v2 = vertices[triangle.v2].tstart;
                triangles[i] = triangle;
            }

            vertexCount = dst;
            _vertices.Resize(vertexCount);
            if (vertNormals != null) _vertNormals.Resize(vertexCount, true);
            if (vertTangents != null) _vertTangents.Resize(vertexCount, true);
            if (vertUV2D != null) _vertUV2D.Resize(vertexCount, true);
            if (vertUV3D != null) _vertUV3D.Resize(vertexCount, true);
            if (vertUV4D != null) _vertUV4D.Resize(vertexCount, true);
            if (vertColors != null) _vertColors.Resize(vertexCount, true);
            if (vertBoneWeights != null) _vertBoneWeights.Resize(vertexCount, true);

        }
        #endregion

        #region Calculate Sub Mesh Offsets
        private void CalculateSubMeshOffsets()
        {
            int lastSubMeshIndex = -1;
            _subMeshOffsets = new int[SubMeshCount];

            Triangle[] triangles = _triangles.Data;
            int triangleCount = _triangles.Length;
            for (int i = 0; i < triangleCount; i++)
            {
                Triangle triangle = triangles[i];
                if (triangle.subMeshIndex > lastSubMeshIndex)
                {
                    for (int j = lastSubMeshIndex + 1; j < triangle.subMeshIndex; j++)
                    {
                        _subMeshOffsets[j] = i;
                    }
                    _subMeshOffsets[triangle.subMeshIndex] = i;
                    lastSubMeshIndex = triangle.subMeshIndex;
                }
            }

            for (int i = lastSubMeshIndex + 1; i < SubMeshCount; i++)
            {
                _subMeshOffsets[i] = triangleCount;
            }
        }
        #endregion

        #region Triangle helper functions
        [MethodImpl(256)] // MethodImplOptions.AggressiveInlining
        private void GetTrianglesContainingVertex(ref Vertex vert, HashSet<Triangle> tris)
        {
            int trianglesCount = vert.tcount;
            int startIndex = vert.tstart;

            for (int a = startIndex; a < startIndex + trianglesCount; a++)
            {
                tris.Add(_triangles[_refs[a].tid]);
            }
        }

        [MethodImpl(256)] // MethodImplOptions.AggressiveInlining
        private void GetTrianglesContainingBothVertices(ref Vertex vert0, ref Vertex vert1, HashSet<Triangle> tris)
        {
            int triangleCount = vert0.tcount;
            int startIndex = vert0.tstart;

            for (int refIndex = startIndex; refIndex < (startIndex + triangleCount); refIndex++)
            {
                int tid = _refs[refIndex].tid;
                Triangle tri = _triangles[tid];

                if (_vertices[tri.v0].index == vert1.index ||
                    _vertices[tri.v1].index == vert1.index ||
                    _vertices[tri.v2].index == vert1.index)
                {
                    tris.Add(tri);
                }
            }
        }
        #endregion Triangle helper functions
        #endregion

        #region Public Methods
        #region Sub-Meshes
        /// <summary>
        /// Returns the triangle indices for all sub-meshes.
        /// </summary>
        /// <returns>The triangle indices for all sub-meshes.</returns>
        public int[][] GetAllSubMeshTriangles()
        {
            int[][] indices = new int[SubMeshCount][];
            for (int subMeshIndex = 0; subMeshIndex < SubMeshCount; subMeshIndex++)
            {
                indices[subMeshIndex] = GetSubMeshTriangles(subMeshIndex);
            }
            return indices;
        }

        /// <summary>
        /// Returns the triangle indices for a specific sub-mesh.
        /// </summary>
        /// <param name="subMeshIndex">The sub-mesh index.</param>
        /// <returns>The triangle indices.</returns>
        public int[] GetSubMeshTriangles(int subMeshIndex)
        {
            if (subMeshIndex < 0)
                throw new ArgumentOutOfRangeException("subMeshIndex", "The sub-mesh index is negative.");

            // First get the sub-mesh offsets
            if (_subMeshOffsets == null)
            {
                CalculateSubMeshOffsets();
            }

            if (subMeshIndex >= _subMeshOffsets.Length)
                throw new ArgumentOutOfRangeException("subMeshIndex", "The sub-mesh index is greater than or equals to the sub mesh count.");
            if (_subMeshOffsets.Length != SubMeshCount)
                throw new InvalidOperationException("The sub-mesh triangle offsets array is not the same size as the count of sub-meshes. This should not be possible to happen.");

            Triangle[] triangles = _triangles.Data;
            int triangleCount = _triangles.Length;

            int startOffset = _subMeshOffsets[subMeshIndex];
            if (startOffset >= triangleCount)
                return new int[0];

            int endOffset = ((subMeshIndex + 1) < SubMeshCount ? _subMeshOffsets[subMeshIndex + 1] : triangleCount);
            int subMeshTriangleCount = endOffset - startOffset;
            if (subMeshTriangleCount < 0) subMeshTriangleCount = 0;
            int[] subMeshIndices = new int[subMeshTriangleCount * 3];

            Debug.AssertFormat(startOffset >= 0, "The start sub mesh offset at index {0} was below zero ({1}).", subMeshIndex, startOffset);
            Debug.AssertFormat(endOffset >= 0, "The end sub mesh offset at index {0} was below zero ({1}).", subMeshIndex + 1, endOffset);
            Debug.AssertFormat(startOffset < triangleCount, "The start sub mesh offset at index {0} was higher or equal to the triangle count ({1} >= {2}).", subMeshIndex, startOffset, triangleCount);
            Debug.AssertFormat(endOffset <= triangleCount, "The end sub mesh offset at index {0} was higher than the triangle count ({1} > {2}).", subMeshIndex + 1, endOffset, triangleCount);

            for (int triangleIndex = startOffset; triangleIndex < endOffset; triangleIndex++)
            {
                Triangle triangle = triangles[triangleIndex];
                int offset = (triangleIndex - startOffset) * 3;
                subMeshIndices[offset] = triangle.v0;
                subMeshIndices[offset + 1] = triangle.v1;
                subMeshIndices[offset + 2] = triangle.v2;
            }

            return subMeshIndices;
        }

        /// <summary>
        /// Clears out all sub-meshes.
        /// </summary>
        public void ClearSubMeshes()
        {
            SubMeshCount = 0;
            _subMeshOffsets = null;
            _triangles.Resize(0);
        }

        /// <summary>
        /// Adds several sub-meshes at once with their triangle indices for each sub-mesh.
        /// </summary>
        /// <param name="triangles">The triangle indices for each sub-mesh.</param>
        public void AddSubMeshTriangles(int[][] triangles)
        {
            if (triangles == null)
                throw new ArgumentNullException("triangles");

            int totalTriangleCount = 0;
            for (int i = 0; i < triangles.Length; i++)
            {
                if (triangles[i] == null)
                    throw new ArgumentException(string.Format("The index array at index {0} is null.", i));
                if ((triangles[i].Length % TriangleVertexCount) != 0)
                    throw new ArgumentException(string.Format("The index array length at index {0} must be a multiple of 3 in order to represent triangles.", i), "triangles");

                totalTriangleCount += triangles[i].Length / TriangleVertexCount;
            }

            int triangleIndexStart = _triangles.Length;
            _triangles.Resize(_triangles.Length + totalTriangleCount);
            Triangle[] trisArr = _triangles.Data;

            for (int i = 0; i < triangles.Length; i++)
            {
                int subMeshIndex = SubMeshCount++;
                int[] subMeshTriangles = triangles[i];
                int subMeshTriangleCount = subMeshTriangles.Length / TriangleVertexCount;
                for (int j = 0; j < subMeshTriangleCount; j++)
                {
                    int offset = j * 3;
                    int v0 = subMeshTriangles[offset];
                    int v1 = subMeshTriangles[offset + 1];
                    int v2 = subMeshTriangles[offset + 2];
                    int triangleIndex = triangleIndexStart + j;
                    trisArr[triangleIndex] = new Triangle(triangleIndex, v0, v1, v2, subMeshIndex);
                }

                triangleIndexStart += subMeshTriangleCount;
            }
        }
        #endregion

        #region UV Sets
        #region Getting
        /// <summary>
        /// Returns the UVs (2D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <returns>The UVs.</returns>
        public Vector2[] GetUVs2D(int channel)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");

            if (_vertUV2D != null && _vertUV2D[channel] != null)
            {
                return _vertUV2D[channel].Data;
            }

            return null;
        }

        /// <summary>
        /// Returns the UVs (2D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void GetUVs(int channel, List<Vector2> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");
            if (uvs == null)
                throw new ArgumentNullException("uvs");

            uvs.Clear();
            if (_vertUV2D != null && _vertUV2D[channel] != null)
            {
                Vector2[] uvData = _vertUV2D[channel].Data;
                if (uvData != null)
                {
                    uvs.AddRange(uvData);
                }
            }
        }

        /// <summary>
        /// Returns the UVs (3D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void GetUVs(int channel, List<Vector3> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");
            if (uvs == null)
                throw new ArgumentNullException("uvs");

            uvs.Clear();
            if (_vertUV3D != null && _vertUV3D[channel] != null)
            {
                Vector3[] uvData = _vertUV3D[channel].Data;
                if (uvData != null)
                {
                    uvs.AddRange(uvData);
                }
            }
        }

        /// <summary>
        /// Returns the UVs (4D) from a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void GetUVs(int channel, List<Vector4> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");
            if (uvs == null)
                throw new ArgumentNullException("uvs");

            uvs.Clear();
            if (_vertUV4D != null && _vertUV4D[channel] != null)
            {
                Vector4[] uvData = _vertUV4D[channel].Data;
                if (uvData != null)
                {
                    uvs.AddRange(uvData);
                }
            }
        }
        #endregion

        #region Setting
        /// <summary>
        /// Sets the UVs (2D) for a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVs(int channel, Vector2[] uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");

            if (uvs != null && uvs.Length > 0)
            {
                if (_vertUV2D == null)
                    _vertUV2D = new UVChannels<Vector2>();

                int uvCount = uvs.Length;
                ResizableArray<Vector2> uvSet = _vertUV2D[channel];
                if (uvSet != null)
                {
                    uvSet.Resize(uvCount);
                }
                else
                {
                    uvSet = new ResizableArray<Vector2>(uvCount, uvCount);
                    _vertUV2D[channel] = uvSet;
                }

                Vector2[] uvData = uvSet.Data;
                uvs.CopyTo(uvData, 0);
            }
            else
            {
                if (_vertUV2D != null)
                {
                    _vertUV2D[channel] = null;
                }
            }

            if (_vertUV3D != null)
            {
                _vertUV3D[channel] = null;
            }
            if (_vertUV4D != null)
            {
                _vertUV4D[channel] = null;
            }
        }

        /// <summary>
        /// Sets the UVs (3D) for a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVs(int channel, Vector3[] uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");

            if (uvs != null && uvs.Length > 0)
            {
                if (_vertUV3D == null)
                    _vertUV3D = new UVChannels<Vector3>();

                int uvCount = uvs.Length;
                ResizableArray<Vector3> uvSet = _vertUV3D[channel];
                if (uvSet != null)
                {
                    uvSet.Resize(uvCount);
                }
                else
                {
                    uvSet = new ResizableArray<Vector3>(uvCount, uvCount);
                    _vertUV3D[channel] = uvSet;
                }

                Vector3[] uvData = uvSet.Data;
                uvs.CopyTo(uvData, 0);
            }
            else
            {
                if (_vertUV3D != null)
                {
                    _vertUV3D[channel] = null;
                }
            }

            if (_vertUV2D != null)
            {
                _vertUV2D[channel] = null;
            }
            if (_vertUV4D != null)
            {
                _vertUV4D[channel] = null;
            }
        }

        /// <summary>
        /// Sets the UVs (4D) for a specific channel.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVs(int channel, List<Vector4> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");

            if (uvs != null && uvs.Count > 0)
            {
                if (_vertUV4D == null)
                    _vertUV4D = new UVChannels<Vector4>();

                int uvCount = uvs.Count;
                ResizableArray<Vector4> uvSet = _vertUV4D[channel];
                if (uvSet != null)
                {
                    uvSet.Resize(uvCount);
                }
                else
                {
                    uvSet = new ResizableArray<Vector4>(uvCount, uvCount);
                    _vertUV4D[channel] = uvSet;
                }

                Vector4[] uvData = uvSet.Data;
                uvs.CopyTo(uvData, 0);
            }
            else
            {
                if (_vertUV4D != null)
                {
                    _vertUV4D[channel] = null;
                }
            }

            if (_vertUV2D != null)
            {
                _vertUV2D[channel] = null;
            }
            if (_vertUV3D != null)
            {
                _vertUV3D[channel] = null;
            }
        }

        /// <summary>
        /// Sets the UVs for a specific channel and automatically detects the used components.
        /// </summary>
        /// <param name="channel">The channel index.</param>
        /// <param name="uvs">The UVs.</param>
        public void SetUVsAuto(int channel, List<Vector4> uvs)
        {
            if (channel < 0 || channel >= UVChannelCount)
                throw new ArgumentOutOfRangeException("channel");

            if (uvs != null && uvs.Count > 0)
            {
                int usedComponents = MeshUtils.GetUsedUVComponents(uvs);
                if (usedComponents <= 2)
                {
                    Vector2[] uv2D = MeshUtils.ConvertUVsTo2D(uvs);
                    SetUVs(channel, uv2D);
                }
                else if (usedComponents == 3)
                {
                    Vector3[] uv3D = MeshUtils.ConvertUVsTo3D(uvs);
                    SetUVs(channel, uv3D);
                }
                else
                {
                    SetUVs(channel, uvs);
                }
            }
            else
            {
                if (_vertUV2D != null)
                {
                    _vertUV2D[channel] = null;
                }
                if (_vertUV3D != null)
                {
                    _vertUV3D[channel] = null;
                }
                if (_vertUV4D != null)
                {
                    _vertUV4D[channel] = null;
                }
            }
        }
        #endregion
        #endregion

        #region Initialize
        /// <summary>
        /// Initializes the algorithm with the original mesh.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        public void Initialize(Mesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            Vertices = mesh.vertices;
            Normals = mesh.normals;
            Tangents = mesh.tangents;

            Colors = mesh.colors;
            BoneWeights = mesh.boneWeights;
            _bindposes = mesh.bindposes;

            for (int channel = 0; channel < UVChannelCount; channel++)
            {
                List<Vector4> uvs = MeshUtils.GetMeshUVs(mesh, channel);
                SetUVsAuto(channel, uvs);
            }

            ClearSubMeshes();

            int subMeshCount = mesh.subMeshCount;
            int[][] subMeshTriangles = new int[subMeshCount][];
            for (int i = 0; i < subMeshCount; i++)
            {
                subMeshTriangles[i] = mesh.GetTriangles(i);
            }
            AddSubMeshTriangles(subMeshTriangles);
        }
        #endregion

        #region Simplify Mesh
        /// <summary>
        /// Simplifies the mesh to a desired quality.
        /// </summary>
        /// <param name="quality">The target quality (between 0 and 1).</param>
        public void SimplifyMesh(float quality)
        {
            quality = Mathf.Clamp01(quality);

            int deletedTris = 0;
            ResizableArray<bool> deleted0 = new ResizableArray<bool>(20);
            ResizableArray<bool> deleted1 = new ResizableArray<bool>(20);
            Triangle[] triangles = _triangles.Data;
            int triangleCount = _triangles.Length;
            int startTrisCount = triangleCount;
            Vertex[] vertices = _vertices.Data;
            int targetTrisCount = Mathf.RoundToInt(triangleCount * quality);

            for (int iteration = 0; iteration < _maxIterationCount; iteration++)
            {
                if ((startTrisCount - deletedTris) <= targetTrisCount)
                    break;

                // Update mesh once in a while
                if ((iteration % 5) == 0)
                {
                    UpdateMesh(iteration);
                    triangles = _triangles.Data;
                    triangleCount = _triangles.Length;
                    vertices = _vertices.Data;
                }

                // Clear dirty flag
                for (int i = 0; i < triangleCount; i++)
                {
                    triangles[i].dirty = false;
                }

                // All triangles with edges below the threshold will be removed
                //
                // The following numbers works well for most models.
                // If it does not, try to adjust the 3 parameters
                double threshold = 0.000000001 * System.Math.Pow(iteration + 3, _agressiveness);

                if (Verbose)
                {
                    Debug.LogFormat("iteration {0} - triangles {1} threshold {2}", iteration, (startTrisCount - deletedTris), threshold);
                }

                // Remove vertices & mark deleted triangles
                RemoveVertexPass(startTrisCount, targetTrisCount, threshold, deleted0, deleted1, ref deletedTris);
            }

            CompactMesh();

            if (Verbose)
            {
                Debug.LogFormat("Finished simplification with triangle count {0}", _triangles.Length);
            }
        }

        /// <summary>
        /// Simplifies the mesh without losing too much quality.
        /// </summary>
        public void SimplifyMeshLossless()
        {
            int deletedTris = 0;
            ResizableArray<bool> deleted0 = new ResizableArray<bool>(0);
            ResizableArray<bool> deleted1 = new ResizableArray<bool>(0);
            int triangleCount = _triangles.Length;
            int startTrisCount = triangleCount;

            for (int iteration = 0; iteration < 9999; iteration++)
            {
                // Update mesh constantly
                UpdateMesh(iteration);
                var triangles = _triangles.Data;
                triangleCount = _triangles.Length;

                // Clear dirty flag
                for (int i = 0; i < triangleCount; i++)
                {
                    triangles[i].dirty = false;
                }

                // All triangles with edges below the threshold will be removed
                //
                // The following numbers works well for most models.
                // If it does not, try to adjust the 3 parameters
                double threshold = DoubleEpsilon;

                if (Verbose)
                {
                    Debug.LogFormat("Lossless iteration {0} - triangles {1}", iteration, triangleCount);
                }

                // Remove vertices & mark deleted triangles
                RemoveVertexPass(startTrisCount, 0, threshold, deleted0, deleted1, ref deletedTris);

                if (deletedTris <= 0)
                    break;

                deletedTris = 0;
            }

            CompactMesh();

            if (Verbose)
            {
                Debug.LogFormat("Finished simplification with triangle count {0}", _triangles.Length);
            }
        }
        #endregion

        #region To Mesh
        /// <summary>
        /// Returns the resulting mesh.
        /// </summary>
        /// <returns>The resulting mesh.</returns>
        public Mesh ToMesh()
        {
            Vector3[] vertices = Vertices;
            Vector3[] normals = Normals;
            Vector4[] tangents = Tangents;
            Color[] colors = Colors;
            BoneWeight[] boneWeights = BoneWeights;
            int[][] indices = GetAllSubMeshTriangles();

            List<Vector2>[] uvs2D = null;
            List<Vector3>[] uvs3D = null;
            List<Vector4>[] uvs4D = null;
            if (_vertUV2D != null)
            {
                uvs2D = new List<Vector2>[UVChannelCount];
                for (int channel = 0; channel < UVChannelCount; channel++)
                {
                    if (_vertUV2D[channel] != null)
                    {
                        List<Vector2> uvs = new List<Vector2>(vertices.Length);
                        GetUVs(channel, uvs);
                        uvs2D[channel] = uvs;
                    }
                }
            }

            if (_vertUV3D != null)
            {
                uvs3D = new List<Vector3>[UVChannelCount];
                for (int channel = 0; channel < UVChannelCount; channel++)
                {
                    if (_vertUV3D[channel] != null)
                    {
                        List<Vector3> uvs = new List<Vector3>(vertices.Length);
                        GetUVs(channel, uvs);
                        uvs3D[channel] = uvs;
                    }
                }
            }

            if (_vertUV4D != null)
            {
                uvs4D = new List<Vector4>[UVChannelCount];
                for (int channel = 0; channel < UVChannelCount; channel++)
                {
                    if (_vertUV4D[channel] != null)
                    {
                        List<Vector4> uvs = new List<Vector4>(vertices.Length);
                        GetUVs(channel, uvs);
                        uvs4D[channel] = uvs;
                    }
                }
            }

            return MeshUtils.CreateMesh(vertices, indices, normals, tangents, colors, boneWeights, uvs2D, uvs3D, uvs4D, _bindposes);
        }
        #endregion
        #endregion
    }
}
