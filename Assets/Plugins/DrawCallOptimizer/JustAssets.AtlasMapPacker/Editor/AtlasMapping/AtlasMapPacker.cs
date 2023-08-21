using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AtlasUtility;
using JetBrains.Annotations;
using JustAssets.AtlasMapPacker.Meshes;
using JustAssets.AtlasMapPacker.Rendering;
using JustAssets.ColliderUtilityEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using ShaderUtil = JustAssets.AtlasMapPacker.Rendering.ShaderUtil;

namespace JustAssets.AtlasMapPacker.AtlasMapping
{
    public class AtlasMapPacker
    {
        private readonly bool _canAttributeAtlasBeShrunk;

        private readonly bool _createRestoreData;

        private readonly float _colorSimilarityThreshold;
       
        private readonly AtlasSize _minimalTexSize;

        private readonly Action<string, float> _showProgress;

        private GameObject[] _scannedObjects;

        public AtlasMapPacker(Action<string, float> showProgress,
            bool canAttributeAtlasBeShrunk,
            bool createRestoreData,
            float colorSimilarityThreshold,
            AtlasSize minimalTexSize)
        {
            _showProgress = showProgress;
            _canAttributeAtlasBeShrunk = canAttributeAtlasBeShrunk;
            _createRestoreData = createRestoreData;
            _colorSimilarityThreshold = colorSimilarityThreshold;
            _minimalTexSize = minimalTexSize;
        }

        public Dictionary<MaterialConfiguration, LayerDetails> MaterialConfigToAtlas { get; private set; }

        public Dictionary<MaterialConfiguration, List<MaterialUsage>> MaterialConfigurations { get; private set; }

        public string MainAssetName { get; private set; }

        public void BakeTextures(string saveFolder)
        {
            var smallStep = 0.05f;
            var progress = 0f;
            var materialStep = 1f / MaterialConfigToAtlas.Count;

            _showProgress?.Invoke("Baking textures", progress);
            foreach (var layer in MaterialConfigToAtlas)
            {
                _showProgress?.Invoke($"Baking texture '{layer.Key.DisplayName}'", progress + smallStep);
                MaterialConfiguration materialConfiguration = layer.Key;
                LayerDetails layerDetails = layer.Value;

                if (layerDetails == null)
                    continue;

                if (layerDetails.TextureSlotNames == null)
                    continue;

                if (materialConfiguration.ShaderInfo.TargetShaderName == null)
                    continue;

                var maxMipCount = MathUtil.MipCount(layerDetails.Margin);
                layerDetails.HighestAtlasMipLevel = maxMipCount;
                var tiles = layerDetails.Tiles;
                var minTileSizeInPixels = tiles.Last().Rectangle.Size;
                var atlasSize = layerDetails.AtlasSize;

                var allMaterials = tiles.SelectMany(x => x.Payload<List<MaterialUsage>>()).Select(x => x.Material).Distinct().ToList();

                var attributes = materialConfiguration.ShaderInfo.ShaderAttributesToTransfer;
                var textureSlotsToTileArea = tiles.Select(x => new TextureTile(x.Payload<List<MaterialUsage>>().First(), attributes, x.Rectangle)).ToList();

                var textureSlotNames = new Dictionary<string, bool>();
    
                foreach (var slotName in layerDetails.TextureSlotNames)
                    textureSlotNames.Add(slotName, false);

                var mappedAttributes = attributes.Where(x =>
                    x.Source.DataSource != DataSource.TextureAttribute && x.Target != null && x.Target.DataSource == DataSource.TextureAttribute).ToList();

                var targetSlots = mappedAttributes.Select(x => x.Target.Name).Distinct().ToList();

                foreach (var targetSlot in targetSlots)
                {
                    if (!textureSlotNames.ContainsKey(targetSlot))
                        textureSlotNames.Add(targetSlot, _canAttributeAtlasBeShrunk);
                }

                var textureStep = materialStep / textureSlotNames.Count;

                foreach (var nameAndPurpose in textureSlotNames)
                {
                    AttributePointerPair mapping = attributes.FirstOrDefault(x => x.Source.Name == nameAndPurpose.Key);
                    CreateAtlasTextureForSlot(saveFolder, progress, atlasSize, minTileSizeInPixels, layerDetails,
                        textureSlotsToTileArea, allMaterials, materialConfiguration.DisplayName, mapping,
                        nameAndPurpose.Key, nameAndPurpose.Value);

                    progress += textureStep;
                }

                progress += materialStep;
            }
            _showProgress?.Invoke(null, 1f);
        }

