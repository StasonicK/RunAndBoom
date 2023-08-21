using UnityEditor;

namespace JustAssets.AtlasMapPacker
{
    internal static class GUIShared
    {
        public static string DrawRequirements(bool saveFolderIsSet, bool meshUtilInitialized, out bool isWarning)
        {
            string tooltip;
            string warn = null;

            if (!saveFolderIsSet)
                tooltip = warn = "You need to select a save folder first.";
            else if (!meshUtilInitialized)
                tooltip = warn = $"Click '{UIScanSelected}' before merging.";
            else if (Selection.gameObjects.Length == 0)
                tooltip = warn = "You need to select a gameobject in the scene.";
            else
                tooltip = "Start merging the meshes.";

            if (warn != null)
                EditorGUILayout.HelpBox(warn, MessageType.Warning);

            isWarning = warn != null;
            return tooltip;
        }

        public const string UIScanSelected = "Scan selected";
    }
}