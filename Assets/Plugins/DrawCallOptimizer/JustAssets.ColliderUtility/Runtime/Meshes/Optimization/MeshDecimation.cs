using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime.Optimization
{
    public class MeshDecimation
    {
        public static List<CombineInstance> DecimateFaces(List<CombineInstance> instances)
        {
            var meshes = instances.Select(x =>
            {
                var mesh = new Mesh {name = x.mesh.name};
                mesh.CombineMeshes(new[] {x});
                return mesh;
            }).ToArray();

            var graphs = meshes.Select(x => new Graph(x, null)).ToList();

            foreach (var graph in graphs)
            {
                graph.DecimateEdges();
                graph.DecimateVertices();
            }

            meshes = graphs.Select(x => x.ToMesh()).ToArray();
            return meshes.Select(x => new CombineInstance {mesh = x, transform = Matrix4x4.identity}).ToList();
        }
    }
}