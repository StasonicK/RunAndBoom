// #define UNITY_MODULE_VEHICLES // Comment in to support WheelCollider

using System;
using System.Collections.Generic;
using System.Linq;
using JustAssets.ColliderUtilityRuntime.Geometry;
using JustAssets.ColliderUtilityRuntime.MIConvexHull;
using JustAssets.ColliderUtilityRuntime.Optimization;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using Vertex = JustAssets.ColliderUtilityRuntime.MIConvexHull.Vertex;

namespace JustAssets.ColliderUtilityRuntime
{
    public static class ColliderUtility
    {
        [Obsolete("Use Create(GameObject, CollideCombineSettings) instead.")]
        public static Mesh Create(GameObject mainGameObject, bool recursive = true, bool finalConvexHull = false,
            int layersToConsider = 0,
            Func<Mesh, Mesh> saveMesh = null)
        {
            return Create(mainGameObject,
                new ColliderCombineSettings(recursive, finalConvexHull, layersToConsider, 0f, 1f, saveMesh));
        }

        public static Mesh Create(GameObject mainGameObject, ColliderCombineSettings colliderCombineSettings)
        {
            var colliders = GetColliders(mainGameObject, colliderCombineSettings.Recursive,
                colliderCombineSettings.LayersToConsider);
            var colliderCombine = GetCombineInstances(colliders);

            var decimateVertices = colliderCombineSettings.DecimateVertices;
            var normalOffsetCosine = Mathf.Cos(colliderCombineSettings.FaceNormalOffsetDegree / 180 * Mathf.PI);
            var buildMergedCollider = CreateNormalizedColliderMesh(mainGameObject.transform.worldToLocalMatrix, colliderCombine, decimateVertices, normalOffsetCosine);

            if (colliderCombineSettings.FinalConvexHull)
                buildMergedCollider = DecimateMesh(buildMergedCollider);

            buildMergedCollider.name = $"{mainGameObject.name}_Collider";
            return buildMergedCollider;
        }

        [Obsolete("Use CreateCollider(GameObject, ColliderCombineSettings, bool) instead.")]
        public static void CreateCollider(GameObject gameObject, bool recursive = true, bool finalConvexHull = false,
            int layersToConsider = 0,
            Func<Mesh, Mesh> saveMesh = null, bool disableColliders = false)
        {
            CreateCollider(gameObject,
                new ColliderCombineSettings(recursive, finalConvexHull, layersToConsider, 0f, 1f, saveMesh),
                disableColliders);
        }

        public static void CreateCollider(GameObject gameObject, ColliderCombineSettings colliderCombineSettings,
            bool disableColliders = false)
        {
            var mesh = Create(gameObject, colliderCombineSettings);

            mesh = SaveMesh(colliderCombineSettings.SaveMesh, mesh);

            if (disableColliders)
                DisableColliders(gameObject);

            var meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = false;
        }

        [Obsolete("Use CreateFilter(GameObject, ColliderCombineSettings, bool) instead.")]
        public static void CreateFilter(GameObject gameObject, bool recursive = true, bool finalConvexHull = false,
            int layersToConsider = 0,
            Func<Mesh, Mesh> saveMesh = null, bool createGameObject = false)
        {
            CreateFilter(gameObject,
                new ColliderCombineSettings(recursive, finalConvexHull, layersToConsider, 0f, 1f, saveMesh),
                createGameObject);
        }

        public static void CreateFilter(GameObject gameObject, ColliderCombineSettings colliderCombineSettings,
            bool createGameObject = false)
        {
            var mesh = Create(gameObject, colliderCombineSettings);

            mesh = SaveMesh(colliderCombineSettings.SaveMesh, mesh);

            if (createGameObject)
            {
                var newGameObject = new GameObject(gameObject.name + " (new)");
                newGameObject.transform.SetParent(gameObject.transform.parent);
                newGameObject.transform.localPosition = gameObject.transform.localPosition;
                newGameObject.transform.localRotation = gameObject.transform.localRotation;
                newGameObject.transform.localScale = gameObject.transform.localScale;

                gameObject = newGameObject;
            }

            var filter = gameObject.GetOrAddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            var renderer = gameObject.GetOrAddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] {new Material(Shader.Find("Diffuse"))};
        }

        public static void DisableColliders(GameObject gameObject)
        {
            var colliders = gameObject.GetComponentsInChildren<Collider>();
            foreach (var b in colliders)
                b.enabled = false;
        }

        public static void RemoveColliders(GameObject target, bool recursive = true)
        {
            var targetCloneColliders = GetColliders(target, recursive, ~0);
            foreach (var collider in targetCloneColliders)
                Object.DestroyImmediate(collider.Component);
        }

        private static Mesh CreateMesh(MeshCollider meshCollider)
        {
            var colliderSharedMesh = meshCollider.sharedMesh;
            if (colliderSharedMesh == null)
            {
                var meshFilter = meshCollider.gameObject.GetComponent<MeshFilter>();
                meshCollider.sharedMesh = meshFilter != null ? meshFilter.sharedMesh : null;
            }

            if (colliderSharedMesh == null)
                return null;

            if (!meshCollider.convex)
                return colliderSharedMesh;

            return DecimateMesh(colliderSharedMesh);
        }

