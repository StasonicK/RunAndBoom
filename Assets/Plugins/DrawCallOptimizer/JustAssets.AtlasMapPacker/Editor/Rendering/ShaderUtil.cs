using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JustAssets.ShaderPatcher;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace JustAssets.AtlasMapPacker.Rendering
{
    internal static class ShaderUtil
    {
        public static Material CreateMaterial(List<Material> sourceMaterials, IShaderInfo shaderInfo,
            Dictionary<string, Texture> atlasTextures)
        {
            var newShader = UnityEngine.Shader.Find(shaderInfo.TargetShaderName);
            var material = new Material(newShader);

            var attributesWithSameTypeMapping = shaderInfo.ShaderAttributesToTransfer.Where(x =>
                x.Target != null && x.Source.DataSource == x.Target.DataSource).ToList();

            foreach (var pointerPair in shaderInfo.ShaderAttributesToTransfer
                .Where(x => x.Target != null && x.Target.DataSource == DataSource.TextureAttribute))
            {
                var name = pointerPair.Target.Name;
                if (material.HasProperty(name))
                {
                    if (atlasTextures.TryGetValue(name, out var atlasTexture))
                        material.SetTexture(name, atlasTexture);
                }
                else
                {
                    Debug.LogWarning($"Could not assign texture '{name}' on material '{material.name}'");
                }
            }

            // Transfer attributes which are supposed to be most common value
            foreach (var pointerPair in attributesWithSameTypeMapping.Where(x=>
                x.Source.DataSource == DataSource.FloatAttribute))
            {
                if (!material.HasProperty(pointerPair.Target.Name))
                    continue;

                var materialsWithProp = sourceMaterials.Where(x => x.HasProperty(pointerPair.Source.Name));
                var possibleValues = materialsWithProp.Select(x => x.GetFloat(pointerPair.Source.Name)).GroupBy(x => x).ToList();

                if (possibleValues.Count > 0)
                {
                    var mostCommon = possibleValues.First().Key;
                    material.SetFloat(pointerPair.Target.Name, mostCommon);
                }
            }

            foreach (var pointerPair in attributesWithSameTypeMapping.Where(x=>
                x.Source.DataSource == DataSource.VectorAttribute))
            {
                if (!material.HasProperty(pointerPair.Target.Name))
                    continue;

                var mostCommon = sourceMaterials.Where(x => x.HasProperty(pointerPair.Source.Name))
                    .Select(x => x.GetVector(pointerPair.Source.Name)).GroupBy(x => x).First().Key;

                material.SetVector(pointerPair.Target.Name, mostCommon);
            }

            foreach (var keywordTransferInstruction in shaderInfo.KeywordTransferInstructions)
            {
                if (keywordTransferInstruction.Condition.IsTrue(sourceMaterials))
                {
                    material.EnableKeyword(keywordTransferInstruction.Keyword);
                }
                else
                {
                    material.DisableKeyword(keywordTransferInstruction.Keyword);
                }
            }

            foreach (var renderQueueSelectionInstruction in shaderInfo.RenderQueueMapping)
            {
                if (renderQueueSelectionInstruction.Condition.IsTrue(sourceMaterials))
                {
                    material.renderQueue = renderQueueSelectionInstruction.TargetRenderQueue;
                    break;
                }
            }

            foreach (var renderTypeSelectionInstruction in shaderInfo.RenderTypeMapping)
            {
                if (renderTypeSelectionInstruction.Condition.IsTrue(sourceMaterials))
                {
                    material.SetOverrideTag("RenderType", renderTypeSelectionInstruction.TargetRenderType);
                    break;
                }
            }
            
            return material;
        }

        public static void OpenSourceShader(UnityEngine.Shader shader)
        {
        
            if (shader == null)
                return;

            var path = GetShaderPath(shader);

            if (!File.Exists(path))
            {
                Debug.Log("Opening shader at path: " + path);
                EditorUtility.DisplayDialog("Cannot open shader",
                    "Failed to determine shader source. It should be part of Unity's builtin shaders or inside of this project.", "Ok");
                return;
            }

            Process.Start(path);
        }

        private const string ASSET_PATH_BASE_PACKAGE = "Packages/de.justassets.atlasmappacker/Assets";
        private const string ASSET_PATH_BASE_LOCAL = "Assets/Tools/DrawCallOptimizer/JustAssets.AtlasMapPacker/Assets";

        private static string GetShaderPath(UnityEngine.Shader shader)
        {
            var path = GetLocalShaderPath(shader);

            path = Path.GetFullPath(path);
            return path;
        }

        private static string GetLocalShaderPath(UnityEngine.Shader shader)
        {
            var path = AssetDatabase.GetAssetPath(shader);

            if (path == "Resources/unity_builtin_extra")
            {
                path = FindShaderWithName(shader.name, $"{BasePath}/Shaders~/DefaultResourcesExtra/");
            }

            return path;
        }

        private static string FindShaderWithName(string shaderName, string basePath)
        {
            var path = Directory.EnumerateFiles(Path.GetFullPath(basePath), "*.shader", SearchOption.AllDirectories)
                .Select(filePath => (filePath, Regex.Match(File.ReadAllText(filePath), "Shader \"([^\"]+)\"")))
                .Where(x => x.Item2.Success)
                .FirstOrDefault(x => x.Item2.Groups[1].Captures[0].Value == shaderName).filePath;

            var rootPath = Directory.GetParent(Application.dataPath).FullName;

            path = path.Substring(rootPath.Length+1);

            return path;
        }

        public static string BasePath
        {
            get
            {
                var fullPath = Path.GetFullPath(ASSET_PATH_BASE_PACKAGE);
                return !Directory.Exists(fullPath) ? ASSET_PATH_BASE_LOCAL : ASSET_PATH_BASE_PACKAGE;
            }
        }

        public static UnityEngine.Shader CreateAtlasShader(UnityEngine.Shader dataSourceShader, XmlShaderInfo xmlShaderInfo)
        {
            var shaderPath = GetLocalShaderPath(dataSourceShader);
            var localTargetPath = Path.ChangeExtension(shaderPath, "Atlas.shader").Replace("~", "");
            var targetPath = Path.GetFullPath(localTargetPath);

            var folderPath = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var parser = new ShaderParser(shaderPath, BasePath + "/Shaders~/");
            var patcher = new ShaderPatcher.ShaderPatcher(parser, (prio, message) => Debug.unityLogger.Log((LogType) prio, message));
            var replacements = patcher.PatchInAtlasSupport();
            xmlShaderInfo.SetReplacements(replacements);
            parser.Write(targetPath);
            
            AssetDatabase.ImportAsset(localTargetPath);

            return AssetDatabase.LoadAssetAtPath<UnityEngine.Shader>(localTargetPath);
        }

        public static void CreateAtlasShaders(List<string> unsupportedShaders)
        {
            var shaderInfoFactory = new ShaderInfoFactory();
            foreach (var unsupportedShader in unsupportedShaders)
            {
                var xmlShaderInfo = (XmlShaderInfo)shaderInfoFactory.Create(unsupportedShader);
                CreateAtlasShader(UnityEngine.Shader.Find(unsupportedShader), xmlShaderInfo);
                xmlShaderInfo.Save();
            }
        }
    }
}