        private void CreateAtlasTextureForSlot(string saveFolder,
            float progress,
            PixelSize atlasSize,
            PixelSize minTileSizeInPixels,
            LayerDetails layerDetails,
            List<TextureTile> textureSlotsToTileArea,
            List<Material> allMaterials,
            string materialDisplayName,
            AttributePointerPair mapping,
            string atlasName,
            bool noCompression)
        {
            var layerDetailsMargin = layerDetails.Margin;
            var atlasTargetName = mapping == null ? atlasName : mapping.Target?.Name;

            if (atlasTargetName == null)
                return;

            _showProgress?.Invoke($"Baking texture '{materialDisplayName}.{atlasName}'", progress);
            var localAtlasSize = noCompression
                ? new PixelSize(MathUtil.ToNextPowerOfTwo(atlasSize.Width / minTileSizeInPixels.Width),
                    MathUtil.ToNextPowerOfTwo(atlasSize.Height / minTileSizeInPixels.Height)) * 2
                : atlasSize;

            var localScale = new Vector2(localAtlasSize.Width / (float)atlasSize.Width, localAtlasSize.Height / (float)atlasSize.Height);
            var localMargin = (int) (layerDetailsMargin * (noCompression ? localScale.x : 1f));
            var configsPerSlot = GetConfigsPerSlot(textureSlotsToTileArea, localScale, atlasName, allMaterials);

            MemoryStream stream = null;
            try
            {
                stream = AtlasMapWriter.SaveAtlas(configsPerSlot, new Vector2Int((int) localAtlasSize.Width, (int) localAtlasSize.Height), localMargin, ReadTexture2D);
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"Failed to save atlas texture '{materialDisplayName}'...\n{ex}");
            }

            if (stream == null)
                return;
            
            _showProgress?.Invoke($"Importing atlas map '{materialDisplayName}.{atlasName}'",
                progress);

            var outputFileName = $"{materialDisplayName}{atlasName}.png";
            var atlasTexture = ImageFileIO.SaveAtlasTexture(stream, saveFolder, MainAssetName, outputFileName, atlasName,
                noCompression);

            layerDetails.AtlasTextures[atlasTargetName] = atlasTexture;
        }

        private static List<AtlasMapEntry> GetConfigsPerSlot(List<TextureTile> textureSlotsToTileArea,
            Vector2 localScale,
            string name,
            List<Material> allMaterials)
        {
            var configsPerSlot = textureSlotsToTileArea.Select(x =>
            {
                long localScaleX = Mathf.FloorToInt(x.Rectangle.X * localScale.x);
                long localScaleY = Mathf.FloorToInt(x.Rectangle.Y * localScale.y);
                long rectangleWidth = Mathf.CeilToInt(x.Rectangle.Width * localScale.x);
                long rectangleHeight = Mathf.CeilToInt(x.Rectangle.Height * localScale.y);
                var scaledRect = new PixelRect(localScaleX, localScaleY, rectangleWidth, rectangleHeight);
                return new AtlasMapEntry(scaledRect, new AtlasPayload(x, name, allMaterials));
            }).ToList();
            return configsPerSlot;
        }

        public void ClearTextureSlot(string textureSlot, MaterialConfiguration materialConfiguration)
        {
            if (!MaterialConfigToAtlas.TryGetValue(materialConfiguration, out LayerDetails layer))
                return;

            foreach (AtlasMapEntry entry in layer.Tiles)
            {
                var usages = entry.Payload<List<MaterialUsage>>();

                foreach (MaterialUsage materialUsage in usages)
                    materialUsage.Material.SetTexture(textureSlot, null);
            }

            Scan(_scannedObjects);
        }