        private static Mesh CreateMesh(Bounds bounds)
        {
            var mesh = new Mesh
            {
                vertices = new[]
                {
                    bounds.min, bounds.max,
                    new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                    new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                    new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                    new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
                    new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                    new Vector3(bounds.max.x, bounds.max.y, bounds.min.z)
                },
                triangles = new[]
                {
                    0, 7, 4, 0, 3, 7, 5, 1, 3, 3, 1, 7, 7, 1, 4, 4, 1, 6, 5, 3, 2, 2, 3, 0, 0, 4, 2, 2, 4, 6, 1,
                    5, 2, 6, 1, 2
                }
            };
            return mesh;
        }

        private static Mesh CreateMesh(TerrainCollider terrainCollider)
        {
            var terrainData = terrainCollider.terrainData;

            if (terrainData == null)
                return null;

            var heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

            return TerrainColliderGenerator.Create(terrainData.heightmapResolution, terrainData.heightmapResolution,
                heights, terrainData.size, Vector3.zero);
        }

#if !UNITY_2018_1_OR_NEWER || UNITY_MODULE_VEHICLES
        private static Mesh CreateMesh(WheelCollider wheelCollider)
        {
            var radius = wheelCollider.radius;
            var height = 0.5f;

            var rotation = Quaternion.Euler(0, 0, 90);
            var center =
                Matrix4x4.Translate(wheelCollider.center - new Vector3(0, wheelCollider.suspensionDistance / 2, 0));

            var cylinderRotation = Matrix4x4.Rotate(Quaternion.Euler(0, 30, 0));
            var body = new CombineInstance
            {
                mesh = CylinderGenerator.Create(radius, height, 12, 1),
                transform = center * Matrix4x4.Rotate(rotation) * cylinderRotation *
                            Matrix4x4.Translate(-Vector3.up * height / 2)
            };

            var mesh = new Mesh();
            mesh.CombineMeshes(new[] {body});
            return mesh;
        }
#endif

        private static Mesh CreateMesh(SphereCollider sphereCollider)
        {
            var recursionLevel = 1;
            var radius = sphereCollider.radius;

            var center = Matrix4x4.Translate(sphereCollider.center);

            var sphere = new CombineInstance
            {
                mesh = SphereGenerator.Create(radius, recursionLevel),
                transform = center
            };

            var mesh = new Mesh();
            mesh.CombineMeshes(new[] {sphere});
            return mesh;
        }

        private static Mesh CreateMesh(CapsuleCollider capsuleCollider)
        {
            var recursionLevel = 0;
            var radius = capsuleCollider.radius;
            var height = capsuleCollider.height;

            var rotation = capsuleCollider.direction == 1 ? Quaternion.identity :
                capsuleCollider.direction == 0 ? Quaternion.Euler(0, 0, 90) : Quaternion.Euler(90, 0, 0);

            var center = Matrix4x4.Translate(capsuleCollider.center);

            var top = new CombineInstance
            {
                mesh = SphereGenerator.Create(radius, recursionLevel),
                transform = center * Matrix4x4.Rotate(rotation) *
                            Matrix4x4.TRS(Vector3.up * (height / 2 - radius), Quaternion.identity, Vector3.one)
            };
            var bottom = new CombineInstance
            {
                mesh = SphereGenerator.Create(radius, recursionLevel),
                transform = center * Matrix4x4.Rotate(rotation) *
                            Matrix4x4.TRS(Vector3.up * -(height / 2 - radius), Quaternion.identity, Vector3.one)
            };
            var bodyHeight = height - 2 * radius;
            var cylinderRotation = Matrix4x4.Rotate(Quaternion.Euler(0, 30, 0));
            var body = new CombineInstance
            {
                mesh = CylinderGenerator.Create(radius, bodyHeight, 6, 1, false),
                transform = center * Matrix4x4.Rotate(rotation) * cylinderRotation *
                            Matrix4x4.Translate(-Vector3.up * bodyHeight / 2)
            };

            var mesh = new Mesh();
            mesh.CombineMeshes(new[] {top, body, bottom});
            return mesh;
        }

        private static Mesh CreateMesh(BoxCollider collider)
        {
            var bounds = new Bounds(Vector3.zero, collider.size);

            var center = Matrix4x4.Translate(collider.center);
            var body = new CombineInstance
            {
                mesh = CreateMesh(bounds),
                transform = center
            };

            var mesh = new Mesh();
            mesh.CombineMeshes(new[] {body});
            return mesh;
        }

