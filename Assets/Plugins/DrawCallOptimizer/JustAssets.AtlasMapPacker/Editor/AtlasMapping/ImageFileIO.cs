using System.IO;
using JustAssets.ColliderUtilityEditor;
using UnityEditor;
using UnityEngine;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    internal static class ImageFileIO
    {
        public static Texture SaveAtlasTexture(MemoryStream imageStream, string saveFolder, string subFolderName, string fileName, string textureSlotName,
            bool noCompression)
        {
            imageStream.Seek(0, SeekOrigin.Begin);

            var imageData = new byte[imageStream.Length];
            imageStream.Read(imageData, 0, (int) imageStream.Length);
            var savePath = AssetDatabaseHelper.GetSavePath(fileName, saveFolder, subFolderName);
            File.WriteAllBytes(savePath, imageData);

            AssetDatabase.ImportAsset(savePath);
            var asset = AssetDatabase.LoadAssetAtPath<Texture>(savePath);

            SetImportSettings(textureSlotName, noCompression, savePath);
            return asset;
        }

        private static void SetImportSettings(string textureSlotName, bool noCompression, string savePath)
        {
            var isNormalMap = textureSlotName.Contains("Bump") || textureSlotName.Contains("Normal");
            if (isNormalMap || noCompression)
            {
                var importer = AssetImporter.GetAtPath(savePath) as TextureImporter;
                if (importer != null)
                {
                    if (isNormalMap)
                    {
                        importer.textureType = TextureImporterType.NormalMap;
                    }

                    if (noCompression)
                    {
                        importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{format = TextureImporterFormat.RGBA32});
                    }

                    importer.SaveAndReimport();
                }
            }
        }
    }
}