        public void CreateMaterials(string saveFolder)
        {
            foreach (var layer in MaterialConfigToAtlas)
            {
                LayerDetails layerDetails = layer.Value;

                if (layerDetails == null)
                    continue;

                MaterialConfiguration configuration = layer.Key;

                var materials = layerDetails.Tiles.SelectMany(x => x.Payload<List<MaterialUsage>>().Select(r => r.Material)).Distinct().ToList();

                IShaderInfo shaderInfo = configuration.ShaderInfo;

                if (string.IsNullOrEmpty(shaderInfo.TargetShaderName))
                {
                    Debug.LogError($"Could not find target shader for {configuration.Shader.name}.");
                    continue;
                }

                Material newMaterial = ShaderUtil.CreateMaterial(materials, shaderInfo, layerDetails.AtlasTextures);

                newMaterial.SetFloat("_AtlasWidth", layerDetails.AtlasSize.Width);
                newMaterial.SetFloat("_AtlasHeight", layerDetails.AtlasSize.Height);

                SetSpecularAndGlossyFlags(materials, newMaterial);
                SetMaxAttributes(materials, newMaterial, shaderInfo.ShaderAttributesToTransfer);
                SetIlluminationSetting(materials, newMaterial);

                layerDetails.AtlasMaterial = AssetDatabaseHelper.SaveAsset(newMaterial, configuration.DisplayName + ".mat", saveFolder, MainAssetName);
            }
        }

