using JustAssets.AtlasMapPacker.AtlasMapping;
using UnityEditor;
using UnityEngine;
using ShaderUtil = JustAssets.AtlasMapPacker.Rendering.ShaderUtil;

namespace JustAssets.AtlasMapPacker
{
    internal class AllInOneGUI
    {
        private GUIData _data;

        private bool _showOptions;

        public AllInOneGUI(GUIData data)
        {
            _data = data;
        }

        public void Draw()
        {
            var restore = GUI.enabled;

#if UNITY_2019_3_OR_NEWER
            _showOptions = EditorGUILayout.BeginFoldoutHeaderGroup(_showOptions, "Settings");
#else
            _showOptions = EditorGUILayout.ToggleLeft("Settings", _showOptions);
#endif

            if (_showOptions)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Combine Settings", EditorStyles.boldLabel);
                _data.LodLevel = EditorGUILayout.IntSlider("Use LOD level", _data.LodLevel, 0, 5);
                _data.AddDisabledGameObjects =
                    EditorGUILayout.ToggleLeft(new GUIContent("Add inactive", "Include disabled GameObjects/components in merge process."),
                        _data.AddDisabledGameObjects);
                _data.ColorSimilarityThreshold =
                    EditorGUILayout.Slider(
                        new GUIContent("Color similarity", "Default is 0.01. Increasing the value will merge less similar color attributes."),
                        _data.ColorSimilarityThreshold, 0.001f, 1f);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Atlas Settings", EditorStyles.boldLabel);
                _data.AtlasMarginInPixels = EditorGUILayout.IntField("Atlas Tile Margin", _data.AtlasMarginInPixels);
                _data.Strategy = (AtlasingStrategy) EditorGUILayout.EnumPopup("Atlas size strategy", _data.Strategy);
                _data.MaximumTextureSize = EditorDialogExtension.DrawTextureSizeDropDown(_data.MaximumTextureSize);
                _data.CanAttributeAtlasBeShrunk = EditorGUILayout.ToggleLeft("Attribute Atlas Can Be Shrunk", _data.CanAttributeAtlasBeShrunk);
                var minimalTextureSize = EditorDialogExtension.DrawMinTextureSizeDropDown(_data.MinimalTextureSize.x);
                _data.MinimalTextureSize = new Vector2Int(minimalTextureSize, minimalTextureSize);

                EditorGUI.indentLevel--;
            }

#if UNITY_2019_3_OR_NEWER
            EditorGUILayout.EndFoldoutHeaderGroup();
#endif

            GUILayoutUtility.GetRect(5f, 5f);

            EditorGUILayout.BeginHorizontal();
            bool saveFolderIsSet = !string.IsNullOrEmpty(_data.SaveFolderPath);

            EditorGUILayout.BeginVertical();
            var tooltip = GUIShared.DrawRequirements(saveFolderIsSet, true, out var isWarning);

            GUI.enabled = !isWarning && restore;
            if (GUILayout.Button(new GUIContent("Optimize selected", tooltip)))
            {
                Optimize();
            }

            GUI.enabled = restore;

            if (_data.IsDebugOn)
            {
                if (GUILayout.Button("Restore Originals"))
                    AtlasMapping.AtlasMapPacker.RestoreOriginalData();
            }
        
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void Optimize()
        {
            try
            {
                var combiner = new MeshCombiner(Selection.gameObjects, _data.AddDisabledGameObjects, _data.LodLevel);
                combiner.Merge(_data.JustStatic, _data.SaveFolderPath);

                var packer = new AtlasMapping.AtlasMapPacker(ShowProgress, _data.CanAttributeAtlasBeShrunk, _data.IsDebugOn, _data.ColorSimilarityThreshold, new AtlasSize(_data.MinimalTextureSize.x, _data.MinimalTextureSize.y));
                packer.Scan(Selection.gameObjects);

                if (packer.HasShaderErrors())
                {
                    EditorUtility.DisplayDialog("Shader errors",
                        $"Before you can continue, you have to fix all shader errors in the scene.",
                        "Cancel");
                    return;
                }
                
                if (packer.TryGetUnsupportedShaders(out var unsupportedShaders))
                {
                    if (EditorUtility.DisplayDialog("Missing Shaders",
                        $"This optimization tool is shipped with support for Standard shader and a few simple shaders. To improve the performance of your scene the shaders of materials need to support atlas mapping. This tool supports simple shader patching. Should we try to convert the unsupported shaders to atlas shaders now? This is a BETA feature, there is no guarantee given if the result works.\n\nUnsupported shaders:\n{string.Join("\n", unsupportedShaders.ToArray())}",
                        "Create atlas shaders", "Cancel optimization"))
                    {
                        ShaderUtil.CreateAtlasShaders(unsupportedShaders);
                        packer.Scan(Selection.gameObjects);
                    }
                    else
                    {
                        return;
                    }
                }

                if (!packer.TryCreateLayouts((uint)_data.AtlasMarginInPixels, _data.Strategy, (uint)_data.MaximumTextureSize))
                {
                    EditorDialogExtension.ShowErrorDialog(EditorDialogExtension.ErrorCause.AtlasSizeTooLow);
                    return;
                }

                packer.Optimize(_data.SaveFolderPath);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        private void ShowProgress(string info, float progress)
        {
            if (info != null)
                EditorUtility.DisplayProgressBar("Optimizing", info, progress);
            else
                EditorUtility.ClearProgressBar();
        }
    }
}
