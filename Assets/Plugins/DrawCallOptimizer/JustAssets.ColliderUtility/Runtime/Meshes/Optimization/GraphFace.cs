using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime.Optimization
{
    public class GraphFace
    {
        public SingleLinkedList<GraphEdge> Edges { get; }

        public Vector3 Normal { get; private set; }
        public bool IsDeleted { get; set; }

        public GraphFace(SingleLinkedList<GraphEdge> edges, Vector3 normal)
        {
            Edges = edges;
            Normal = normal;
        }

        public override string ToString()
        {
            return $"{nameof(GraphFace)} Edge Count: {Edges.Count}";
        }

        public void Merge(GraphFace otherFace, GraphEdge removeEdge, IEqualityComparer<Vector3> comparer)
        {
            if (Edges.First == null)
                throw new Exception("This face needs edges to be merged.");

            if (otherFace == this)
                throw new ArgumentException("The other face may not be this face.", nameof(otherFace));

            var thisSharedNode = Edges.Find(removeEdge, out var prevSharedNode);

            if (thisSharedNode == null)
                throw new ArgumentException(nameof(removeEdge));

            prevSharedNode = prevSharedNode ?? Edges.Last;
            var otherEdges = otherFace.Edges;
            var otherSharedNode = otherEdges.Find(removeEdge, out var pointerPrev);
            var pointerNext = otherSharedNode.Next;

            RemoveEdge(removeEdge);

            // Add pointer to end to input lists insertion point
            if (pointerNext != null)
            {
                Edges.InsertAt(prevSharedNode, pointerNext, otherEdges.Last);
                prevSharedNode = otherEdges.Last;
            }

            // Add start to pointer to input lists insertion point
            if (pointerPrev != null)
                Edges.InsertAt(prevSharedNode, otherEdges.First, pointerPrev);
            
            // Take the valid normal (if this was a zero size face than the normal is of length zero but the face can be merged with another face having a proper normal)
            Normal = (Normal + otherFace.Normal).normalized;

            otherFace.Dissolve();
        }
        

        private void Dissolve()
        {
            Edges.Clear();
        }

        public void RemoveEdge(GraphEdge edge)
        {
            Edges.Remove(edge);
        }

        public void MergeAndClean(GraphFace otherFace, GraphEdge obsoleteEdge, IEqualityComparer<Vector3> comparer)
        {
            Merge(otherFace, obsoleteEdge, comparer);

            var enumerator = Edges.First;
            while (enumerator!=null)
            {
                enumerator.Value.Usages = 0;
                enumerator = enumerator.Next;
            }

            enumerator = Edges.First;
            while (enumerator != null)
            {
                var edge = enumerator.Value;
                edge.IncreaseUsages();

                if (edge.Usages < 2)
                {
                    enumerator = enumerator.Next;
                    continue;
                }
                
                var facesOfDanglingEdge = edge.Faces;
                facesOfDanglingEdge.Remove(this);
                facesOfDanglingEdge.Remove(this);

                RemoveEdge(edge);
                RemoveEdge(edge);
                edge.IsDeleted = true;
                enumerator = enumerator.Next;
            }
        }

        public Vector3[] ToVertexStrip()
        {
            var result = new List<Vector3>();
            var verts = ToVertices();
            if (verts.Count > 0)
            {
                var a = verts[0];
                for (var i = 2; i < verts.Count; i++)
                {
                    var b = verts[i - 1];
                    var c = verts[i];

                    result.Add(a.Position);
                    result.Add(b.Position);
                    result.Add(c.Position);
                }
            }

            return result.ToArray();
        }

        public List<GraphVertex> ToVertices()
        {
            var vertices = new List<GraphVertex>();

            GraphEdge currentEdge = null;

            foreach (var edge in Edges)
            {
                var lastEdge = currentEdge;
                currentEdge = edge;

                if (lastEdge == null)
                    continue;
                
                if (vertices.Count == 0)
                {
                    if (lastEdge.VertexA == currentEdge.VertexA || lastEdge.VertexA == currentEdge.VertexB)
                    {
                        vertices.Add(lastEdge.VertexB);
                        vertices.Add(lastEdge.VertexA);
                    }
                    else
                    {
                        vertices.Add(lastEdge.VertexA);
                        vertices.Add(lastEdge.VertexB);
                    }
                }
                else
                {
                    var lastVertex = vertices.Last();
                    vertices.Add(lastEdge.VertexA == lastVertex ? lastEdge.VertexB : lastEdge.VertexA);
                }
            }

            return vertices;
        }

        public float ComputeSize()
        {
            var stripData = ToVertexStrip();
            float size = 0.0f;

            for (int i = 0; i < stripData.Length; i+=3)
            {
                var a = stripData[i];
                var b = stripData[i + 1];
                var c = stripData[i + 2];

                var lenA = (b - a).magnitude;
                var lenB = (c - b).magnitude;
                var lenC = (a - c).magnitude;

                // Length of sides must be positive
                // and sum of any two sides
                // must be smaller than third side.
                if (lenA < 0 || lenB < 0 || lenC < 0 ||
                    lenA + lenB <= lenC || lenA + lenC <= lenB ||
                    lenB + lenC <= lenA)
                {
                    size += 0;
                }
                else
                {
                    float s = (lenA + lenB + lenC) / 2;
                    size += (float)Math.Sqrt(s * (s - lenA) *
                                             (s - lenB) * (s - lenC));
                }
            }

            return size;
        }

        // Function to find area
    }
}