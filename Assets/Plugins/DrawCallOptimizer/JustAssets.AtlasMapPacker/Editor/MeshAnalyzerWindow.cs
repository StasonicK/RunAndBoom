#if UNITY_2020_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AtlasUtility.Editor
{
    public sealed class MeshAnalyzerWindow : EditorWindow
    {
        private static MeshAnalyzerWindow window;

        private Mesh _mesh;

        private VertexAttributeDescriptor[] _attributes;

        private int _detailsIndex = -1;

        private List<Vector2> _channelData;

        private Rect _lastRect;

        private float _scale = 1f;

        public Mesh Mesh
        {
            get => _mesh;
            private set
            {
                if (value == _mesh)
                    return;

                _mesh = value;

                UpdateMeshInfo();
            }
        }

        private static int ConvertFormatToSize(VertexAttributeFormat format)
        {
            switch (format)
            {
                case VertexAttributeFormat.Float32:
                case VertexAttributeFormat.UInt32:
                case VertexAttributeFormat.SInt32:
                    return 4;
                case VertexAttributeFormat.Float16:
                case VertexAttributeFormat.UNorm16:
                case VertexAttributeFormat.SNorm16:
                case VertexAttributeFormat.UInt16:
                case VertexAttributeFormat.SInt16:
                    return 2;
                case VertexAttributeFormat.UNorm8:
                case VertexAttributeFormat.SNorm8:
                case VertexAttributeFormat.UInt8:
                case VertexAttributeFormat.SInt8:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException("format", format, $"Unknown vertex format {format}");
            }
        }

        private void UpdateMeshInfo()
        {
            _attributes = _mesh.GetVertexAttributes();
     
        }

        [MenuItem("Window/Mesh analyzer")]
        private static void Init()
        {
            window = (MeshAnalyzerWindow) GetWindow(typeof(MeshAnalyzerWindow), false, "Mesh Analyzer");
            window.minSize = new Vector2(400f, 200f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            DrawInfo();

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginHorizontal();
        
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawInfo()
        {
            var selectedMesh = Selection.objects.OfType<Mesh>().FirstOrDefault();

            if (selectedMesh != null)
                Mesh = selectedMesh;

            if (Mesh == null)
                return;

            EditorGUILayout.LabelField("Selected mesh: ", Mesh.name);

            int num = _attributes.Sum(attr => ConvertFormatToSize(attr.format) * attr.dimension);
            string arg = EditorUtility.FormatBytes(_mesh.vertexCount * num);
            EditorGUILayout.LabelField($"Vertices: {_mesh.vertexCount} ({arg})", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            for (var index = 0; index < _attributes.Length; index++)
            {
                var attr = _attributes[index];
                if (attr.attribute == VertexAttribute.BlendIndices || attr.attribute == VertexAttribute.BlendWeight)
                    continue;

                EditorGUILayout.BeginHorizontal();
                var attributeInfo =
                    $"{attr.format} x {attr.dimension} ({ConvertFormatToSize(attr.format) * attr.dimension} bytes)";
                EditorGUILayout.LabelField(attr.attribute.ToString(), attributeInfo);
                if (GUILayout.Button("Details", GUILayout.Width(80)))
                {
                    _detailsIndex = index;
                    UpdateDetailsData();
                }

                EditorGUILayout.EndHorizontal();
            }

            _scale = EditorGUILayout.Slider("Scale", _scale, 0.01f, 4f);

            EditorGUI.indentLevel--;

            EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.ExpandHeight(true));

            if (_detailsIndex >= 0)
            {
                Handles.BeginGUI();
                Handles.color = Color.red;
                var channelDataCount = _channelData.Count;
                for (var index = 0; index < _channelData.Count ; index+=3)
                {
                    var a = _channelData[index] * _scale;
                    var b = _channelData[(index + 1) % channelDataCount] * _scale;
                    var c = _channelData[(index + 2) % channelDataCount] * _scale;

                    //EditorGUILayout.LabelField($"[{index}]", $"X: {xFrom.x:F3}, Y: {xFrom.y:F3}");
                    Handles.DrawLine(
                        new Vector3(_lastRect.x + _lastRect.width * a.x, _lastRect.y + _lastRect.height * a.y),
                        new Vector3(_lastRect.x + _lastRect.width * b.x, _lastRect.y + _lastRect.height * b.y));
                    Handles.DrawLine(
                        new Vector3(_lastRect.x + _lastRect.width * b.x, _lastRect.y + _lastRect.height * b.y),
                        new Vector3(_lastRect.x + _lastRect.width * c.x, _lastRect.y + _lastRect.height * c.y));
                    Handles.DrawLine(
                        new Vector3(_lastRect.x + _lastRect.width * c.x, _lastRect.y + _lastRect.height * c.y),
                        new Vector3(_lastRect.x + _lastRect.width * a.x, _lastRect.y + _lastRect.height * a.y));
                }

                Handles.EndGUI();

                //Mesh.
                EditorGUILayout.LabelField("");
            }

            EditorGUILayout.EndVertical();

            if (Event.current.type == EventType.Repaint)
                _lastRect = GUILayoutUtility.GetLastRect();
        }

        private void UpdateDetailsData()
        {
            var attribute = _attributes[_detailsIndex];

            if (attribute.attribute >= VertexAttribute.TexCoord0 && attribute.attribute <= VertexAttribute.TexCoord7)
            {
                var uvChannel = attribute.attribute - VertexAttribute.TexCoord0;
                _channelData = new List<Vector2>();
                Mesh.GetUVs(uvChannel, _channelData);
            }
        }
    }
}
#endif