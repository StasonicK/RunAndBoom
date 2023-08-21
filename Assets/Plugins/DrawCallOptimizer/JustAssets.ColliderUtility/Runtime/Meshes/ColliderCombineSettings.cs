using System;
using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime
{
    public class ColliderCombineSettings
    {
        public bool Recursive { get; }

        public bool FinalConvexHull { get; }

        public int LayersToConsider { get; }

        public float DecimateVertices { get; }

        public Func<Mesh, Mesh> SaveMesh { get; }
        
        public float FaceNormalOffsetDegree { get; }

        public ColliderCombineSettings(
            bool recursive = true,
            bool finalConvexHull = false,
            int layersToConsider = 0,
            float decimateVertices = 0f,
            float faceNormalOffsetDegree = 1,
            Func<Mesh, Mesh> saveMesh = null)
        {
            FaceNormalOffsetDegree = faceNormalOffsetDegree;
            Recursive = recursive;
            FinalConvexHull = finalConvexHull;
            LayersToConsider = layersToConsider;
            DecimateVertices = decimateVertices;
            SaveMesh = saveMesh;
        }
    }
}