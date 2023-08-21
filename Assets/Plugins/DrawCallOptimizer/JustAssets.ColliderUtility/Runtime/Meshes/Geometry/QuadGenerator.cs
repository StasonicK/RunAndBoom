using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace JustAssets.ColliderUtilityRuntime.Geometry
{
    public class QuadGenerator
    {
        public static GameObject CreatePlaneOfTries(int width, int height, out Mesh colliderMesh,
            IEnumerable<CombineInstance> additonalData = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);

            var combineInstances = new List<CombineInstance>();
            if (additonalData != null)
                combineInstances.AddRange(additonalData);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var combineInstance = CreateQuad(go, x, y);
                    combineInstances.Add(combineInstance);
                }
            }

            colliderMesh = new Mesh
            {
                indexFormat = IndexFormat.UInt32
            };
            colliderMesh.CombineMeshes(combineInstances.ToArray());
            return go;
        }

        public static CombineInstance CreateQuad(GameObject go, int x, int y)
        {
            var combineInstance = new CombineInstance
            {
                mesh = go.GetComponent<MeshFilter>().sharedMesh,
                transform = Matrix4x4.TRS(Vector3.right * (0.5f + x) + Vector3.up * (0.5f + y),
                    Quaternion.identity, Vector3.one)
            };
            return combineInstance;
        }
    }
}