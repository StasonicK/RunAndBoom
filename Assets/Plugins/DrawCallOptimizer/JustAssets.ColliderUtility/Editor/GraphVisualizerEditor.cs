using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime.Optimization
{
    [CustomEditor(typeof(GraphVisualizer))]
    public class GraphVisualizerEditor : Editor
    {
        public GraphEdge ActiveEdge
        {
            get => _activeEdge;
            private set
            {
                _activeEdge = value;
                EditorUtility.SetDirty(target);
            }
        }

        private int _count = 85;
        private GraphEdge _activeEdge;

        public override void OnInspectorGUI()
        {
            var graphVisualizer = (GraphVisualizer) target;
            if (GUILayout.Button("Init"))
                graphVisualizer.Create(Selection.activeGameObject);

            GUI.enabled = graphVisualizer.CanOptimizeEdges;
            if (GUILayout.Button("Dissolve Edges++"))
            {
                graphVisualizer.DissolveEdges(_count, ActiveEdge);
            }

            GUI.enabled = graphVisualizer.CanOptimizeVertices;
            if (GUILayout.Button("Dissolve Vertices++"))
                graphVisualizer.DissolveVertices(_count);

            GUI.enabled = true;

            _count = EditorGUILayout.IntField("Steps", _count);
            graphVisualizer.VertexRenderSize =
                EditorGUILayout.FloatField("Vertex Render Size", graphVisualizer.VertexRenderSize);

            DrawVertexInfo(graphVisualizer.ActiveVertex);
            DrawEdgeInfo(_activeEdge);

            base.OnInspectorGUI();
        }

        private void DrawEdgeInfo(GraphEdge activeEdge)
        {
            if (activeEdge == null)
                return;

            EditorGUILayout.LabelField("Active edge", EditorStyles.boldLabel);
            EditorGUILayout.IntField("Face count", activeEdge.Faces.Count);

            var totalSize = 0f;
            var faceSizes = new float[activeEdge.Faces.Count];
            for (var i = 0; i < activeEdge.Faces.Count; i++)
            {
                var face = activeEdge.Faces[i];
                var computeSize = face.ComputeSize();
                totalSize += computeSize;
                faceSizes[i] = computeSize;
            }

            var oldGuiColor = GUI.backgroundColor;
            for (var index = 0; index < faceSizes.Length; index++)
            {
                GUI.backgroundColor = _faceColors[index % 2];
                var faceSize = faceSizes[index];
                var currentFace = activeEdge.Faces[index];
                EditorGUILayout.LabelField($"Face size {index}: ", (faceSize * 100 / totalSize).ToString("F2"));
                EditorGUI.indentLevel++;
                var vertices = currentFace.ToVertices();
                var bounds = new Bounds(vertices.First().Position, Vector3.zero);
                foreach (var vertex in vertices)
                {
                    bounds.Encapsulate(vertex.Position);
                }

                for (var i = 0; i < vertices.Count; i++)
                {
                    var vertex = vertices[i];
                    
                    if (GUILayout.Button(i.ToString(), GUILayout.Width(30)))
                    {
                        _focusedVertex = vertex;
                        var sceneView = SceneView.lastActiveSceneView;
                        sceneView.Frame(new Bounds(vertex.Position, Vector3.one * 0.3f), false);
                        EditorUtility.SetDirty(target);
                    }
                }
                EditorGUI.indentLevel--;
            }
            GUI.backgroundColor = oldGuiColor;


            EditorGUILayout.IntField("Vertex A ECount", activeEdge.VertexA.Edges.Count(x => !x.Value.IsDeleted));
            EditorGUILayout.IntField("Vertex B ECount", activeEdge.VertexB.Edges.Count(x => !x.Value.IsDeleted));
        }

        private static void DrawVertexInfo(GraphVertex activeVertex)
        {
            if (activeVertex == null)
                return;
            
            EditorGUILayout.LabelField("Active vertex", EditorStyles.boldLabel);
            EditorGUILayout.Vector3Field("Position", activeVertex.Position);

            var edges = activeVertex.Edges;
            EditorGUILayout.IntField("Edge Count Valid", edges.Count(x => !x.Value.IsDeleted));
            var graphEdges = edges.Values.Where(x => !x.IsDeleted).ToList();
            for (var index = 0; index < graphEdges.Count; index++)
            {
                var edgeA = graphEdges[index];
                var edgeB = graphEdges[(index + 1) % graphEdges.Count];
                var dirA = GetDir(activeVertex, edgeA);
                var dirB = GetDir(activeVertex, edgeB);
                EditorGUILayout.LabelField("Direction", Vector3.Angle(dirA,dirB).ToString("F5"));
            }

            EditorGUILayout.IntField("Edge Count", edges.Count);
        }

        private static Vector3 GetDir(GraphVertex activeVertex, GraphEdge edgeA)
        {
            var toVertexA = activeVertex == edgeA.VertexA ? edgeA.VertexB : edgeA.VertexA;
            var dir = (toVertexA.Position - activeVertex.Position).normalized;
            return dir;
        }

        private readonly Color[] _faceColors = { Color.green, Color.blue };
        private GraphVertex _focusedVertex;

        protected virtual void OnSceneGUI()
        {
            var visualizer = (GraphVisualizer) target;

            if (visualizer.GraphEdges != null)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    var lowestDistance = Mathf.Infinity;
                    GraphEdge bestMatch = null;
                    foreach (var edge in visualizer.GraphEdges)
                    {
                        var d = HandleUtility.DistanceToLine(edge.VertexA.Position, edge.VertexB.Position);
                        if (d < lowestDistance)
                        {
                            lowestDistance = d;
                            bestMatch = edge;
                        }
                    }

                    if (lowestDistance < 10)
                        ActiveEdge = bestMatch;
                }

                foreach (var edge in visualizer.GraphEdges.Where(x => !x.IsDeleted))
                {
                    if (ActiveEdge == edge)
                    {
                        Handles.color = Color.cyan;
                        var colorIndex = 0;
                        foreach (var face in edge.Faces.Where(x => !x.IsDeleted))
                        {
                            var verts = face.ToVertexStrip();
                            for (int i = 0; i < verts.Length; i += 3)
                            {
                                Handles.DrawSolidRectangleWithOutline(
                                    new[] {verts[i], verts[i + 1], verts[i + 2], verts[i]},
                                    _faceColors[colorIndex], _faceColors[colorIndex]);
                            }
                            colorIndex++;
                        }
                    }
                    else
                    {
                        var d = (SceneView.lastActiveSceneView.camera.transform.position -
                                 (edge.VertexA.Position + edge.VertexB.Position) / 2).magnitude;

                        Handles.color = Color.white / d;
                    }

                    DrawLine(edge);
                }
            }

            Handles.color = Color.blue;

            if (visualizer.CurrentEdgeOptimization != null)
            {
                var currentValue = visualizer.CurrentEdgeOptimization?.Value;
                if (currentValue != null)
                    for (var index = 0; index < currentValue.Count; index++)
                    {
                        Handles.color = index == 0 ? Color.green : Color.black;

                        var face = currentValue[index];

                        if (face.IsDeleted)
                            continue;

                        foreach (var edge in face.Edges.Where(x => !x.IsDeleted))
                            DrawLine(edge);
                    }

                var activeEdge = visualizer.CurrentEdgeOptimization?.Key;
                if (activeEdge != null)
                {
                    Handles.color = Color.magenta;

                    if (!activeEdge.IsDeleted)
                        DrawLine(activeEdge, 4);
                }
            }


            if (visualizer.Vertices != null)
            {
                Handles.color = Color.white;
                foreach (var vertex in visualizer.Vertices)
                {
                    Color color = Color.white;
                    if (visualizer.ActiveVertex != null)
                    {
                        foreach (var activeEdge in visualizer.ActiveVertex.Edges.Values)
                        {
                            Handles.color = activeEdge.IsDeleted ? Color.red / 5 : Color.red;
                            DrawLine(activeEdge, 3);
                        }

                        color = vertex == visualizer.ActiveVertex ? Color.red / 2 : Color.blue;
                    }

                    Handles.color = _focusedVertex != null && _focusedVertex == vertex ? Color.magenta : color;

                    if (Handles.Button(vertex.Position, Quaternion.identity, visualizer.VertexRenderSize,
                        visualizer.VertexRenderSize * 2, Handles.CubeHandleCap))
                    {
                        visualizer.ActiveVertex = vertex;
                        EditorUtility.SetDirty(target);
                    }
                }
            }
        }

        private static void DrawLine(GraphEdge edge, int thickness = 2)
        {
#if UNITY_2020_1_OR_NEWER
            Handles.DrawLine(edge.VertexA.Position, edge.VertexB.Position, thickness);
#else
            Handles.DrawLine(edge.VertexA.Position, edge.VertexB.Position);
#endif
        }
    }
}