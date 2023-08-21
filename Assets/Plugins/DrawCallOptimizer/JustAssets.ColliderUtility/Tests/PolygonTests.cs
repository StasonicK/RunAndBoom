using System.Collections.Generic;
using JustAssets.ColliderUtilityRuntime.Optimization;
using NUnit.Framework;
using UnityEngine;

namespace JustAssets.ColliderUtilityEditorTests.Assets.ColliderUtility.Tests
{
    internal class PolygonTests
    {
        [Test]
        public void JoinTwoFaces_OneFaceIsZeroSized_OneFaceReturned()
        {
            // Create
            var mesh = new Mesh
                {
                    vertices =
                        new[]
                        {
                            new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0),
                            new Vector3(0, 0.1f, 0), new Vector3(0, 0.9f, 0)
                        },
                    triangles = new[]
                    {
                        4, 0, 3, 5, 4, 3, 0, 1, 2, 3, 0, 2
                    }
                };

            // Act
            var graph = new Graph(mesh, null);
            graph.DecimateEdges();
            graph.DecimateVertices();
            mesh = graph.ToMesh();

            // Verify
            Assert.AreEqual(4, mesh.vertexCount);
            Assert.AreEqual(2, mesh.triangles.Length / 3);
        }

        [Test]
        public void FaceToVertices()
        {
            var v0 = new GraphVertex(new Vector3(1,0,0));
            var v1 = new GraphVertex(new Vector3(0,2,0));
            var v2 = new GraphVertex(new Vector3(0,0,3));

            var face =
                new GraphFace(
                    new SingleLinkedList<GraphEdge>(new List<GraphEdge>
                    {
                        new GraphEdge(v0, v1),
                        new GraphEdge(v2, v1),
                        new GraphEdge(v0, v2)
                    }), Vector3.up);

            var sorted = face.ToVertices();

            Assert.AreEqual(v0, sorted[0]);
            Assert.AreEqual(v1, sorted[1]);
            Assert.AreEqual(v2, sorted[2]);
        }
    }
}
