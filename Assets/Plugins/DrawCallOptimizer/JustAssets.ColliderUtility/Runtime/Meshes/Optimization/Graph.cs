//#define STEP_BY_STEP_EXECUTION

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime.Optimization
{
    public partial class Graph
    {
        private readonly IEqualityComparer<Vector3> _equalityComparer;
        private readonly float _faceNormalOffset;

        public List<GraphFace> Faces { get; } = new List<GraphFace>();

        public List<GraphVertex> Vertices { get; } = new List<GraphVertex>();

        public List<GraphEdge> Edges { get; } = new List<GraphEdge>();

        public Graph(Mesh mesh, Transform transform, float threshold = 0.0001f, float faceNormalOffset = 0.995f) : this(threshold, faceNormalOffset)
        {
            var ind = ExtractMeshIndices(mesh);
            var meshVertices = mesh.vertices;

            SerializeToFile(ind, meshVertices);
            CreateData(transform, meshVertices, ind);
        }


        public Graph(Vector3[] meshVertices, List<List<int>> ind, Transform transform, float threshold = 0.0001f, float faceNormalOffset = 0.995f)
            : this(threshold, faceNormalOffset)
        {
            CreateData(transform, meshVertices, ind);
        }

        private Graph(float threshold, float faceNormalOffset)
        {
            _equalityComparer = new Vector3EqualityComparer(threshold);
            _faceNormalOffset = faceNormalOffset;
        }

        public Mesh ToMesh()
        {
            var mesh = new Mesh();

            var indices = GenerateIndices(out var vertices);

            var newVertices = new Vector3[vertices.Count];
            for (var i = 0; i < vertices.Count; i++)
                newVertices[i] = vertices[i].Position;

            mesh.vertices = newVertices;
            mesh.subMeshCount = 1;
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);

            return mesh;
        }

        public static void AddToIndex(List<GraphVertex> vertices, GraphVertex vertex, List<int> indices)
        {
            var vertexIndex = vertices.IndexOf(vertex);
            if (vertexIndex < 0)
            {
                vertices.Add(vertex);
                indices.Add(vertices.Count - 1);
            }
            else
            {
                indices.Add(vertexIndex);
            }
        }

#if STEP_BY_STEP_EXECUTION
        public void DecimateEdges()
        {
            foreach (var unused in DecimateEdgesStep())
            {
            }
        }
#endif

#if STEP_BY_STEP_EXECUTION
        public void DecimateVertices()
        {
            foreach (var unused in DecimateVerticesStep())
            {
            }
        }
#endif

#if STEP_BY_STEP_EXECUTION
        public IEnumerable<KeyValuePair<GraphEdge, List<GraphFace>>> DecimateEdgesStep()
#else
        public void DecimateEdges()
#endif
        {
            foreach (var edgeToProcess in Edges)
            {
#if STEP_BY_STEP_EXECUTION
                yield return
                    new KeyValuePair<GraphEdge, List<GraphFace>>(edgeToProcess,
                        edgeToProcess.Faces.Where(x => !x.IsDeleted).ToList());
#endif
                if (edgeToProcess.IsDeleted || edgeToProcess.Faces.Count != 2)
                    continue;

                var otherFace = edgeToProcess.Faces[1];
                var thisFace = edgeToProcess.Faces[0];

                if (otherFace == thisFace)
                {
                    thisFace.RemoveEdge(edgeToProcess);
                    edgeToProcess.Faces.RemoveAll(x => x == otherFace);
                    edgeToProcess.IsDeleted = true;
                }
                else if (_equalityComparer.Equals(thisFace.Normal, Vector3.zero)
                         || _equalityComparer.Equals(otherFace.Normal, Vector3.zero)
                         || Vector3.Dot(thisFace.Normal, otherFace.Normal) >= _faceNormalOffset)
                {
                    var otherEdgesEnumerator = otherFace.Edges.First;
                    while (otherEdgesEnumerator != null)
                    {
                        if (otherEdgesEnumerator.Value != edgeToProcess)
                        {
                            var facesOfOtherEdge = otherEdgesEnumerator.Value.Faces;

                            facesOfOtherEdge.Add(thisFace);
                            facesOfOtherEdge.Remove(otherFace);
                        }

                        otherEdgesEnumerator = otherEdgesEnumerator.Next;
                    }


#if STEP_BY_STEP_EXECUTION
                    if (thisFace.Edges.Count == 0)
                        throw new Exception("Merge failed");
#endif

                    otherFace.IsDeleted = true;
                    edgeToProcess.IsDeleted = true;

                    thisFace.MergeAndClean(otherFace, edgeToProcess, _equalityComparer);

#if STEP_BY_STEP_EXECUTION
                    yield return
                        new KeyValuePair<GraphEdge, List<GraphFace>>(null,
                            edgeToProcess.Faces.Where(x => !x.IsDeleted).ToList());
#endif
                }
            }

            Faces.RemoveAll(x => x.IsDeleted || x.Edges.Count <= 2);
            Edges.RemoveAll(x => x.IsDeleted);

            var vertexMap = new HashSet<GraphVertex>();
            foreach (var edge in Edges)
            {
                vertexMap.Add(edge.VertexA);
                vertexMap.Add(edge.VertexB);
            }
            Vertices.Clear();
            Vertices.AddRange(vertexMap.ToList());
        }

#if STEP_BY_STEP_EXECUTION
        public IEnumerable<KeyValuePair<List<GraphEdge>, List<GraphFace>>> DecimateVerticesStep()
#else
        public void DecimateVertices()
#endif
        {
            var verticesToEdges = BuildVertexEdgeDependencies();

            foreach (var vertexToEdges in verticesToEdges)
            {
                var vertex = vertexToEdges.Key;
                var edges = vertexToEdges.Value;
                if (edges.Count != 2)
                    continue;

                var otherEdge = edges[1];
                var thisEdge = edges[0];

#if STEP_BY_STEP_EXECUTION
                yield return new KeyValuePair<List<GraphEdge>, List<GraphFace>>(
                    new List<GraphEdge> {otherEdge, thisEdge}, null);
#endif

                if (EdgesOnSameLine(thisEdge, otherEdge, out var newEdge))
                {
                    // Remove edge from vertices
                    verticesToEdges[otherEdge.VertexA].Remove(otherEdge);
                    verticesToEdges[otherEdge.VertexB].Remove(otherEdge);
                    verticesToEdges[thisEdge.VertexA].Remove(thisEdge);
                    verticesToEdges[thisEdge.VertexB].Remove(thisEdge);
                    Vertices.Remove(vertex);

                    // Remove edge from face
                    var otherEdgeFaces = otherEdge.Faces;
                    foreach (var face in otherEdgeFaces)
                        face.RemoveEdge(otherEdge);
                    otherEdgeFaces.Clear();

                    Edges.Remove(otherEdge);

                    // Adjust remaining edge
                    thisEdge.ReConnect(newEdge.Item1, newEdge.Item2);

                    // Register modified edge
                    verticesToEdges[thisEdge.VertexA].Add(thisEdge);
                    verticesToEdges[thisEdge.VertexB].Add(thisEdge);

#if STEP_BY_STEP_EXECUTION
                    if (Faces.Any(x => x.Edges.Count == 0))
                        throw new Exception("A face with zero edges was created due to corrupt input data.");
#endif
                }
            }

            Vertices.Clear();
            Vertices.AddRange(Edges.Select(x => x.VertexA).Union(Edges.Select(x => x.VertexB)).Distinct());
        }

        private void CreateData(Transform transform, Vector3[] meshVertices, List<List<int>> indicesBySubmesh)
        {
            var vertexMap = CreateUniqueVertices(transform, meshVertices, out var vertices, _equalityComparer);
            Vertices.Clear();
            Vertices.AddRange(vertexMap.Values);

            CreateEdgesAndFaces(indicesBySubmesh, vertices, meshVertices);
        }

        private void CreateEdgesAndFaces(List<List<int>> ind, GraphVertex[] vertices, Vector3[] meshVertices)
        {
            for (int s = 0, c = ind.Count; s < c; s++)
            {
                var indices = ind[s];

                for (var i = 0; i < indices.Count; i += 3)
                {
                    var faceEdges = new SingleLinkedList<GraphEdge>();

                    for (var t = 0; t < 3; t++)
                    {
                        var vertexA = vertices[indices[i + t]];
                        var vertexB = vertices[indices[i + (t + 1) % 3]];

                        if (!vertexA.Edges.TryGetValue(vertexB, out var existingEdge))
                        {
                            var newEdge = new GraphEdge(vertexA, vertexB);
                            vertexA.Edges[vertexB] = newEdge;
                            vertexB.Edges[vertexA] = newEdge;
                            Edges.Add(newEdge);

                            existingEdge = newEdge;
                        }

                        faceEdges.AddAtEnd(existingEdge);
                    }

                    var u = meshVertices[indices[i]];
                    var v = meshVertices[indices[i + 1]];
                    var w = meshVertices[indices[i + 2]];
                    var normal = Vector3.Cross(v - u, w - u).normalized;

                    var graphFace = new GraphFace(faceEdges, normal);

                    var enumerator = faceEdges.First;
                    while (enumerator != null)
                    {
                        enumerator.Value.Faces.Add(graphFace);
                        enumerator = enumerator.Next;
                    }

                    Faces.Add(graphFace);
                }
            }
        }

        private static Dictionary<Vector3, GraphVertex> CreateUniqueVertices(Transform transform,
            Vector3[] meshVertices,
            out GraphVertex[] vertices,
            IEqualityComparer<Vector3> comparer)
        {
            var vertexMap = new Dictionary<Vector3, GraphVertex>(comparer);

            vertices = new GraphVertex[meshVertices.Length];
            for (var index = 0; index < meshVertices.Length; index++)
            {
                var x = meshVertices[index];
                if (!vertexMap.TryGetValue(x, out var vertex))
                {
                    vertexMap[x] = vertex = new GraphVertex(x);
                    if (transform != null)
                        vertex.Position = transform.TransformPoint(vertex.Position);
                }

                vertices[index] = vertex;
            }

            return vertexMap;
        }

        private List<int> GenerateIndices(out List<GraphVertex> vertices)
        {
            var indices = new List<int>();
            vertices = new List<GraphVertex>();

            foreach (var poly in Faces)
            {
                var graphVertices = poly.ToVertices();

                if (graphVertices.Count == 0)
                    continue;

                var a = graphVertices[0];
                for (var i = 2; i < graphVertices.Count; i++)
                {
                    var b = graphVertices[i - 1];
                    var c = graphVertices[i];

                    AddToIndex(vertices, a, indices);
                    AddToIndex(vertices, b, indices);
                    AddToIndex(vertices, c, indices);
                }
            }

            return indices;
        }

        private static List<List<int>> ExtractMeshIndices(Mesh mesh)
        {
            var ind = new List<List<int>>();

            for (int i = 0, c = mesh.subMeshCount; i < c; i++)
            {
                if (mesh.GetTopology(i) != MeshTopology.Triangles)
                    continue;
                var indices = new List<int>();
                mesh.GetIndices(indices, i);
                ind.Add(indices);
            }

            return ind;
        }

        private Dictionary<GraphVertex, List<GraphEdge>> BuildVertexEdgeDependencies()
        {
            var data = new Dictionary<GraphVertex, List<GraphEdge>>();
            foreach (var graphEdge in Edges)
            {
                if (graphEdge.IsDeleted)
                    continue;

                if (!data.ContainsKey(graphEdge.VertexA))
                    data[graphEdge.VertexA] = new List<GraphEdge>();
                data[graphEdge.VertexA].Add(graphEdge);

                if (!data.ContainsKey(graphEdge.VertexB))
                    data[graphEdge.VertexB] = new List<GraphEdge>();
                data[graphEdge.VertexB].Add(graphEdge);
            }

            return data;
        }

        private bool EdgesOnSameLine(GraphEdge edgeA, GraphEdge edgeB, out (GraphVertex, GraphVertex) newEdge)
        {
            GraphVertex a;
            GraphVertex b;
            GraphVertex c;
            if (edgeA.VertexA == edgeB.VertexA)
            {
                a = edgeA.VertexB;
                b = edgeA.VertexA;
                c = edgeB.VertexB;
            }
            else if (edgeA.VertexA == edgeB.VertexB)
            {
                c = edgeA.VertexB;
                b = edgeA.VertexA;
                a = edgeB.VertexA;
            }
            else if (edgeA.VertexB == edgeB.VertexA)
            {
                a = edgeA.VertexA;
                b = edgeA.VertexB;
                c = edgeB.VertexB;
            }
            else
            {
                c = edgeA.VertexA;
                b = edgeA.VertexB;
                a = edgeB.VertexA;
            }

            var ba = (a.Position - b.Position).normalized;
            var ca = (b.Position - c.Position).normalized;

            var isSame = Vector3.Dot(ba, ca) >= _faceNormalOffset;

            if (isSame)
            {
                newEdge = (a, c);
                return true;
            }

            newEdge = default;
            return false;
        }
    }
}