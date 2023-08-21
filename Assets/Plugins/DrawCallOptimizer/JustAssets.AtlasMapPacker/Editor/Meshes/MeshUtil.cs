
using System;
using System.Collections.Generic;
using System.Linq;
using JustAssets.AtlasMapPacker.AtlasMapping;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace JustAssets.AtlasMapPacker.Meshes
{
    internal static class MeshUtil
    {
        private const string title = "Combining gameobjects";

        public static List<MeshAnalysisResult> AnalyzeMeshes(List<KeyValuePair<Material, List<MeshAndMatrix>>> groups)
        {
            var result = new List<MeshAnalysisResult>();

            foreach (var group in groups)
                foreach (var meshAndMatrix in group.Value)
                {
                    var mesh = meshAndMatrix.Mesh;

                    //var uvsTemp = new List<Vector2>();
                    //for (var i = 2; i < 8; i++)
                    //{
                    //    mesh.GetUVs(i, uvsTemp);
                    //    if (uvsTemp.Count > 0)
                    //        result.Add(new MeshAnalysisResult(
                    //            $"The mesh {mesh.name} uses UV channel {i} which might result in a defective mesh.", mesh,
                    //            UVSolution.ClearOtherUVSets));
                    //}

                    var lightmapUVs = mesh.uv2;

                    var lightmapBounds = lightmapUVs.Length > 0 ? ComputeUVBounds(lightmapUVs) : Rect.zero;

                    if (lightmapBounds.width > 1 || lightmapBounds.height > 1)
                    {
                        result.Add(new MeshAnalysisResult($"Lightmap UVs are broken and too large for {mesh.name}.",
                            mesh,
                            UVSolution.Unwrap));
                        continue;
                    }

                    if (lightmapUVs.Length == 0)
                        result.Add(new MeshAnalysisResult(
                            $"The mesh {mesh.name} has no lightmap UVs. They need to be computed before merging to save time.",
                            mesh, UVSolution.Unwrap));
                }

            return result;
        }

        public static Mesh CombineMeshes(bool mergeSubMeshes, params Mesh[] meshes)
        {
            var array = new CombineInstance[meshes.Length];
            for (var i = 0; i < array.Length; i++)
                array[i].mesh = meshes[i];

            var mesh = new Mesh();
            mesh.name = string.Join("+", meshes.Select(x => x.name));
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.CombineMeshes(array, mergeSubMeshes, false);
            return mesh;
        }

        public static Mesh ExtractSubmesh(Mesh mesh, int submeshIndex)
        {
            var topology = mesh.GetTopology(submeshIndex);
            if (topology != MeshTopology.Triangles)
            {
                Debug.LogWarningFormat(
                    "Extract Submesh method could handle triangle topology only. Current topology is {0}. Mesh name {1} submeshIndex {2}",
                    topology, mesh, submeshIndex);
                return mesh;
            }

            var triangles = mesh.GetTriangles(submeshIndex);
            var newTriangles = new int[triangles.Length];

            var remapping = new Dictionary<int, int>();
            var newVertexCount = 0;
            for (var i = 0; i < triangles.Length; ++i)
            {
                var index = triangles[i];
                if (!remapping.ContainsKey(index))
                {
                    newTriangles[i] = newVertexCount;

                    remapping.Add(index, newVertexCount);
                    newVertexCount++;
                }
                else
                {
                    newTriangles[i] = remapping[index];
                }
            }

            var vertices = mesh.vertices;
            var newVertices = new Vector3[newVertexCount];
            foreach (var kvp in remapping)
                newVertices[kvp.Value] = vertices[kvp.Key];

            var result = new Mesh
            {
                name = mesh.name,
                indexFormat = mesh.indexFormat,
                vertices = newVertices,
                colors = Copy(mesh.colors, vertices.Length, newVertexCount, remapping),
                colors32 = Copy(mesh.colors32, vertices.Length, newVertexCount, remapping),
                boneWeights = Copy(mesh.boneWeights, vertices.Length, newVertexCount, remapping),
                normals = Copy(mesh.normals, vertices.Length, newVertexCount, remapping),
                tangents = Copy(mesh.tangents, vertices.Length, newVertexCount, remapping)
            };

            for (var i = 0; i < 8; i++)
            {
                var uvs = new List<Vector2>();
                mesh.GetUVs(i, uvs);
                result.SetUVs(i, Copy(uvs, vertices.Length, newVertexCount, remapping).ToList());
            }

            result.triangles = newTriangles;
            return result;
        }

        public static void FixIssues(List<MeshAnalysisResult> analysisResults)
        {
            for (var index = 0; index < analysisResults.Count; index++)
            {
                var meshAnalysisResult = analysisResults[index];

                var mesh = meshAnalysisResult.Sender;
                EditorUtility.DisplayProgressBar(title, $"Resolving mesh issues for {mesh.name}...",
                    index / (float) analysisResults.Count);

                switch (meshAnalysisResult.Solution)
                {
                    case UVSolution.Unwrap:
                        Unwrapping.GenerateSecondaryUVSet(mesh);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                EditorUtility.ClearProgressBar();
            }
        }

        public static List<AtlasMapEntry> LayoutGroups(List<KeyValuePair<Material, List<MeshAndMatrix>>> groups,
            bool justStatic)
        {
            var atlasTiles = new List<IAtlasTile>();

            foreach (var group in groups)
                foreach (var meshAndMatrix in group.Value)
                {
                    var participatesInLightmapUVs = !justStatic || meshAndMatrix.GameObject.isStatic;

                    var mesh = meshAndMatrix.Mesh;
                    var uvs = mesh.uv2;

                    var lightmapBounds = ComputeUVBounds(uvs);
                    PixelSize texSize;
                    if (!participatesInLightmapUVs)
                        texSize = PixelSize.Zero;
                    else
                    {
                        Vector2 lightmapSize = ComputeLightmapSize(meshAndMatrix);
                        texSize = new PixelSize(
                            Mathf.CeilToInt(lightmapBounds.size.x * lightmapSize.x),
                            Mathf.CeilToInt(lightmapBounds.size.y * lightmapSize.y));
                    }

                    atlasTiles.Add(new LightmapTile(texSize, mesh));
                }

            EditorUtility.DisplayProgressBar(title, "Checking UVs done.", 1f);
            var requiredSize =
                AtlasMapUtil.ComputePOTSize(atlasTiles.Select(x => x.Size), 8, AtlasingStrategy.NPOT);
            var entries = AtlasMapUtil.Match(atlasTiles, 2, requiredSize, ShowProgress);
            EditorUtility.ClearProgressBar();

            return entries;
        }

        public static List<KeyValuePair<Material, List<MeshAndMatrix>>> MapMeshesToMaterials(
            IEnumerable<MeshOwner> gameObjectsWithMesh)
        {
            var meshGroups = new Dictionary<Material, List<MeshAndMatrix>>();
            foreach (var gameObjectWithMesh in gameObjectsWithMesh)
            {
                var sharedMesh = gameObjectWithMesh.SharedMesh;
                var gameObject = gameObjectWithMesh.GameObject;

                if (sharedMesh == null)
                    throw new ArgumentNullException(nameof(sharedMesh), gameObject.name);

                var meshes = Separate(sharedMesh);
                Material[] rendererMaterials;
                var renderer = gameObject.GetComponent<Renderer>();
                if (renderer == null)
                {
                    rendererMaterials = new Material[meshes.Length];
                }
                else
                {
                    rendererMaterials = renderer.sharedMaterials;
                    Array.Resize(ref rendererMaterials, meshes.Length);
                }

                for (var m = 0; m < rendererMaterials.Length; ++m)
                {
                    var material = rendererMaterials[m];
                    if (material == null)
                        continue;

                    if (!meshGroups.ContainsKey(material))
                        meshGroups.Add(material, new List<MeshAndMatrix>());

                    var group = meshGroups[material];
                    group.Add(new MeshAndMatrix(meshes[m], gameObject.transform.localToWorldMatrix, gameObject));
                    group.Sort((a, b) =>
                        string.Compare(a.GameObject.name, b.GameObject.name, StringComparison.Ordinal));
                }
            }

            var result = meshGroups.ToList();
            result.Sort((a, b) => string.Compare(a.Key.name, b.Key.name, StringComparison.Ordinal));

            return result;
        }

        public static Mesh MergeMeshes(List<KeyValuePair<Material, List<MeshAndMatrix>>> groups,
            Matrix4x4 targetToLocal,
            string name,
            Matrix4x4 targetRotationMatrix,
            List<AtlasMapEntry> atlasMapEntries,
            out Material[] materials)
        {
            const string title = "Creating missing lightmap UVs...";
            EditorUtility.DisplayProgressBar(title, "", 0);

            var hasSubMeshes = true;

            var resultMaterials = new List<Material>(groups.Count);
            var resultCombineInstances = new List<CombineInstance>(groups.Count);

            var meshToInstances = atlasMapEntries.GroupBy(x => x.Payload<Mesh>())
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var kvp in groups)
            {
                var group = kvp.Value;
                var groupCombineInstances = new List<CombineInstance>();
                var groupIndex = 0;
                var vertexCount = 0;
                while (groupIndex < group.Count)
                {
                    var meshTransform = group[groupIndex];
                    if (meshTransform.Mesh.subMeshCount > 1)
                        hasSubMeshes = false;
                    vertexCount += meshTransform.Mesh.vertexCount;
                    var groupCombineInstance = new CombineInstance
                    {
                        mesh = RemapLightmapUVs(meshTransform.Mesh, meshToInstances),
                        transform = targetToLocal * meshTransform.Transform
                    };
                    groupCombineInstances.Add(groupCombineInstance);

                    groupIndex++;
                }

                var mesh = new Mesh {indexFormat = vertexCount > 65534 ? IndexFormat.UInt32 : IndexFormat.UInt16};
                mesh.CombineMeshes(groupCombineInstances.ToArray(), true);

                resultMaterials.Add(kvp.Key);
                resultCombineInstances.Add(new CombineInstance {mesh = mesh, transform = targetRotationMatrix});
            }

            EditorUtility.DisplayProgressBar(title, "Merging meshes...", 1f);
            materials = resultMaterials.ToArray();

            var finalMesh = new Mesh {name = name, indexFormat = IndexFormat.UInt32};
            finalMesh.CombineMeshes(resultCombineInstances.ToArray(), hasSubMeshes && materials.Length <= 1, true,
                false);
            finalMesh.RecalculateBounds();

            EditorUtility.ClearProgressBar();
            return finalMesh;
        }

        public static Mesh[] Separate(Mesh mesh)
        {
            if (mesh.subMeshCount <= 1)
                return new[] {mesh};

            var result = new Mesh[mesh.subMeshCount];
            for (var i = 0; i < mesh.subMeshCount; ++i)
                result[i] = ExtractSubmesh(mesh, i);

            return result;
        }

        public static void WeldVertices(Mesh mesh, float maxDistDelta = 0.01f)
        {
            var maxDelta = maxDistDelta * maxDistDelta;
            var verts = mesh.vertices;
            var newVerts = new List<int>();
            var map = new int[verts.Length];

            // create mapping and filter duplicates.
            for (var vertexIndex = 0; vertexIndex < verts.Length; vertexIndex++)
            {
                var vert = verts[vertexIndex];

                var duplicate = false;
                for (var newVertexIndex = 0; newVertexIndex < newVerts.Count; newVertexIndex++)
                {
                    var a = newVerts[newVertexIndex];
                    if ((verts[a] - vert).sqrMagnitude <= maxDelta)
                    {
                        map[vertexIndex] = newVertexIndex;
                        duplicate = true;
                        break;
                    }
                }

                if (!duplicate)
                {
                    map[vertexIndex] = newVerts.Count;
                    newVerts.Add(vertexIndex);
                }
            }

            // create new vertices
            var vertices = new Vector3[newVerts.Count];
            for (var i = 0; i < newVerts.Count; i++)
                vertices[i] = verts[newVerts[i]];

            // map the triangle to the new vertices
            var triangles = mesh.triangles;
            for (var i = 0; i < triangles.Length; i++)
                triangles[i] = map[triangles[i]];

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
        }

        private static void ShowProgress(string header, string text, float progress)
        {
            if (header != null)
                EditorUtility.DisplayProgressBar(header, text, progress);
            else
                EditorUtility.ClearProgressBar();
        }

        private static Vector2 ComputeLightmapSize(MeshAndMatrix mesh)
        {
            var scale = mesh.Transform.lossyScale;
            var triangles = mesh.Mesh.triangles;
            var vertices = mesh.Mesh.vertices;

            var sum = 0.0;

            for (var i = 0; i < triangles.Length; i += 3)
            {
                var corner = vertices[triangles[i]];
                var a = vertices[triangles[i + 1]] - corner;
                var b = vertices[triangles[i + 2]] - corner;

                a.Scale(scale);
                b.Scale(scale);

                sum += Vector3.Cross(a, b).magnitude;
            }

            var surfaceSize = (float) (sum / 2.0);
            var side = Mathf.Sqrt(surfaceSize);

            return Vector2.one * Math.Max(1f, side) * 16f;
        }

        private static Rect ComputeUVBounds(Vector2[] uvs)
        {
            var xMin = uvs[0].x;
            var yMin = uvs[0].y;
            var xMax = uvs[0].x;
            var yMax = uvs[0].y;

            foreach (var uv in uvs)
            {
                if (uv.x > xMax)
                    xMax = uv.x;
                if (uv.y > yMax)
                    yMax = uv.y;
                if (uv.x < xMin)
                    xMin = uv.x;
                if (uv.y < yMin)
                    yMin = uv.y;
            }

            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        private static T[] Copy<T>(IReadOnlyList<T> list,
            int vertexCount,
            int newVertexCount,
            Dictionary<int, int> remapping)
        {
            if (list.Count == vertexCount)
            {
                var remappedList = new T[newVertexCount];
                foreach (var entry in remapping)
                    remappedList[entry.Value] = list[entry.Key];
                return remappedList;
            }

            if (list.Count != 0)
                Debug.LogWarning($"{typeof(T)}.Length != vertexCount");

            return new T[0];
        }


        private static Mesh RemapLightmapUVs(Mesh originalMesh, Dictionary<Mesh, List<AtlasMapEntry>> atlasMapEntries)
        {
            var freeSlots = atlasMapEntries[originalMesh];

            var freeSlot = freeSlots.First();
            freeSlots.RemoveAt(0);

            var copy = Object.Instantiate(originalMesh);

            var lightMapUVs = copy.uv2;
            var scale = new Vector2(freeSlot.UVRectangle.Width, freeSlot.UVRectangle.Height);
            var offset = new Vector2(freeSlot.UVRectangle.X, freeSlot.UVRectangle.Y);

            for (var i = 0; i < lightMapUVs.Length; i++)
                lightMapUVs[i] = lightMapUVs[i] * scale + offset;

            copy.uv2 = lightMapUVs;

            return copy;
        }

        public readonly struct MeshAndMatrix
        {
            public Mesh Mesh { get; }

            public Matrix4x4 Transform { get; }

            public GameObject GameObject { get; }

            public MeshAndMatrix(Mesh mesh, Matrix4x4 transform, GameObject gameObject)
            {
                Mesh = mesh;
                Transform = transform;
                GameObject = gameObject;
            }
        }
    }
}