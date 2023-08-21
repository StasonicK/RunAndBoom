using JustAssets.ColliderUtilityRuntime.Geometry;
using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime.Optimization
{
    public class GraphPerformanceBehavior : MonoBehaviour
    {
        private float _time;

        public void OnEnable()
        {
            _time = Time.time;
        }

        public void Update()
        {
            if (Time.time - _time < 2)
                return;

            _time = Time.time;

            var go = QuadGenerator.CreatePlaneOfTries(16, 16, out var colliderMesh);

            var graph = new Graph(colliderMesh, go.transform);
            graph.DecimateEdges();
            graph.DecimateVertices();
        }
    }
}