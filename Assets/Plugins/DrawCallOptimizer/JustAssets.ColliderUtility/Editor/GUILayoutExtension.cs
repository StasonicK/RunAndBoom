using UnityEditor;
using UnityEngine;

namespace JustAssets.ColliderUtilityEditor
{
    public static class GUILayoutExtension
    {
        public static DefaultAsset DrawSaveFolder(DefaultAsset saveFolder, string defaultPath)
        {
            EditorGUILayout.BeginHorizontal();
            var lw = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;
            saveFolder =
                EditorGUILayout.ObjectField("Save folder: ", saveFolder, typeof(DefaultAsset), false) as DefaultAsset;

            if (saveFolder == null)
                if (GUILayout.Button("Create", GUILayout.Width(80)))
                {
                    saveFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(defaultPath);

                    if (saveFolder == null)
                        saveFolder = UnityEditorUtils.CreateFolderRecursive(defaultPath);
                }

            EditorGUIUtility.labelWidth = lw;
            EditorGUILayout.EndHorizontal();

            return saveFolder;
        }

        public static Texture2D ScaledTexture2D(this Texture2D original, int targetWidth, int targetHeight)
        {
            var rt = new RenderTexture(targetWidth, targetHeight, 24);
            RenderTexture.active = rt;
            Graphics.Blit(original, rt);
            var result = new Texture2D(targetWidth, targetHeight);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();
            return result;
        }
    }
}