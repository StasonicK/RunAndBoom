using System;
using System.IO;
using JustAssets.AtlasMapPacker.AtlasMapping;
using UnityEditor;
using UnityEngine;

namespace JustAssets.AtlasMapPacker
{
    public sealed class ChannelSwapperWindow : EditorWindow
    {
    
        private static ChannelSwapperWindow _window;

        private Texture2D _textureToModify;

        private Channel _channelR;

        private Channel _channelG;

        private Channel _channelB;

        private Channel _channelA;

        private string _assetPath;

        private TextureType _textureType;

        [MenuItem("Tools/Mesh Draw Call Optimizer/Channel Swapper...")]
        private static void Init()
        {
            FindWindow();
            _window.minSize = new Vector2(400f, 200f);
            _window.Show();
        }

        private static void FindWindow()
        {
            _window = (ChannelSwapperWindow) GetWindow(typeof(ChannelSwapperWindow), false, "Channel Swapper");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            var textureToModify = (Texture2D) EditorGUILayout.ObjectField(_textureToModify, typeof(Texture2D), false);
            EditorGUILayout.LabelField("Path:", _assetPath);
            EditorGUILayout.LabelField("Type:", _textureType.ToString());

            if (textureToModify != _textureToModify)
            {
                _textureToModify = textureToModify;
                _assetPath = AssetDatabase.GetAssetPath(_textureToModify);
                _textureType = GetTextureType(_assetPath);
            }
        
            _channelR = (Channel) EditorGUILayout.EnumPopup("Red", _channelR);
            _channelG = (Channel) EditorGUILayout.EnumPopup("Green", _channelG);
            _channelB = (Channel) EditorGUILayout.EnumPopup("Blue", _channelB);
            _channelA = (Channel) EditorGUILayout.EnumPopup("Alpha", _channelA);

            EditorGUILayout.EndVertical();

            GUI.enabled = _textureType != TextureType.Unsupported;
            if (GUILayout.Button("Apply"))
            {
                var newTex = _textureToModify.ReorderChannels(_channelR, _channelG, _channelB, _channelA);
                SaveTexture(newTex);
            }

            GUI.enabled = true;
        }

        private void SaveTexture(Texture2D newTex)
        {
            byte[] data;
            switch (_textureType)
            {
                case TextureType.PNG:
                    data = newTex.EncodeToPNG();
                    break;
                case TextureType.JPG:
                    data = newTex.EncodeToJPG();
                    break;
                case TextureType.Targa:
                    data = newTex.EncodeToTGA();
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