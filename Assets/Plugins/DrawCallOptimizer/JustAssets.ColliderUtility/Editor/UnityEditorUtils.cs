using UnityEditor;

namespace JustAssets.ColliderUtilityEditor
{
    public static class UnityEditorUtils
    {
        public static DefaultAsset CreateFolderRecursive(string fullPath)
        {
            string subPath = "";

            string[] folders = fullPath.Split('/');

            string lastGuid = null;
            string parentPath = "";
            foreach (string folder in folders)
            {
                subPath += folder;
                if (AssetDatabase.LoadAssetAtPath<DefaultAsset>(subPath) == null)
                    lastGuid = AssetDatabase.CreateFolder(parentPath, folder);
                parentPath = subPath;
                subPath += "/";
            }

            if (!string.IsNullOrEmpty(lastGuid))
                return AssetDatabase.LoadAssetAtPath<DefaultAsset>(AssetDatabase.GUIDToAssetPath(lastGuid));

            return null;
        }
    }
}