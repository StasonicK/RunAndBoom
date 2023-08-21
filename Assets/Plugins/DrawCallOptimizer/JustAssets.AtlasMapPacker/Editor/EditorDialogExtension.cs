using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JustAssets.AtlasMapPacker
{
    internal class EditorDialogExtension
    {
        private static readonly Dictionary<ErrorCause, string> errors = new Dictionary<ErrorCause, string>
        {
            {
                ErrorCause.AtlasSizeTooLow,
                "The texture size is too low to layout the textures on it. Try reducing the margin or increase the texture size. When using very small textures you should try to switch the strategy to Scale-Up."
            }
        };

        private static readonly GUIContent MaxSize =
            EditorGUIUtility.TrTextContent("Max Texture Size", "Textures larger than this will be scaled down.");

        private static readonly GUIContent MinSize =
            EditorGUIUtility.TrTextContent("Min Texture Size", "Undefined textures will receive this size.");

        private static readonly string[] MaxTextureSizeStrings =
        {
            "32",
            "64",
            "128",
            "256",
            "512",
            "1024",
            "2048",
            "4096",
            "8192",
            "16384"
        };

        private static readonly int[] MaxTextureSizeValues =
        {
            32,
            64,
            128,
            256,
            512,
            1024,
            2048,
            4096,
            8192,
            16384
        };
        
        private static readonly string[] MinTextureSizeStrings =
        {
            "2",
            "4",
            "8",
            "16",
            "32",
        };

        private static readonly int[] MinTextureSizeValues =
        {
            2,
            4,
            8,
            16,
            32,
        };

        public static void ShowErrorDialog(ErrorCause errorCause)
        {
            EditorUtility.DisplayDialog("Error", errors[errorCause], "Okay");
        }

        public static int DrawTextureSizeDropDown(int currentSize)
        {
            return EditorGUILayout.IntPopup(MaxSize.text, currentSize, MaxTextureSizeStrings, MaxTextureSizeValues);
        }
        
        public static int DrawMinTextureSizeDropDown(int currentSize)
        {
            return EditorGUILayout.IntPopup(MinSize.text, currentSize, MinTextureSizeStrings, MinTextureSizeValues);
        }

        internal enum ErrorCause
        {
            AtlasSizeTooLow
        }
    }
}