using System.Collections.Generic;
using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime.Optimization
{
    public class GraphVertex
    {
        public Vector3 Position { get; set; }
        public Dictionary<GraphVertex, GraphEdge> Edges { get; } = new Dictionary<GraphVertex, GraphEdge>(2);

        public GraphVertex(Vector3 position)
        {
            Position = position;
        }

        public override string ToString()
        {
            return $"{nameof(Position)}: {Position:F5}";
        }

        /// <summary>
        /// Linearly interpolate between two vertices.
        /// </summary>
        /// <param name="x">Left parameter.</param>
        /// <param name="y">Right parameter.</param>
        /// <param name="weight">The weight of the interpolation. 0 is fully x, 1 is fully y.</param>
        /// <returns>A new vertex interpolated by weight between x and y.</returns>
        public static GraphVertex Mix(GraphVertex x, GraphVertex y, float weight)
        {
            float i = 1f - weight;
            return new GraphVertex(x.Position * i + y.Position * weight);
        }
    }
}