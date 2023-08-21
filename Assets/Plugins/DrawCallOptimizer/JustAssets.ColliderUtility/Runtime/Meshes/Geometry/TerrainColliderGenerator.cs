using System;
using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime.Geometry
{
    public static class TerrainColliderGenerator
    {
        public static Mesh Create(TerrainCollider terrainCollider)
        {
            throw new NotImplementedException();
        }

        public static Mesh Create(int heightmapWidth, int heightmapHeight, float[,] heights, Vector3 terrainSize,
            Vector3 terrainPos, int sampleResolution = 2)
        {
            int w = heightmapWidth;
            int h = heightmapHeight;

            Vector3 meshScale = terrainSize;
            int tRes = (int) Mathf.Pow(2, sampleResolution);
            meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
            Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
            float[,] tData = heights;

            w = (w - 1) / tRes + 1;
            h = (h - 1) / tRes + 1;
            Vector3[] tVertices = new Vector3[w * h];
            Vector2[] tUV = new Vector2[w * h];

            int[] tPolys = new int[(w - 1) * (h - 1) * 6];

            // Build vertices and UVs
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                tVertices[y * w + x] =
                    Vector3.Scale(meshScale, new Vector3(y, tData[x * tRes, y * tRes], x)) + terrainPos;
                tUV[y * w + x] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
            }

            int index = 0;

            // Build triangle indices: 3 indices into vertex array for each triangle
            for (int y = 0; y < h - 1; y++)
            for (int x = 0; x < w - 1; x++)
            {
                // For each grid cell output two triangles
                tPolys[index++] = y * w + x + 1;
                tPolys[index++] = (y + 1) * w + x + 1;
                tPolys[index++] = (y + 1) * w + x;

                tPolys[index++] = y * w + x + 1;
                tPolys[index++] = (y + 1) * w + x;
                tPolys[index++] = y * w + x;
            }

            Mesh mesh = new Mesh {vertices = tVertices, triangles = tPolys, uv = tUV};

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            //return mesh;

            MeshSimplifier.MeshSimplifier meshSimplifier = new MeshSimplifier.MeshSimplifier(mesh) {PreserveBorderEdges = true};
            meshSimplifier.SimplifyMesh(0.05f);
            Mesh destMesh = meshSimplifier.ToMesh();

            return destMesh;
        }
    }
}