using UnityEngine;

namespace JustAssets.AtlasMapPacker.Meshes
{
    internal readonly struct MeshOwner
    {
        public MeshOwner(GameObject gameObject, Mesh sharedMesh)
        {
            GameObject = gameObject;
            SharedMesh = sharedMesh;
        }

        public GameObject GameObject { get; }

        public Mesh SharedMesh { get; }
    }
}