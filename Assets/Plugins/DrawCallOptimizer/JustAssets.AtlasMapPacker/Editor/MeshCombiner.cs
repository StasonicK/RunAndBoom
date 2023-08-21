using System.Collections.Generic;
using System.Linq;
using JustAssets.AtlasMapPacker.AtlasMapping;
using JustAssets.AtlasMapPacker.Meshes;
using JustAssets.ColliderUtilityEditor;
using JustAssets.ColliderUtilityRuntime;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace JustAssets.AtlasMapPacker
{
    internal class MeshCombiner
    {
        private GameObject _mainGameObject;

        private Material[] _materials;

        private bool _considerDisabled;

        private ICollection<GameObject> _objectsToCombine;

        public int LODLevel { get; set; }

        public MeshCombiner(ICollection<GameObject> objectsToCombine, bool considerDisabled, int lodToUse = 0)
        {
            _objectsToCombine = objectsToCombine;
            _mainGameObject = objectsToCombine.FirstOrDefault();
            _considerDisabled = considerDisabled;
            LODLevel = lodToUse;
            var meshOwners = CollectMeshOwners(objectsToCombine, considerDisabled);

            HasLODs = objectsToCombine.SelectMany(x => x.GetComponentsInChildren<LODGroup>()).Any();
            Mappings = MeshUtil.MapMeshesToMaterials(meshOwners);
            Issues = MeshUtil.AnalyzeMeshes(Mappings);
        }

        public bool HasLODs { get; }

        public Mesh FinalMesh { get; private set; }

        public Mesh ColliderMesh { get; private set; }

        public List<AtlasMapEntry> Layout { get; private set; }

        public List<MeshAnalysisResult> Issues { get; private set; }

        public List<KeyValuePair<Material, List<MeshUtil.MeshAndMatrix>>> Mappings { get; }

        public void Combine()
        {
            ColliderMesh = ColliderUtility.Create(_mainGameObject, new ColliderCombineSettings(layersToConsider: -1));

            var targetRotationMatrix = Matrix4x4.TRS(Vector3.zero, _mainGameObject.transform.rotation,
                _mainGameObject.transform.localScale);
            FinalMesh = MeshUtil.MergeMeshes(Mappings, _mainGameObject.transform.worldToLocalMatrix, _mainGameObject.name, targetRotationMatrix, Layout, out var materials);

            _materials = materials;
        }

        public void FixMeshes()
        {
            MeshUtil.FixIssues(Issues);
            Issues = MeshUtil.AnalyzeMeshes(Mappings);
        }

        public void LayoutAtlas(bool justStatic)
        {
            Layout = MeshUtil.LayoutGroups(Mappings, justStatic);
        }

        public void Save(string saveFolder)
        {
            foreach (var gameObject in _objectsToCombine)
            {
                gameObject.SetActive(false);
            }
            
            var newInstance = Object.Instantiate(_mainGameObject);
            newInstance.name = _mainGameObject.name;
            newInstance.isStatic = true;

            var instanceTransform = newInstance.transform;

            instanceTransform.SetParent(_mainGameObject.transform.parent, false);

            WipeOutMergedComponents(newInstance);

            WipeOutEmptyGameObjects(newInstance);

            FinalMesh = AssetDatabaseHelper.SaveAsset(FinalMesh, _mainGameObject.name + ".asset", saveFolder, string.Empty);
            ColliderMesh = AssetDatabaseHelper.SaveAsset(ColliderMesh, _mainGameObject.name + "_Collider.asset", saveFolder, string.Empty);

            var filter = newInstance.GetOrAddComponent<MeshFilter>();
            filter.sharedMesh = FinalMesh;

            var renderer = newInstance.GetOrAddComponent<MeshRenderer>();
            renderer.sharedMaterials = _materials;

            var meshCollider = newInstance.GetOrAddComponent<MeshCollider>();
            meshCollider.sharedMesh = ColliderMesh;
            meshCollider.convex = false;

            newInstance.SetActive(true);

            Selection.activeGameObject = newInstance;
        }

        private void WipeOutEmptyGameObjects(GameObject newInstance)
        {
            var emptyNodes = newInstance.GetComponentsInChildren<Transform>(_considerDisabled).Select(x => x.gameObject)
                .Where(x => x.GetComponentsInChildren<Component>(true).All(r => r is Transform)).ToList();

            emptyNodes.Remove(newInstance);

            foreach (GameObject gameObject in emptyNodes)
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        private static void WipeOutMergedComponents(GameObject newInstance)
        {
            List<Component> allComponentsToStrip = newInstance.GetComponentsInChildren<MeshRenderer>(true)
                .Union<Component>(newInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                .Union(newInstance.GetComponentsInChildren<MeshFilter>(true))
                .Union(newInstance.GetComponentsInChildren<LODGroup>())
                .Union(newInstance.GetComponentsInChildren<Collider>().Where(c => !c.isTrigger))
                .ToList();

            foreach (Component component in allComponentsToStrip)
            {
                try
                {
                    Object.DestroyImmediate(component);
                }
                catch
                {
                    switch (component)
                    {
                        case MeshRenderer meshRenderer:
                            meshRenderer.sharedMaterials = new Material[0];
                            break;
                        case SkinnedMeshRenderer skinned:
                            skinned.sharedMesh = null;
                            skinned.sharedMaterials = new Material[0];
                            break;
                        case MeshFilter meshFilter:
                            meshFilter.sharedMesh = null;
                            break;
                    }

                    Debug.LogWarning("Could not remove component due to dependency.", component);
                }
            }
        }

        private List<MeshOwner> CollectMeshOwners(ICollection<GameObject> objectsToCombine, bool considerDisabled)
        {
            var meshOwners = new List<MeshOwner>();

            foreach (GameObject gameObject in objectsToCombine)
            {
                if (!considerDisabled && !gameObject.activeInHierarchy)
                    continue;

                var lods = gameObject.GetComponentsInChildren<LODGroup>(considerDisabled);

                // Get all renderers which are inactive
                var allInactiveLodRenderers = lods.SelectMany(x => x.GetLODs().Where((y, i) => i != LODLevel))
                    .SelectMany(x => x.renderers).ToList();

                var skinnedRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(considerDisabled);
                var renderers = gameObject.GetComponentsInChildren<MeshRenderer>(considerDisabled);
                var allRenderers = skinnedRenderers.OfType<Renderer>().Union(renderers).Except(allInactiveLodRenderers).ToList();

                foreach (var renderer in allRenderers)
                {
                    var rendererGameObject = renderer.gameObject;
                    var filter = rendererGameObject.GetComponent<MeshFilter>();

                    if (filter != null && filter.sharedMesh != null && (considerDisabled || renderer.enabled))
                    {
                        meshOwners.Add(new MeshOwner(rendererGameObject, filter.sharedMesh));
                        continue;
                    }

                    var skinned = renderer as SkinnedMeshRenderer;

                    if (skinned != null && skinned.sharedMesh != null && (considerDisabled || skinned.enabled))
                        meshOwners.Add(new MeshOwner(rendererGameObject, skinned.sharedMesh));
                }
            }

            return meshOwners;
        }

        public void Merge(bool justStatic, string saveFolderPath)
        {
            if (Issues.Count > 0)
                FixMeshes();
        
            LayoutAtlas(justStatic);
            Combine();
            Save(saveFolderPath);
        }
    }
}