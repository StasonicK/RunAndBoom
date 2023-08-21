using System.Collections.Generic;
using System.IO;
using JustAssets.AtlasMapPacker.AtlasMapping;
using JustAssets.AtlasMapPacker.AtlasMapping.Resampling;
using UnityEditor;
using UnityEngine;

namespace JustAssets.AtlasMapPacker
{
    public sealed class ImageReader : EditorWindow
    {
        private static ImageReader _window;

        private Texture2D _atlasTexture;

        private ResamplingFilters _downScalingFilter = ResamplingFilters.Lanczos3;

        private Texture2D _readTexture;

        private Texture _savedFile;

        private Texture2D _scaledTexture;

        private Texture2D _textureToRead;

        private static void FindWindow()
        {
            _window = (ImageReader) GetWindow(typeof(ImageReader), false, "Image Reader");
        }

        [MenuItem("Tools/Mesh Draw Call Optimizer/Image Reader...")]
        private static void Init()
        {
            FindWindow();
            _window.minSize = new Vector2(400f, 200f);
            _window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            var downScalingFilter = (ResamplingFilters) EditorGUILayout.EnumPopup("Downscaling filter:", _downScalingFilter);

            var textureToModify = (Texture2D) EditorGUILayout.ObjectField(_textureToRead, typeof(Texture2D), false);

            if (textureToModify != _textureToRead || downScalingFilter != _downScalingFilter)
            {
                _textureToRead = textureToModify;
                _downScalingFilter = downScalingFilter;

                _readTexture = _textureToRead != null ? ImageManipulation.ReadTexture(_textureToRead) : null;
                _scaledTexture = _readTexture != null
                    ? ImageManipulation.Scale(_readTexture, _readTexture.width / 2, _readTexture.height / 2, downScalingFilter: _downScalingFilter)
                    : null;
                var atlasMapEntries = new List<AtlasMapEntry>
                    {new AtlasMapEntry(new PixelRect(5, 5, 200, 200), _readTexture), new AtlasMapEntry(new PixelRect(210, 5, 200, 200), _readTexture)};
                _atlasTexture = AtlasMapWriter.CreateAtlasTexture(atlasMapEntries, new Vector2Int(512, 512), 4, entry => entry.Payload<Texture2D>());
                if (_textureToRead != null)
                {
                    MemoryStream stream = AtlasMapWriter.SaveAtlas(atlasMapEntries, new Vector2Int(512, 512), 4, entry => entry.Payload<Texture2D>());
                    _savedFile = ImageFileIO.SaveAtlasTexture(stream, "Assets", "TEMP", "temp.png", _textureToRead.name, true);
                }
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(_readTexture, typeof(Texture2D), true, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.ObjectField(_scaledTexture, typeof(Texture2D), true, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(_atlasTexture, typeof(Texture2D), true, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.ObjectField(_savedFile, typeof(Texture), true, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}