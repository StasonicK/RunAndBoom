//#define STEP_BY_STEP_EXECUTION

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime.Optimization
{
    [ExecuteInEditMode]
    public class GraphVisualizer : MonoBehaviour
    {
        private Graph _graph;
        private IEnumerator<KeyValuePair<GraphEdge, List<GraphFace>>> _edgeOptimization;
        private IEnumerator<KeyValuePair<List<GraphEdge>, List<GraphFace>>> _vertexOptimization;
        public float VertexRenderSize { get; set; } = 0.1f;

        public bool CanOptimizeEdges => _edgeOptimization != null;
        public bool CanOptimizeVertices => _vertexOptimization != null;
        public List<GraphVertex> Vertices { get; private set; }
        public GraphVertex ActiveVertex { get; set; }
        public List<GraphEdge> GraphEdges => _graph?.Edges;
        public KeyValuePair<GraphEdge, List<GraphFace>>? CurrentEdgeOptimization => _edgeOptimization?.Current;


        public void Create(GameObject activeGameObject)
        {
            var meshes = activeGameObject.GetComponentsInChildren<MeshFilter>();

            var combinesInstances =
                meshes.Select(x => new CombineInstance {mesh = x.sharedMesh, transform = x.transform.localToWorldMatrix}).ToList();

            var mesh = new Mesh();
            mesh.CombineMeshes(combinesInstances.ToArray());

            _graph = new Graph(mesh, transform);
            Vertices = _graph.Vertices;
#if STEP_BY_STEP_EXECUTION
            _edgeOptimization = _graph.DecimateEdgesStep().GetEnumerator();
            _vertexOptimization = _graph.DecimateVerticesStep().GetEnumerator();
#endif
        }

        public void DissolveEdges(int i, GraphEdge activeEdge)
        {
            if (!CanOptimizeEdges)
                return;

            for (int j = 0; j < i; j++)
            {
                if (_edgeOptimization==null)
                    break;

                bool moveNext = _edgeOptimization.MoveNext();

                if (_edgeOptimization.Current.Key != null && _edgeOptimization.Current.Key.Equals(activeEdge))
                    Debugger.Break();

                if (!moveNext)
                    _edgeOptimization = null;

            }
#if UNITY_EDITOR
            SceneView.RepaintAll();
#endif
        }

        public void DissolveVertices(int i)
        {
            if (!CanOptimizeVertices)
                return;

            for (int j = 0; j < i; j++)
            {
                if (_vertexOptimization == null)
                    break;
                
                var moveNext = _vertexOptimization.MoveNext();
                if (!moveNext)
                    _vertexOptimization = null;
            }
#if UNITY_EDITOR
            SceneView.RepaintAll();
#endif
        }
    }
}