using System.Linq;
using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime
{
    public static class UnityExtension
    {
        public static T GetOrAddComponent<T>(this GameObject owner) where T : Component
        {
            T component = owner.GetComponent<T>();
            if (component != null)
                return component;

            return owner.AddComponent<T>();
        }

        public static System.Numerics.Vector3[] ToNumeric(this Vector3[] vectorArray)
        {
            return vectorArray.Select(x => new System.Numerics.Vector3(x.x, x.y, x.z)).ToArray();
        }

        public static Vector3[] ToUnity(this System.Numerics.Vector3[] vectorArray)
        {
            return vectorArray.Select(x => new Vector3(x.X, x.Y, x.Z)).ToArray();
        }
    }
}