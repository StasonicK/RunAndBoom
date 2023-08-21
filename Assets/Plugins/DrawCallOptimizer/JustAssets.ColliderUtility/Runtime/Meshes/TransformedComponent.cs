using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime
{
    internal class TransformedComponent<T> where T : Component
    {
        public T Component { get; }

        public Matrix4x4 Transformation { get; }

        public TransformedComponent(T component, Matrix4x4 transformation)
        {
            Component = component;
            Transformation = transformation;
        }
    }
}