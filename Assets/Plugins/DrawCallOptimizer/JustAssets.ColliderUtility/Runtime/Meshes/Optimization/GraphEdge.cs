using System;
using System.Collections.Generic;

namespace JustAssets.ColliderUtilityRuntime.Optimization
{
    public class GraphEdge
    {
        private int _usages;

        public GraphVertex VertexA { get; private set; }
        public GraphVertex VertexB { get; private set; }

        public int Usages
        {
            get => _usages;
            set => _usages = value;
        }

        public List<GraphFace> Faces { get; } = new List<GraphFace>();
        public bool IsDeleted { get; set; }

        public GraphEdge(GraphVertex vertexA, GraphVertex vertexB)
        {
            //if (Equals(vertexA, vertexB))
                //throw new ArgumentOutOfRangeException("Vertices have to be different.");

            VertexA = vertexA;
            VertexB = vertexB;
        }

        public override string ToString()
        {
            var dir = VertexB.Position - VertexA.Position;

            return $"d: {dir:F3} - {VertexA.Position:F2} - {VertexB.Position:F2}";
        }

        public void ReConnect(GraphVertex a, GraphVertex b)
        {
            VertexA = a;
            VertexB = b;
        }

        public void IncreaseUsages()
        {
            _usages++;
        }
    }
}