        public void CreateNewMeshes(string saveFolder)
        {
            var newData = new Dictionary<(Renderer, MeshFilter), Dictionary<Material, List<Mesh>>>();

            var maxUVIndex = 2;

            try
            {
                foreach (var layerDetails in MaterialConfigToAtlas)
                {
                    MaterialConfiguration materialConfig = layerDetails.Key;
                    LayerDetails atlas = layerDetails.Value;
                    Material atlasMaterial = atlas.AtlasMaterial;
                    var atlasMaterialName = atlasMaterial?.name ?? "[none/pass through]";
                    EditorUtility.DisplayProgressBar("Creating meshes", $"Material {atlasMaterialName}...", 0);

                    foreach (AtlasMapEntry atlasMapEntry in atlas.Tiles)
                    {
                        var materialUsages = atlasMapEntry.Payload<List<MaterialUsage>>();
                        var atlasRect = atlasMapEntry.UVRectangle;
                        var cache =
                            new Dictionary<(Mesh mesh, int oldSlot, AtlasRect uvRect, Material oldMaterial, IShaderInfo shaderInfo), Mesh>();

                        foreach (MaterialUsage materialUsage in materialUsages)
                        {
                            Material oldMaterial = materialUsage.Material;
                            var oldSlot = materialUsage.SlotIndex;
                            Renderer oldRenderer = materialUsage.Renderer;
                            MeshFilter oldFilter = materialUsage.Filter;

                            EditorUtility.DisplayProgressBar("Creating meshes",
                                $"Renderer {atlasMaterialName}.{oldRenderer.name}...", 0);

                            (Renderer oldRenderer, MeshFilter oldFilter) valueTuple = (oldRenderer, oldFilter);
                            if (!newData.TryGetValue(valueTuple, out var data))
                                data = newData[valueTuple] = new Dictionary<Material, List<Mesh>>();

                            List<Mesh> meshUsages;
                            if (atlasMaterial == null)
                            {
                                meshUsages = data[oldMaterial] = new List<Mesh>();
                            }
                            else if (!data.TryGetValue(atlasMaterial, out meshUsages))
                            {
                                meshUsages = data[atlasMaterial] = new List<Mesh>();
                            }

                            Mesh oldMesh = oldFilter.sharedMesh;

                            if (oldMesh == null)
                            {
                                Debug.LogWarning($"Mesh was missing for {oldFilter.name}[{oldSlot}]",
                                    oldFilter.gameObject);
                                continue;
                            }

                            var shaderInfo = materialConfig.ShaderInfo;
                            if (!cache.TryGetValue((oldMesh, oldSlot, atlasRect, oldMaterial, shaderInfo), out var cachedMesh))
                            {
                                Mesh extractSubmesh = MeshUtil.ExtractSubmesh(oldMesh, oldSlot);

                                if (atlasMaterial != null)
                                {
                                    SetAtlasData(extractSubmesh, atlasRect, oldMaterial, shaderInfo, maxUVIndex);
                                }

                                meshUsages.Add(extractSubmesh);
                                cache[(oldMesh, oldSlot, atlasRect, oldMaterial, shaderInfo)] = extractSubmesh;
                            }
                            else
                            {
                                meshUsages.Add(cachedMesh);
                            }
                        }
                    }
                }

                if (_createRestoreData)
                {
                    foreach ((Renderer renderer, MeshFilter meshFilter) in newData.Keys)
                    {
                        var originalData = renderer.gameObject.AddComponent<OriginalRendererAndFilter>();
                        originalData.Materials = renderer.sharedMaterials;
                        originalData.Mesh = meshFilter.sharedMesh;
                    }
                }

                ApplyNewMeshesAndMaterials(saveFolder, newData);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public bool HasShaderErrors()
        {
            return MaterialConfigurations.Keys.Any(shader => shader.Shader.name.Contains("InternalErrorShader"));
        }

        public void Optimize(string saveFolderPath)
        {
            BakeTextures(saveFolderPath);
            CreateMaterials(saveFolderPath);
            CreateNewMeshes(saveFolderPath);
        }

        public static void RestoreOriginalData()
        {
            var originalData = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<OriginalRendererAndFilter>(true));

            foreach (OriginalRendererAndFilter originalRendererAndFilter in originalData)
            {
                GameObject owner = originalRendererAndFilter.gameObject;
                var renderer = owner.GetComponent<Renderer>();
                var filter = owner.GetComponent<MeshFilter>();

                if (renderer != null)
                    renderer.sharedMaterials = originalRendererAndFilter.Materials;

                if (filter != null)
                    filter.sharedMesh = originalRendererAndFilter.Mesh;

                Object.DestroyImmediate(originalRendererAndFilter);
            }
        }

        public void Scan(GameObject[] objectsToScan)
        {
            _scannedObjects = objectsToScan;
            MainAssetName = objectsToScan.FirstOrDefault()?.gameObject.name;
            var affectedRenderers = objectsToScan
                .SelectMany(x => x.GetComponentsInChildren<Renderer>())
                .Where(x => !(x is ParticleSystemRenderer))
                .Where(x => !(x is SkinnedMeshRenderer))
                .ToList();
            var materialUsage = new Dictionary<MaterialConfiguration, List<MaterialUsage>>();
            var shaderInfoFactory = new ShaderInfoFactory();

            foreach (Renderer affectedRenderer in affectedRenderers)
            {
                var filter = affectedRenderer.GetComponent<MeshFilter>();
                var materials = affectedRenderer.sharedMaterials;

                for (var index = 0; index < materials.Length; index++)
                {
                    Material material = materials[index];

                    if (material == null)
                    {
                        continue;
                    }
                    var config = new MaterialConfiguration(shaderInfoFactory, material);
                    if (!materialUsage.TryGetValue(config, out var usage))
                        usage = materialUsage[config] = new List<MaterialUsage>();

                    usage.Add(new MaterialUsage(affectedRenderer, index, filter, _colorSimilarityThreshold));
                }
            }

            MaterialConfigurations = materialUsage;
        }

        public bool TryCreateLayouts(uint margin, AtlasingStrategy strategy, uint maximumTextureSize)
        {
            var success = true;
            var layerPerShader = new Dictionary<MaterialConfiguration, LayerDetails>();

            foreach (var materialConfig in MaterialConfigurations)
            {
                IReadOnlyCollection<MaterialUsage> materialUsages = materialConfig.Value;
                MaterialConfiguration materialConfiguration = materialConfig.Key;

                Debug.Log($"Creating layout for material {materialConfiguration.DisplayName}...");

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                LayerDetails layoutForMaterial = CreateLayoutForMaterial(margin, materialUsages, materialConfiguration, strategy, maximumTextureSize);
                if (layoutForMaterial == null)
                {
                    success = false;
                    var entries = materialUsages.Select(x => new AtlasMapEntry(PixelRect.Zero, new List<MaterialUsage> { x })).ToList();
                    layoutForMaterial = new LayerDetails(entries, PixelSize.Zero, 0, null);
                }
                stopwatch.Stop();
                Debug.Log($"Created layout for material {materialConfiguration.DisplayName}. Success: {success}. Took: {TimespanToReadable(stopwatch.Elapsed)}");

                layerPerShader[materialConfiguration] = layoutForMaterial;
            }

            MaterialConfigToAtlas = layerPerShader;
            return success;
        }

        private string TimespanToReadable(TimeSpan elapsed)
        {
            int minutes = (int)elapsed.TotalMinutes;
            double fsec = 60 * (elapsed.TotalMinutes - minutes);
            int ms = (int) (1000 * (fsec - (int)fsec));
            return $"{minutes}:{(int)fsec:D2}.{ms}";
        }

        public bool TryGetUnsupportedShaders(out List<string> unsupportedShaders)
        {
            unsupportedShaders = new List<string>();
            foreach (MaterialConfiguration materialConfiguration in MaterialConfigurations.Keys)
            {
                if (!UnityEngine.Shader.Find(materialConfiguration.ShaderInfo.TargetShaderName))
                    unsupportedShaders.Add(materialConfiguration.Shader.name);
            }

            return unsupportedShaders.Count > 0;
        }

        private void ApplyNewMeshesAndMaterials(string saveFolder, Dictionary<(Renderer, MeshFilter), Dictionary<Material, List<Mesh>>> newData)
        {
            var meshCombinationCash = new Dictionary<int, Mesh>();
            var cache = new Dictionary<(int, int), Mesh>();

            foreach (var owner in newData)
            {
                Renderer renderer = owner.Key.Item1;
                MeshFilter filter = owner.Key.Item2;
                var ownerValue = owner.Value;

                var usedMaterials = ownerValue.Keys;
                renderer.sharedMaterials = usedMaterials.ToArray();

                Mesh[] meshes = new Mesh[renderer.sharedMaterials.Length];
                List<List<Mesh>> meshSubmeshes = ownerValue.Values.ToList();
                for (var index = 0; index < meshSubmeshes.Count; index++)
                {
                    var submeshList = meshSubmeshes[index];
                    var contentHash = submeshList.GetContentHash();
                    if (!meshCombinationCash.TryGetValue(contentHash, out var cashedCombinedMesh))
                    {
                        var combineMeshes = MeshUtil.CombineMeshes(true, submeshList.ToArray());
                        meshes[index] = combineMeshes;
                        meshCombinationCash[contentHash] = combineMeshes;
                    }
                    else
                    {
                        meshes[index] = cashedCombinedMesh;
                    }
                }
                var materialHash = usedMaterials.GetContentHash();
                var meshHash = meshes.GetContentHash();
                if (!cache.TryGetValue((materialHash, meshHash), out var cashedCombinedFinalMesh))
                {
                    Mesh sharedMesh = MeshUtil.CombineMeshes(false, meshes);
                    AssetDatabaseHelper.SaveAsset(sharedMesh, owner.Key.Item1.name + ".asset", saveFolder, MainAssetName);
                    filter.sharedMesh = sharedMesh;
                    cache[(materialHash, meshHash)] = sharedMesh;
                }
                else
                {
                    filter.sharedMesh = cashedCombinedFinalMesh;
                }
            }

            MaterialConfigToAtlas = null;
        }

        private static void AssignToComponent(ValueComponent targetComponent, float[] storage, float value)
        {
            switch (targetComponent)
            {
                case ValueComponent.X:
                    storage[0] = value;
                    break;
                case ValueComponent.Y:
                    storage[1] = value;
                    break;
                case ValueComponent.Z:
                    storage[2] = value;
                    break;
                case ValueComponent.W:
                    storage[3] = value;
                    break;
            }
        }

        private static List<List<MaterialUsage>> ComputeUniqueCombinations(MaterialConfiguration materialConfiguration,
            IReadOnlyCollection<MaterialUsage> materialUsages)
        {
            var mappedAttributes = materialConfiguration.ShaderInfo.ShaderAttributesToTransfer.Where(x => x.Target != null && x.Source != null).ToList();

            var uniqueCombinations = new UniqueCombinationSet();
            foreach (MaterialUsage materialUsage in materialUsages)
            {
                var textureConfigsBySlot = materialUsage.MaterialTextures.TextureSlots;
                var colorConfigsBySlot = materialUsage.MaterialTextures.ColorSlots;
                var configsBySlot = new Dictionary<string, IAttributeConfig>();
                foreach (var slot in textureConfigsBySlot)
                {
                    configsBySlot[slot.Key] = slot.Value;
                }
                foreach (var slot in colorConfigsBySlot)
                {
                    configsBySlot[slot.Key] = slot.Value;
                }

                var mappedTextureConfigsBySlot = configsBySlot
                    .Where(x => mappedAttributes.Any(u => u.Source.Name == x.Key && u.Target.DataSource == DataSource.TextureAttribute))
                    .ToDictionary(k => k.Key, v => v.Value);

                List<IAttributeConfig> textureConfigs = mappedTextureConfigsBySlot.Select(x => x.Value).ToList();
                

                if (uniqueCombinations.TryGetValue(textureConfigs, out UniqueCombination existingCombination))
                    existingCombination.Add(materialUsage);
                else
                    uniqueCombinations.Add(new UniqueCombination(new[] {materialUsage}, textureConfigs));
            }

            return new List<List<MaterialUsage>>(uniqueCombinations);
        }

        private IAtlasTile CreateAtlasTileForUsages(List<MaterialUsage> usages, Vector2Int maxTileSize)
        {
            var textures = usages.SelectMany(usage => usage.MaterialTextures.TextureSlots.Values.Select(config => config.Texture))
                .Where(texture => texture != null).ToList();

            textures.Sort((a, b) => (a.width * a.height).CompareTo(b.width * b.height));
            AtlasSize dim = textures.Count > 0 ? new AtlasSize(textures.Last().width, textures.Last().height) : _minimalTexSize;
            var size = new PixelSize((long)Math.Min(dim.Width, maxTileSize.x), (long)Math.Min(dim.Height, maxTileSize.y));
            return new AtlasTile<List<MaterialUsage>>(size, usages);
        }

        private LayerDetails CreateLayoutForMaterial(uint margin, IReadOnlyCollection<MaterialUsage> materialUsages, MaterialConfiguration materialConfig,
            AtlasingStrategy strategy, uint maximumTextureSize)
        {
            var maxTileSize = new Vector2Int((int)maximumTextureSize,(int)maximumTextureSize);

            var uniqueCombinations = ComputeUniqueCombinations(materialConfig, materialUsages);

            var dimensions = uniqueCombinations.Select(x => CreateAtlasTileForUsages(x, maxTileSize)).ToList();

            var bestAtlasSize = AtlasMapUtil.ComputePOTSize(dimensions.Select(x => x.Size), margin, strategy);
            List<AtlasMapEntry> entries = null;

            while (entries == null && bestAtlasSize.Width <= maximumTextureSize && bestAtlasSize.Height <= maximumTextureSize)
            {
                var maxSide = Math.Max(bestAtlasSize.Width, bestAtlasSize.Height);
                if (maxSide > maximumTextureSize)
                {
                    var scale = maxSide / maximumTextureSize;
                    bestAtlasSize = new PixelSize(bestAtlasSize.Width / scale, bestAtlasSize.Height / scale);
                }

                if (bestAtlasSize.Width <= margin * 2 || bestAtlasSize.Height <= margin * 2)
                    return null;

                entries = AtlasMapUtil.Match(dimensions, margin, bestAtlasSize, ShowProgress);

                if (entries == null)
                    bestAtlasSize *= 2L;
            }

            if (entries == null)
                return null;

            var slotNames = new List<string>();

            var textureSlots = materialUsages.First().MaterialTextures.TextureSlots;
            foreach (var slotName in textureSlots.Keys)
            {
                if (materialUsages.Any(x => x.MaterialTextures.TextureSlots[slotName].Texture != null))
                    slotNames.Add(slotName);
            }

            var result = new LayerDetails(entries, bestAtlasSize, margin, slotNames);
            return result;
        }

        private void ShowProgress(string job, string task, float progress)
        {
            _showProgress?.Invoke(job != null ? $"{job}|{task}" : null, progress);
        }

        private static float GetMax(Material x, string name)
        {
            switch (x.shader.GetPropertyType(name))
            {
                case ShaderPropertyType.Color:
                case ShaderPropertyType.Vector:
                    return x.GetColor(name).maxColorComponent;
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    return x.GetFloat(name);
            }

            return 1;
        }

        private static float Pack(float a, float b, int precision = 3)
        {
            if (b >= 1f)
                throw new ArgumentOutOfRangeException(nameof(b));

            return Mathf.Round((float) (a * Math.Pow(10, precision))) + b;
        }

        private static Texture2D ReadMaterial(List<AttributePointerPair> attributes, PixelSize tileSize, Material sourceMaterial, List<Material> materials)
        {
            var readMaterial = new Texture2D((int)tileSize.Width, (int)tileSize.Height, TextureFormat.ARGB32, false);

            var values = new float[4];
            for (var index = 0; index < attributes.Count; index++)
            {
                AttributePointerPair attribute = attributes[index];
                if (sourceMaterial.HasProperty(attribute.Source.Name))
                {
                    switch (sourceMaterial.shader.GetPropertyType(attribute.Source.Name))
                    {
                        case ShaderPropertyType.Color:
                        case ShaderPropertyType.Vector:

                            for (var i = 0; i < 4; i++)
                            {
                                values[i] = ReadNormalizedProperty(attribute, sourceMaterial, materials, m => m.GetVector(attribute.Source.Name)[i]);
                            }

                            break;
                        case ShaderPropertyType.Float:
                        case ShaderPropertyType.Range:
                            AssignToComponent(attribute.Target.Component, values,
                                ReadNormalizedProperty(attribute, sourceMaterial, materials, m => m.GetFloat(attribute.Source.Name)));
                            break;
                    }
                }
            }

            for (var index = 0; index < values.Length; index++)
            {
                var clamp = Mathf.Clamp(values[index] * 255, 0, 255);
                if (clamp > 0 && clamp < 1)
                    clamp = 1;
                values[index] = clamp;
            }

            if (values.Any(x => x < 0 || x > 255))
                throw new ArgumentOutOfRangeException(nameof(values));

            var color = new Color32((byte) values[0], (byte) values[1], (byte) values[2], (byte) values[3]);
            readMaterial.ClearTo(color);

            return readMaterial;
        }

        private static float ReadNormalizedProperty(AttributePointerPair attribute, Material sourceMaterial, List<Material> materials,
            Func<Material, float> read)
        {
            var max = attribute.Target != null && !string.IsNullOrEmpty(attribute.Target.MaximumAttribute)
                ? Math.Max(1, materials.Where(x => x.HasProperty(attribute.Source.Name)).Max(x => GetMax(x, attribute.Source.Name)))
                : 1;

            var value = read(sourceMaterial) / max;
            if (value > 1 || value < 0)
            {
                Debug.LogWarning($"The value for '{sourceMaterial.name}.{attribute.Source.Name}' falls outside of valid range 0..1: {value:F3}");
            }

            return value;
        }

        private static Texture2D ReadTexture2D(AtlasMapEntry atlasMapEntry)
        {
            var payload = atlasMapEntry.Payload<AtlasPayload>();
            MaterialUsage materialUsage = payload.TextureTile.MaterialUsage;
            var mappings = payload.TextureTile.Attributes.Where(x =>
                x.Source.DataSource != DataSource.TextureAttribute && x.Target != null && x.Target.Name == payload.TextureName).ToList();

            if (!materialUsage.MaterialTextures.TextureSlots.TryGetValue(payload.TextureName, out TextureConfig textureSlot))
            {
                return mappings.Count > 0 ? ReadMaterial(mappings, atlasMapEntry.Rectangle.Size, materialUsage.Material, payload.AllMaterials) : null;
            }

            // Handle attribute baking here (missing slot on material)
            if (textureSlot.Texture != null)
                return ImageManipulation.ReadTexture(textureSlot.Texture);

            // Handle attribute baking here (missing texture on slot)
            if (mappings.Count > 0)
                return ReadMaterial(mappings, atlasMapEntry.Rectangle.Size, materialUsage.Material, payload.AllMaterials);

            var image = new Texture2D(16, 16, TextureFormat.ARGB32, false);
            image.ClearTo(new Color32((byte) (textureSlot.TextureFallbackColor.r * 255), (byte) (textureSlot.TextureFallbackColor.g * 255),
                (byte) (textureSlot.TextureFallbackColor.b * 255), (byte) (textureSlot.TextureFallbackColor.a * 255)));

            return image;
        }

        private void SetAtlasData(Mesh submesh, AtlasRect placement, Material oldMaterial, IShaderInfo shaderInfo, int atlasCoordIndex)
        {
            var uvLength = submesh.uv.Length;
            var uvAtlas = new Vector4[uvLength];

            var atlasInfo = new Vector4(placement.X, placement.Y, placement.Width, placement.Height);

            var tilingToFetch = shaderInfo.ShaderAttributesToTransfer.FirstOrDefault(x => x?.Target?.UseTiling ?? false)?.Source.Name;
            if (tilingToFetch != null)
            {
                Vector2 scale = oldMaterial.GetTextureScale(tilingToFetch);
                Vector2 offset = oldMaterial.GetTextureOffset(tilingToFetch);

                atlasInfo.x = Pack(offset.x, atlasInfo.x);
                atlasInfo.y = Pack(offset.y, atlasInfo.y);
                atlasInfo.z = Pack(scale.x, atlasInfo.z);
                atlasInfo.w = Pack(scale.y, atlasInfo.w);
            }
            else
            {
                atlasInfo.z = Pack(1, atlasInfo.z);
                atlasInfo.w = Pack(1, atlasInfo.w);
            }

            for (var i = 0; i < uvAtlas.Length; i++)
                uvAtlas[i] = atlasInfo;

            MaterialConfiguration.CopyAttributesToMesh(shaderInfo.ShaderAttributesToTransfer, oldMaterial, submesh);

            submesh.SetUVs(atlasCoordIndex, uvAtlas.ToList());
        }

        private void SetIlluminationSetting(List<Material> materials, Material newMaterial)
        {
            MaterialGlobalIlluminationFlags illumination =
                materials.Aggregate<Material, MaterialGlobalIlluminationFlags>(0, (current, material) => current | material.globalIlluminationFlags);

            illumination =
                illumination.HasFlag(MaterialGlobalIlluminationFlags.RealtimeEmissive) && !illumination.HasFlag(MaterialGlobalIlluminationFlags.BakedEmissive)
                    ? MaterialGlobalIlluminationFlags.RealtimeEmissive
                    : MaterialGlobalIlluminationFlags.BakedEmissive;

            newMaterial.globalIlluminationFlags = illumination;
        }

        private void SetMaxAttributes(List<Material> materials, Material newMaterial, List<AttributePointerPair> attributes)
        {
            foreach (AttributePointerPair attributeMapping in attributes.Where(x => x.Target != null && !string.IsNullOrEmpty(x.Target.MaximumAttribute)))
            {
                var materialsWithAttribute = materials.Where(x => x.HasProperty(attributeMapping.Source.Name)).ToList();
                var maxValue = materialsWithAttribute.Count != 0 ? Math.Max(1, materialsWithAttribute.Max(x => GetMax(x, attributeMapping.Source.Name))) : 1f;

                newMaterial.SetFloat(attributeMapping.Target.MaximumAttribute, maxValue);
            }
        }

        private static void SetSpecularAndGlossyFlags(IReadOnlyCollection<Material> materials, Material newMaterial)
        {
            var anySpecularHighlights = materials.Any(x => x.HasProperty("_SpecularHighlights") && x.GetFloat("_SpecularHighlights") >= 1f);

            var anyGlossyReflections = materials.Any(x => x.HasProperty("_GlossyReflections") && x.GetFloat("_GlossyReflections") >= 1f);

            newMaterial.SetFloat("_SpecularHighlights", anySpecularHighlights ? 1f : 0f);
            newMaterial.SetFloat("_GlossyReflections", anyGlossyReflections ? 1f : 0f);
        }

        private class UniqueCombination : List<MaterialUsage>
        {
            public UniqueCombination([NotNull] ICollection<MaterialUsage> attributes, ICollection<IAttributeConfig> valueCollection) : base(attributes)
            {
                Attributes = valueCollection;
            }

            public ICollection<IAttributeConfig> Attributes { get; }
        }

        private class UniqueCombinationSet : List<UniqueCombination>
        {
            public bool TryGetValue(ICollection<IAttributeConfig> attributes, out UniqueCombination existingCombination)
            {
                existingCombination = this.FirstOrDefault(uniqueCombination => uniqueCombination.Attributes.SequenceEqual(attributes));

                return existingCombination != null;
            }
        }
    }
}