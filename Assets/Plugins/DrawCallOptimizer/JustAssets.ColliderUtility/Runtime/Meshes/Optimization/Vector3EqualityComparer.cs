using System;
using System.Collections.Generic;
using UnityEngine;

namespace JustAssets.ColliderUtilityRuntime.Optimization
{
    /// <summary>
    /// Use this class to compare two Vector3 objects for equality with NUnit constraints. Call Vector3EqualityComparer.Instance comparer to perform a comparison with the default calculation error value 0.0001f. To specify a different error value, use the one argument constructor to instantiate a new comparer.
    /// </summary>
    public class Vector3EqualityComparer : IEqualityComparer<Vector3>
    {
        private readonly float _allowedError;

        /// <summary>
        /// A comparer instance with the default calculation error value equal to 0.0001f.
        ///</summary>
        public static Vector3EqualityComparer Default { get; } = new Vector3EqualityComparer();
        
        /// <summary>
        /// Initializes an instance of Vector3Equality comparer with custom allowed calculation error.
        /// </summary>
        /// <param name="allowedError">This value identifies the calculation error allowed.</param>
        public Vector3EqualityComparer(float allowedError = 0.0001f)
        {
            _allowedError = allowedError;
        }

        public bool Equals(Vector3 expected, Vector3 actual)
        {
            return Math.Abs(actual.x - expected.x) <= _allowedError &&
                Math.Abs(actual.y - expected.y) <= _allowedError &&
                Math.Abs(actual.z - expected.z) <= _allowedError;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <param name="vec3">A not null Vector3</param>
        /// <returns>Returns 0</returns>
        public int GetHashCode(Vector3 vec3)
        {
            return 0;
        }
    }
}