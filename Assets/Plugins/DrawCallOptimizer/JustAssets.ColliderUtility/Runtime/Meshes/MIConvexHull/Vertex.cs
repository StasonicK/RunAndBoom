using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime.MIConvexHull
{
    public class Vertex : IVertex
    {
        public double[] Position { get; set; }

        public Vertex (Vector3 ver)
        {
            Position = new double[3] { ver.x, ver.y, ver.z };
        }
        public Vector3 ToVec()
        {
            return new Vector3((float)Position[0], (float)Position[1], (float)Position[2]);
        }
    }
}