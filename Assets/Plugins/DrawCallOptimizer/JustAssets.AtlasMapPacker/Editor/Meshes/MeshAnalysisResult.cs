using UnityEngine;

namespace JustAssets.AtlasMapPacker.Meshes
{
    internal class MeshAnalysisResult
    {
        public MeshAnalysisResult(string message, Mesh sender, UVSolution solution)
        {
            Message = message;
            Sender = sender;
            Solution = solution;
        }

        public string Message { get; }

        public Mesh Sender { get; }

        public UVSolution Solution { get; }
    }
}