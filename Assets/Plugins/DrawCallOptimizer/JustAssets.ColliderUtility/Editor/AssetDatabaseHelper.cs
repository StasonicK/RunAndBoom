using System.IO;
using UnityEditor;
using UnityEngine;

namespace JustAssets.ColliderUtilityEditor
{
    public static class AssetDatabaseHelper
    {
        public static string GetSavePath(string fileName, string saveFolder, string subFolder)
        {
            string folderPath = string.Format("{0}/{1}", saveFolder, subFolder);
            string absoluteFolderPath = Path.GetFullPath(folderPath);

            if (!Directory.Exists(absoluteFolderPath))
                Directory.CreateDirectory(absoluteFolderPath);

            return folderPath + string.Format("/{0}", fileName);
        }

        public static T SaveAsset<T>(T inData, string fileName, string saveFolder, string subFolder) where T : Object
        {
            string savePath = GetSavePath(fileName, saveFolder, subFolder);

            savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);
            AssetDatabase.CreateAsset(inData, savePath);

            return AssetDatabase.LoadAssetAtPath<T>(savePath);
        }
    }
}