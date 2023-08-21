#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering;
#else
using ShaderPropertyType = UnityEditor.ShaderUtil.ShaderPropertyType;
#endif
using System;
using System.Collections.Generic;
using JustAssets.AtlasMapPacker.AtlasMapping;
using UnityEditor;
using UnityEngine;

namespace JustAssets.AtlasMapPacker
{
    public static class UnityExtension
    {
        /// <summary>Compares the current object with another object of the same type.</summary>
        /// <returns>
        ///     A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has
        ///     the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other" />
        ///     parameter.Zero This object is equal to <paramref name="other" />. Greater than zero This object is greater than
        ///     <paramref name="other" />.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public static int CompareTo(this Rect that, Rect other)
        {
            const double TOLERANCE = 0.0001f;
            if (Math.Abs(that.x - other.x) < TOLERANCE)
            {
                if (Math.Abs(that.y - other.y) < TOLERANCE)
                {
                    if (Math.Abs(that.height - other.height) < TOLERANCE)
                        return that.width.CompareTo(other.width);
                    return that.height.CompareTo(other.height);
                }

                return that.y.CompareTo(other.y);
            }

            return that.x.CompareTo(other.x);
        }

        public static int GetContentHash<T>(this ICollection<T> that)
        {
            unchecked
            {
                int hash = 19;
                foreach (var mesh in that)
                {
                    hash = hash * 31 + mesh.GetHashCode();
                }
                return hash;
            }
        }

        public static bool IntersectsWith(this Rect that, Rect other)
        {
            return other.x < that.x + that.width &&
                   that.x < other.x + other.width &&
                   other.y < that.y + that.height &&
                   that.y < other.y + other.height;
        }

        public static bool Contains(this Rect that, Rect other)
        {
            return that.x <= other.x &&
                   other.x + other.width <= that.x + that.width &&
                   that.y <= other.y &&
                   other.y + other.height <= that.y + that.height;
        }

        public static ShaderPropertyType GetPropertyType(this UnityEngine.Shader shader, string name)
        {
            var index = GetPropertyIndexByName(shader, name);

            return index >= 0 ? GetShaderPropertyType(shader, index) : (ShaderPropertyType) (-1);
        }

    
        public static ShaderProperty[] GetProperties(this UnityEngine.Shader shader)
        {
            if (shader == null)
                return new ShaderProperty[0];

            var result = new ShaderProperty[GetShaderPropertyCount(shader)];
            for (var i = 0; i < GetShaderPropertyCount(shader); i++)
            {
                var name = GetShaderPropertyName(shader, i);
                var type = GetShaderPropertyType(shader, i);

                result[i] = new ShaderProperty(i, name, type);
            }

            return result;
        }

        public static ShaderPropertyType GetShaderPropertyType(this UnityEngine.Shader shader, int i)
        {
#if UNITY_2019_3_OR_NEWER
            return shader.GetPropertyType(i);
#else
            return ShaderUtil.GetPropertyType(shader, i);
#endif
        }

        public static string GetShaderPropertyName(this UnityEngine.Shader shader, int i)
        {
#if UNITY_2019_3_OR_NEWER
            return shader.GetPropertyName(i);
#else
            return ShaderUtil.GetPropertyName(shader, i);
#endif
        }

        public static int GetShaderPropertyCount(this UnityEngine.Shader shader)
        {
#if UNITY_2019_3_OR_NEWER
            return shader.GetPropertyCount();
#else
            return ShaderUtil.GetPropertyCount(shader);
#endif
        }

        public static string GetShaderPropertyDefault(this UnityEngine.Shader shader, int propertyIndexByName)
        {
#if UNITY_2019_3_OR_NEWER
            return shader.GetPropertyTextureDefaultName(propertyIndexByName);
#else
            return string.Empty;
#endif
        }


        public readonly struct ShaderProperty
        {
            public int Id { get; }

            public string Name { get; }

            public ShaderPropertyType Type { get; }

            public ShaderProperty(int id, string name, ShaderPropertyType type)
            {
                Id = id;
                Name = name;
                Type = type;
            }
        }

        public static int GetPropertyIndexByName(this UnityEngine.Shader shader, string name)
        {
            if (shader == null)
                return -1;

            var index = -1;
            for (var i = 0; i < GetShaderPropertyCount(shader); i++)
            {
                var currentName = GetShaderPropertyName(shader, i);

                if (currentName == name)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        public static DefaultAsset CreateFolderRecursive(string fullPath)
        {
            var subPath = "";

            var folders = fullPath.Split('/');

            string lastGUID = null;
            string parentPath = "";
            foreach (var folder in folders)
            {
                subPath += folder;
                if (AssetDatabase.LoadAssetAtPath<DefaultAsset>(subPath) == null)
                    lastGUID = AssetDatabase.CreateFolder(parentPath, folder);
                parentPath = subPath;
                subPath += "/";
            }

            if (!String.IsNullOrEmpty(lastGUID))
                return AssetDatabase.LoadAssetAtPath<DefaultAsset>(AssetDatabase.GUIDToAssetPath(lastGUID));

            return null;
        }
    }
}