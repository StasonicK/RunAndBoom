using System;
using System.IO;
using JustAssets.AtlasMapPacker.AtlasMapping.Resampling;
using UnityEditor;
using UnityEngine;

namespace JustAssets.AtlasMapPacker
{
    public sealed class ImageScalerWindow : EditorWindow
    {
    
        private static ImageScalerWindow window;

        [SerializeField]
        private Texture2D _textureToModify;

        [SerializeField]
        private string _assetPath;

        [SerializeField]
        private TextureType _textureType;

        [SerializeField]
        private int _newSizeY = 512;

        [SerializeField]
        private int _newSizeX = 512;

        private ResamplingFilters _algorithm = ResamplingFilters.Lanczos3;

        [MenuItem("Tools/Mesh Draw Call Optimizer/Image scaler...")]
        private static void Init()
        {
            FindWindow();
            window.minSize = new Vector2(400f, 200f);
            window.Show();
        }

        private static void FindWindow()
        {
            window = (ImageScalerWindow) GetWindow(typeof(ImageScalerWindow), false, "Image Scaler");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            var textureToModify = (Texture2D) EditorGUILayout.ObjectField(_textureToModify, typeof(Texture2D), false);
            EditorGUILayout.LabelField("Path:", _assetPath);
            EditorGUILayout.LabelField("Type:", _textureType.ToString());
            _algorithm = (ResamplingFilters) EditorGUILayout.EnumPopup("Filter:", _algorithm);

            if (textureToModify != _textureToModify)
            {
                _textureToModify = textureToModify;
                _assetPath = AssetDatabase.GetAssetPath(_textureToModify);
                _textureType = GetTextureType(_assetPath);
            }
        
            _newSizeX = EditorGUILayout.IntField("New Height", _newSizeX);
            _newSizeY = EditorGUILayout.IntField("New Width", _newSizeY);

            EditorGUILayout.EndVertical();

            GUI.enabled = _textureType != TextureType.Unsupported;
            if (GUILayout.Button("Apply"))
            {
                var newTexture = ImageResampling.ApplyFilter(_textureToModify, _newSizeX, _newSizeY, _algorithm);
                SaveTexture(newTexture);
            }

            GUI.enabled = true;
        }

        private void SaveTexture(Texture2D newTexture)
        {
            byte[] data;
            switch (_textureType)
            {
                case TextureType.PNG:
                    data = newTexture.EncodeToPNG();
                    break;
                case TextureType.JPG:
                    data = newTexture.EncodeToJPG();
                    break;
                case TextureType.Targa:
                    data = newTexture.EncodeToTGA();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var fullPath = Path.GetFullPath(_assetPath);
            File.WriteAllBytes(fullPath, data);
            AssetDatabase.ImportAsset(_assetPath);
        }

        private static TextureType GetTextureType(string assetPath)
        {
            switch (Path.GetExtension(assetPath).ToLower())
            {
                case ".png":
                    return TextureType.PNG;
                case ".jpg":
                case ".jpeg":
                    return TextureType.JPG;
                case ".tga":
                    return TextureType.Targa;
                default:
                    return TextureType.Unsupported;
            }
        }

        private enum TextureType
        {
            PNG,

            JPG,

            Targa,

            Unsupported
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}