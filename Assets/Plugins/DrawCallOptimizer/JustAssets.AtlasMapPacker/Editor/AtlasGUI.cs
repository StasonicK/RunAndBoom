using System;
using System.Collections.Generic;
using System.Linq;
using JustAssets.AtlasMapPacker.AtlasMapping;
using JustAssets.AtlasMapPacker.Rendering;
using UnityEditor;
using UnityEngine;

namespace JustAssets.AtlasMapPacker
{
    internal class AtlasGUI
    {
        private const string UIScanSelected = "Scan selected";

        private readonly Action<List<AtlasMapEntry>, Rect, Mesh> _drawAtlasMap;

        private readonly GUIData _data;

        private bool[] _atlasMenuIsOpen;

        private AtlasMapping.AtlasMapPacker _atlasser;

        private Rect _controlRect;

        private Vector2 _scrollPos = Vector2.zero;

        private GUIStyle _smallTextStyle;
    
        public AtlasGUI(GUIData data, Action<List<AtlasMapEntry>, Rect, Mesh> drawAtlasMap)
        {
            _drawAtlasMap = drawAtlasMap;
            _data = data;
        }

        private static int AtlasMarginInPixels { get; set; } = 8;

        public void Draw(Rect position)
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));

            AtlasMarginInPixels = Math.Max(1, EditorGUILayout.IntField("Margin", AtlasMarginInPixels));
            _data.Strategy = (AtlasingStrategy) EditorGUILayout.EnumPopup("Matching strategy", _data.Strategy);
            _data.MaximumTextureSize = EditorDialogExtension.DrawTextureSizeDropDown(_data.MaximumTextureSize);

            if (GUILayout.Button(UIScanSelected))
            {
                _atlasser = new AtlasMapping.AtlasMapPacker(ShowProgress, _data.CanAttributeAtlasBeShrunk,
                    _data.IsDebugOn, _data.ColorSimilarityThreshold, new AtlasSize(_data.MinimalTextureSize.x, _data.MinimalTextureSize.y));

                _atlasser.Scan(Selection.gameObjects);
                var allLayouted = _atlasser.TryCreateLayouts((uint)AtlasMarginInPixels, _data.Strategy, (uint)_data.MaximumTextureSize);
                _atlasMenuIsOpen = new bool[_atlasser.MaterialConfigToAtlas.Count];
                if (!allLayouted)
                {
                    EditorDialogExtension.ShowErrorDialog(EditorDialogExtension.ErrorCause.AtlasSizeTooLow);
                    return;
                }
            }

            var defaultBackground = GUI.backgroundColor;

            if (_atlasser?.MaterialConfigToAtlas != null)
            {
                GUILayout.Label($"Creating {_atlasser.MaterialConfigToAtlas.Count} atlas maps.");
                GUILayout.BeginVertical(EditorStyles.textArea);
                {
                    var index = 0;
                    foreach (var atlasMap in _atlasser.MaterialConfigToAtlas)
                    {
                        var shaderUnsupported = atlasMap.Key.ShaderInfo.TargetShaderName == null;
                        GUI.backgroundColor = shaderUnsupported
                            ? Color.red
                            : defaultBackground;

                        LayerDetails layerDetails = atlasMap.Value;
                        var name = $"{atlasMap.Key.DisplayName}";
                        if (shaderUnsupported)
                            name += " (unsupported)";
                        name += $"\t{layerDetails?.AtlasSize.Width}x{layerDetails?.AtlasSize.Height}\tCoverage: {layerDetails?.Coverage:P0}";

                        _atlasMenuIsOpen[index] = EditorGUILayout.BeginFoldoutHeaderGroup(_atlasMenuIsOpen[index], name);

                        GUI.backgroundColor = defaultBackground;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("", GUILayout.Height(80), GUILayout.Width(80));
                        if (Event.current.type == EventType.Repaint)
                            _controlRect = GUILayoutUtility.GetLastRect();

                        EditorGUILayout.EndHorizontal();

                        if (layerDetails != null)
                            _drawAtlasMap(layerDetails.Tiles, _controlRect, default);

                        if (_atlasMenuIsOpen[index])
                            DrawAtlasDetails(atlasMap.Key, position.width);

                        EditorGUILayout.EndFoldoutHeaderGroup();
                        index++;
                    }
                }
                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !string.IsNullOrEmpty(_data.SaveFolderPath) && _atlasser?.MaterialConfigurations != null;

            if (_data.IsDebugOn)
            {
                if (GUILayout.Button("Bake Atlas"))
                {
                    try
                    {
                        _atlasser?.BakeTextures(_data.SaveFolderPath);
                    }
                    finally
                    {
                        EditorUtility.ClearProgressBar();
                    }

                }

                if (GUILayout.Button("Create Material"))
                    _atlasser?.CreateMaterials(_data.SaveFolderPath);

                if (GUILayout.Button("Create Meshes"))
                    _atlasser?.CreateNewMeshes(_data.SaveFolderPath);

                if (GUILayout.Button("Restore Originals"))
                    AtlasMapping.AtlasMapPacker.RestoreOriginalData();
            }
            else
            {
                if (GUILayout.Button("Optimize"))
                    _atlasser?.Optimize(_data.SaveFolderPath);
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private static void ShowProgress(string text, float progress)
        {
            if (text != null)
                EditorUtility.DisplayProgressBar("Baking", text, progress);
            else
                EditorUtility.ClearProgressBar();
        }

        private void Init()
        {
            try
            {
                _smallTextStyle = EditorStyles.miniLabel;
            }
            catch
            {
            }
        }

        private void DrawAtlasDetails(MaterialConfiguration materialConfiguration, float parentWidth)
        {
            if (_atlasser?.MaterialConfigurations == null)
                return;

            Init();

            var materialConfig = _atlasser.MaterialConfigurations[materialConfiguration];

            EditorGUILayout.BeginVertical();

            if (_atlasser.MaterialConfigToAtlas.TryGetValue(materialConfiguration, out var layer))
                EditorGUILayout.LabelField(layer?.Tiles?.Count + " unique texture slot combinations",
                    _smallTextStyle);

            var textureSlots = GetUsedTextureSlots(materialConfig);
            float firstColumnWidth = 30;
            var positionWidth = parentWidth - 30 - firstColumnWidth;
            var columnWidth = positionWidth / Math.Max(1, textureSlots.Count);

            EditorGUILayout.BeginHorizontal();
            var textureSlotNames = textureSlots.Keys.ToList();
            EditorGUILayout.LabelField("", GUILayout.MaxWidth(firstColumnWidth));
            foreach (var textureSlotsKey in textureSlotNames)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(columnWidth));
                EditorGUILayout.LabelField(textureSlotsKey, GUILayout.Width(columnWidth));
                if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(20)))
                    if (EditorUtility.DisplayDialog("Wipe out texture slot",
                        $"Are you certain that you want to clear the texture slot '{textureSlotsKey}' of all materials listed here? This will modify your assets!",
                        "Clear texture usages", "Cancel"))
                        _atlasser.ClearTextureSlot(textureSlotsKey, materialConfiguration);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();

            if (layer != null && layer.Tiles != null)
            {
                foreach (var entry in layer.Tiles)
                {
                    var usages = entry.Payload<List<MaterialUsage>>();
                    var usage = usages.First();

                    EditorGUILayout.BeginHorizontal(GUILayout.Width(columnWidth));
                    for (var index = -1; index < textureSlotNames.Count; index++)
                    {
                        if (index >= 0)
                        {
                            EditorGUILayout.BeginHorizontal(GUILayout.Width(columnWidth));
                            var config = usage.MaterialTextures.TextureSlots[textureSlotNames[index]];
                            DrawTextureDetails(config, columnWidth);
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.BeginHorizontal(GUILayout.Width(firstColumnWidth));
                            EditorGUILayout.LabelField(usages.Count + "x", EditorStyles.boldLabel,
                                GUILayout.MaxWidth(firstColumnWidth));
                            EditorGUILayout.EndHorizontal();
                        }
                    }

                    if (textureSlotNames.Count == 0)
                    {
                        EditorGUILayout.BeginHorizontal(GUILayout.Width(columnWidth));
                        EditorGUILayout.LabelField("Material does not use any textures.");
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(materialConfig.Count + " usages", _smallTextStyle);
            EditorGUILayout.EndHorizontal();

            foreach (var usage in materialConfig)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(usage.Material, typeof(Material), false);
                EditorGUILayout.ObjectField(usage.Renderer, typeof(Renderer), true);
                EditorGUILayout.LabelField($"[{usage.SlotIndex}]", GUILayout.Width(50));
                EditorGUILayout.ObjectField(usage.Filter, typeof(MeshFilter), true);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawTextureDetails(TextureConfig config, float columnWidth)
        {
            EditorGUILayout.ObjectField(config.Texture, typeof(Texture), true, GUILayout.Width(60), GUILayout.Height(60));

            EditorGUILayout.BeginVertical(GUILayout.Width(columnWidth - 64));
            {
                EditorGUILayout.LabelField($"D: {config.Dimensions}", GUILayout.Width(columnWidth - 64));
                EditorGUILayout.LabelField($"S: {config.Scale}", GUILayout.Width(columnWidth - 64));
                EditorGUILayout.LabelField($"T: {config.Offset}", GUILayout.Width(columnWidth - 64));
            }
            EditorGUILayout.EndVertical();
        }

        private static Dictionary<string, TextureConfig> GetUsedTextureSlots(List<MaterialUsage> materialConfig)
        {
            var textureSlots = materialConfig.First().MaterialTextures.TextureSlots;

            var unusedSlots = new List<string>();
            foreach (var textureSlot in textureSlots)
                if (materialConfig.All(x => x.MaterialTextures.TextureSlots[textureSlot.Key].Texture == null))
                    unusedSlots.Add(textureSlot.Key);

            foreach (var unusedSlot in unusedSlots)
                textureSlots.Remove(unusedSlot);

            return textureSlots;
        }
    }
}