        private static Mesh CreateNormalizedColliderMesh(Matrix4x4 targetRotationMatrix,
            List<CombineInstance> colliderCombine,
            float decimateVertices,
            float faceNormalOffsetCosine)
        {
            var colliderMesh = new Mesh();

            colliderMesh.indexFormat = IndexFormat.UInt32;
            colliderMesh.CombineMeshes(colliderCombine.ToArray());

            CombineInstance[] clearedRotationMesh =
            {
                new CombineInstance
                {
                    mesh = colliderMesh, transform = targetRotationMatrix
                }
            };

            var finalColliderMesh = new Mesh {name = "Collider"};
            finalColliderMesh.CombineMeshes(clearedRotationMesh);

            if (decimateVertices > 0f)
            {
                var graph = new Graph(finalColliderMesh, null, decimateVertices, faceNormalOffsetCosine);
                graph.DecimateEdges();
                graph.DecimateVertices();
                finalColliderMesh = graph.ToMesh();
            }

            return finalColliderMesh;
        }

        private static Mesh DecimateMesh(Mesh meshToDecimate)
        {
            var mesh = new Mesh();
            var triangles = new List<int>();
            var vertices = meshToDecimate.vertices.Select(x => new Vertex(x)).ToList();

            var result = ConvexHullBuilder.Create(vertices);

            if (result.Result == null)
                return mesh;

            mesh.vertices = result.Result.Points.Select(x => x.ToVec()).ToArray();
            var list = result.Result.Points.ToList();

            foreach (var face in result.Result.Faces)
            {
                triangles.Add(list.IndexOf(face.Vertices[0]));
                triangles.Add(list.IndexOf(face.Vertices[1]));
                triangles.Add(list.IndexOf(face.Vertices[2]));
            }

            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }

        private static Mesh GetColliderMesh(Collider collider)
        {
            if (collider.isTrigger)
                return null;

            var meshCollider = collider as MeshCollider;
            if (meshCollider != null)
            {
                if (meshCollider.sharedMesh == null)
                    UnityEngine.Debug.LogWarning($"Mesh collider misses mesh on {meshCollider.name}, setting mesh filter mesh.",
                        meshCollider);

                return CreateMesh(meshCollider);
            }

            var boxCollider = collider as BoxCollider;
            if (boxCollider != null)
                return CreateMesh(boxCollider);

            var capsuleCollider = collider as CapsuleCollider;
            if (capsuleCollider != null)
                return CreateMesh(capsuleCollider);

            var sphereCollider = collider as SphereCollider;
            if (sphereCollider != null)
                return CreateMesh(sphereCollider);

#if !UNITY_2018_1_OR_NEWER || UNITY_MODULE_VEHICLES
            var wheelCollider = collider as WheelCollider;
            if (wheelCollider != null)
                return CreateMesh(wheelCollider);
#endif

            var terrainCollider = collider as TerrainCollider;
            if (terrainCollider != null)
                return CreateMesh(terrainCollider);

            var colliderMeshes = CreateMesh(collider.bounds);
            UnityEngine.Debug.LogWarning(
                string.Format("Cannot merge collider of type {0} of {1}, assuming it to be box.", collider.GetType(),
                    collider.gameObject.name),
                collider.gameObject);
            return colliderMeshes;
        }

        private static List<TransformedComponent<Collider>> GetColliders(GameObject mainGameObject, bool recursive,
            int layersToConsider)
        {
            return GetColliders(mainGameObject, recursive, mainGameObject.transform.localToWorldMatrix, layersToConsider);
        }

        private static List<TransformedComponent<Collider>> GetColliders(GameObject mainGameObject, bool recursive,
            Matrix4x4 currentTransform,
            int layersToConsider)
        {
            var result = new List<TransformedComponent<Collider>>();
            var layer = 1 << mainGameObject.layer;
            if ((layersToConsider & layer) == layer)
                result.AddRange(mainGameObject.GetComponents<Collider>()
                    .Select(x => new TransformedComponent<Collider>(x, currentTransform)));

            if (!recursive)
                return result;

            foreach (Transform child in mainGameObject.transform)
                result.AddRange(GetColliders(child.gameObject, true,
                    currentTransform * Matrix4x4.TRS(child.localPosition, child.localRotation, child.localScale),
                    layersToConsider));

            return result;
        }

        private static List<CombineInstance> GetCombineInstances(List<TransformedComponent<Collider>> gameObjects)
        {
            var colliderCombine = new List<CombineInstance>();

            foreach (var obj in gameObjects)
            {
                var colliderMesh = GetColliderMesh(obj.Component);
                if (colliderMesh == null)
                    continue;
                
                colliderMesh.name = obj.Component.name;
                colliderCombine.Add(new CombineInstance
                {
                    mesh = colliderMesh,
                    transform = obj.Transformation
                });
            }

            return colliderCombine;
        }

        private static Mesh SaveMesh(Func<Mesh, Mesh> saveMesh, Mesh mesh)
        {
            try
            {
                Mesh savedMesh = null;
                if (saveMesh != null)
                    savedMesh = saveMesh.Invoke(mesh);

                if (savedMesh == null)
                    savedMesh = mesh;
                return savedMesh;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(string.Format("Could not save mesh: {0}.", e));
            }

            return mesh;
        }
    }
}