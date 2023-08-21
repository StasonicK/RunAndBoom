using System;
using System.Collections.Generic;
using JustAssets.AtlasMapPacker.AtlasMapping;
using JustAssets.AtlasMapPacker.Meshes;
using UnityEditor;
using UnityEngine;

namespace JustAssets.AtlasMapPacker
{
    internal class CombineGUI
    {
        private readonly Action<List<AtlasMapEntry>, Rect, Mesh> _drawAtlasMap;

        private Rect _controlRect;

        private bool[] _meshCombineInfoOpen;

        private MeshCombiner _meshCombiner;

        private Vector2 _scrollPos;

        private Vector2 _scrollPosAtlasDetails;

        private Mesh _selectedMesh;

        private static GUIContent _justStaticContent = new GUIContent("Just static game objects",
            "Select this option to remap only lightmap UVs for objects marked static. UVs of non-static objects will be discarded and those objects won't be able to receive lightmaps.");

        private GUIData _data;

        public CombineGUI(GUIData data, Action<List<AtlasMapEntry>, Rect, Mesh> drawAtlasMap)
        {
            _data = data;
            _drawAtlasMap = drawAtlasMap;
        }
    
        public void Draw()
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));

            if (_meshCombiner?.HasLODs ?? false)
                _meshCombiner.LODLevel = EditorGUILayout.IntSlider("Use LOD level", _meshCombiner.LODLevel, 0, 5);

            if (GUILayout.Button(GUIShared.UIScanSelected))
            {
                _meshCombiner = new MeshCombiner(Selection.gameObjects, _data.AddDisabledGameObjects, _meshCombiner?.LODLevel ?? 0);
                _meshCombineInfoOpen = new bool[_meshCombiner.Mappings.Count];
            }

            if (_meshCombiner?.Mappings != null)
            {
                GUILayout.Label($"Found {_meshCombiner.Mappings.Count} unique materials.");

                if (_meshCombiner.Issues.Count > 0)
                {
                    EditorGUILayout.HelpBox($"Detected {_meshCombiner.Issues.Count} problems.", MessageType.Warning);

                    GUILayout.BeginVertical(EditorStyles.textArea);
                    foreach (var meshAnalysisResult in _meshCombiner.Issues)
                        GUILayout.Label($"{meshAnalysisResult.Message} for '{meshAnalysisResult.Sender.name}'.");
                    GUILayout.EndVertical();
                }

                if (_meshCombiner.Layout != null)
                {
                    var atlasMapEntries = _meshCombiner.Layout;

                    EditorGUILayout.BeginHorizontal();
                    var size = 256;
                    EditorGUILayout.LabelField("", EditorStyles.textArea, GUILayout.Height(size), GUILayout.Width(size));
                    if (Event.current.type == EventType.Repaint)
                    {
                        var controlRect = GUILayoutUtility.GetLastRect();
                        const int margin = 5;
                        controlRect = new Rect(controlRect.x + margin, controlRect.y + margin,
                            size - 2 * margin, size - 2 * margin);
                        _controlRect = controlRect;
                    }

                    _scrollPosAtlasDetails = EditorGUILayout.BeginScrollView(_scrollPosAtlasDetails);
                    EditorGUILayout.BeginVertical();
                    {
                        foreach (var atlasMapEntry in atlasMapEntries)
                            if (atlasMapEntry.Payload<Mesh>() == _selectedMesh)
                                EditorGUILayout.RectField(_selectedMesh.name,
                                    new Rect(atlasMapEntry.UVRectangle.X, atlasMapEntry.UVRectangle.Y,
                                        atlasMapEntry.UVRectangle.Width, atlasMapEntry.UVRectangle.Height));
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndScrollView();

                    EditorGUILayout.EndHorizontal();
                    GUILayoutUtility.GetRect(10f, 10f);

                    _drawAtlasMap(atlasMapEntries, _controlRect, _selectedMesh);
                }

                GUILayout.BeginVertical(EditorStyles.textArea);
                {
                    var index = 0;
                    foreach (var mapping in _meshCombiner.Mappings)
                    {
                        _meshCombineInfoOpen[index] =
                            EditorGUILayout.BeginFoldoutHeaderGroup(_meshCombineInfoOpen[index], $"{mapping.Key.name}\t{mapping.Value.Count} meshes");


                        if (_meshCombineInfoOpen[index])
                            DrawMaterialDetails(mapping);

                        EditorGUILayout.EndFoldoutHeaderGroup();
                        index++;
                    }
                }
                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();

            if (_meshCombiner?.ColliderMesh != null)
                EditorGUILayout.ObjectField("Collider mesh", _meshCombiner.ColliderMesh, typeof(Mesh), true);

            var meshWasCreated = _meshCombiner?.FinalMesh != null;
            if (meshWasCreated)
                EditorGUILayout.ObjectField("Final mesh", _meshCombiner.FinalMesh, typeof(Mesh), true);

            _data.JustStatic = EditorGUILayout.ToggleLeft(_justStaticContent, _data.JustStatic);

            EditorGUILayout.BeginHorizontal();

            var saveFolderIsSet = !string.IsNullOrEmpty(_data.SaveFolderPath);
            if (_data.IsDebugOn)
            {
                GUI.enabled = _meshCombiner?.Issues != null && _meshCombiner.Issues.Count > 0;
                if (GUILayout.Button("Fix issues"))
                    _meshCombiner.FixMeshes();

                GUI.enabled = _meshCombiner?.Mappings != null;
                if (GUILayout.Button("Layout"))
                    _meshCombiner.LayoutAtlas(_data.JustStatic);

                GUI.enabled = _meshCombiner?.Layout != null;
                if (GUILayout.Button("Combine"))
                    _meshCombiner.Combine();

                GUI.enabled = meshWasCreated && saveFolderIsSet;
                if (GUILayout.Button("Save"))
                    _meshCombiner.Save(_data.SaveFolderPath);
            }
            else
            {
                EditorGUILayout.BeginVertical();
                var meshUtilInitialized = _meshCombiner != null;
                var tooltip = GUIShared.DrawRequirements(saveFolderIsSet, meshUtilInitialized, out var isWarning);

                GUI.enabled = !isWarning;
                if (GUILayout.Button(new GUIContent("Merge", tooltip)))
                    _meshCombiner?.Merge(_data.JustStatic, _data.SaveFolderPath);
                EditorGUILayout.EndVertical();
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawMaterialDetails(KeyValuePair<Material, List<MeshUtil.MeshAndMatrix>> mappingValue)
        {
            EditorGUILayout.BeginVertical();

            foreach (var meshAndMatrix in mappingValue.Value)
            {
                EditorGUILayout.BeginHorizontal();
                if (_meshCombiner.Layout != null && GUILayout.Button("Select", GUILayout.Width(80)))
                    _selectedMesh = meshAndMatrix.Mesh;
                EditorGUILayout.ObjectField(meshAndMatrix.Mesh, typeof(Mesh), true);
                EditorGUILayout.ObjectField(meshAndMatrix.GameObject, typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
    }
}