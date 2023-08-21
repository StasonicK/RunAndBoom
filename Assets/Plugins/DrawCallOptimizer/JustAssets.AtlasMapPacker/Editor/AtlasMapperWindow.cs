using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using JustAssets.AtlasMapPacker.AtlasMapping;
using JustAssets.ColliderUtilityEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JustAssets.AtlasMapPacker
{
    public sealed class AtlasMapperWindow : EditorWindow
    {
        private static AtlasMapperWindow window;
        
        private readonly Version _version = new Version(2, 6, 3);
        
        [SerializeField] 
        private readonly GUIData _data = new GUIData();

        private AllInOneGUI _allInOneGUI;
        private AtlasGUI _atlasGUI;
        private int _clickCounter;
        private CombineGUI _combineGUI;

        
        private readonly List<string> _menuNames = new List<string>
        {
            "Main",
            "Atlas Baker",
            "Mesh Combiner",
            "Shader Mapper",
            "Help"
        };

        private DefaultAsset _saveFolder;

        private ShaderMappingGUI _shaderMappingGUI;
        private Styles _styles;

        public DefaultAsset SaveFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_data.SaveFolderPath) && _saveFolder != null)
                    _data.SaveFolderPath = AssetDatabase.GetAssetPath(_saveFolder);

                if (!string.IsNullOrEmpty(_data.SaveFolderPath) && _saveFolder == null)
                    return _saveFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(_data.SaveFolderPath);

                return _saveFolder;
            }
            private set
            {
                if (value == _saveFolder)
                    return;

                _saveFolder = value;
                _data.SaveFolderPath = AssetDatabase.GetAssetPath(value);
            }
        }
        
        private static void DrawAtlasMap(List<AtlasMapEntry> atlasMapEntries, Rect controlRect, Mesh selection)
        {
            EditorGUI.DrawRect(controlRect, Color.green);
            controlRect.size -= Vector2.one * 4;
            controlRect.position += Vector2.one * 2;
            EditorGUI.DrawRect(controlRect, Color.gray);

            controlRect.size -= Vector2.one * 2;
            controlRect.position += Vector2.one * 1;

            foreach (var atlasMapEntry in atlasMapEntries)
            {
                var uvRectangle = atlasMapEntry.UVRectangle;
                var entryRect = new Rect(controlRect.position.x + controlRect.width * uvRectangle.X,
                    controlRect.position.y + controlRect.height * uvRectangle.Y, controlRect.width * uvRectangle.Width,
                    controlRect.height * uvRectangle.Height);

                EditorGUI.DrawRect(entryRect, Color.green);
                entryRect.size -= Vector2.one * 2;
                entryRect.position += Vector2.one;

                var color = Color.gray;
                if (atlasMapEntry.Payload<object>() is Mesh mesh && mesh == selection)
                    color = Color.red;
                EditorGUI.DrawRect(entryRect, color);
            }
        }

        [MenuItem("Tools/Mesh Draw Call Optimizer/Open... %#O")]
        private static void Init()
        {
            FindWindow();
            window.minSize = new Vector2(400f, 200f);
            window.Show();
        }

        private void OnEnable()
        {
            PipelineTool.IsPipelineSupported(isSupported =>
            {
                if (!isSupported)
                    EditorUtility.DisplayDialog("Other pipeline shaders",
                        "Using the Universal/HD Rendering Pipeline int this Unity version is not supported by the optimizer. You need at least Unity 2020.2.",
                        "Cancel");
            });

            _combineGUI = new CombineGUI(_data, DrawAtlasMap);
            _atlasGUI = new AtlasGUI(_data, DrawAtlasMap);
            _allInOneGUI = new AllInOneGUI(_data);
            _shaderMappingGUI = new ShaderMappingGUI(_data);
        }

        private static void FindWindow()
        {
            window = (AtlasMapperWindow) GetWindow(typeof(AtlasMapperWindow), false, "Mesh Draw Call Optimizer");
        }

        private void OnGUI()
        {
            if (_styles == null)
                InitStyles();

            EditorGUILayout.BeginVertical();
            DrawMenu();
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            if (_data.ActiveMenu == 0)
                DrawTitle();

            DrawSaveFolder();
            switch (_data.ActiveMenu)
            {
                case 0:
                    _allInOneGUI.Draw();
                    break;
                case 1:
                    _atlasGUI.Draw(position);
                    break;
                case 2:
                    _combineGUI.Draw();
                    break;
                case 3:
                    _shaderMappingGUI.Draw();
                    break;
                case 4:
                    DrawHelp();
                    break;
            }

            GUI.enabled = true;

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void DrawSaveFolder()
        {
            var activeScene = SceneManager.GetActiveScene();
            var activeSceneName = !string.IsNullOrEmpty(activeScene.path) ? activeScene.name : "Untitled";
            SaveFolder = GUILayoutExtension.DrawSaveFolder(SaveFolder, $"Assets/Graphics/AtlasObjects/{activeSceneName}");

            var saveFolderCreated = SaveFolder != null;

            if (!saveFolderCreated)
                EditorGUILayout.HelpBox("Please create the save folder before getting started.", MessageType.Warning);

            GUI.enabled = saveFolderCreated;
        }

        private void DrawTitle()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(Styles.IconMeshRenderer), GUILayout.Width(48),
                GUILayout.Height(48));
            EditorGUILayout.LabelField($"Draw Call Optimizer {_version.ToString(3)}", _styles.Header,
                GUILayout.Height(48), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

        }

        private void DrawMenu()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {
                for (var index = 0; index < _menuNames.Count; index++)
                {
                    var menuName = _menuNames[index];
                    if (GUILayout.Toggle(_data.ActiveMenu == index, menuName, EditorStyles.toolbarButton,
                        GUILayout.MaxWidth(100)))
                        _data.ActiveMenu = index;
                }
            }
            EditorGUILayout.EndHorizontal();
        }


        private void InitStyles()
        {
            _styles = new Styles();
        }

        private void DrawHelp()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            GUI.enabled = true;
            if (GUILayout.Button("Please select an option:", EditorStyles.label))
            {
                _clickCounter++;
                if (_clickCounter > 5)
                {
                    _data.IsDebugOn = !_data.IsDebugOn;
                    _clickCounter = 0;
#if UNITY_2020_1_OR_NEWER
                    ShowNotification(new GUIContent("Activated debug mode"), 2);
#else
                    ShowNotification(new GUIContent("Activated debug mode"));
#endif
                }
            }

            if (GUILayout.Button("Open Help"))
                try
                {
                    Process.Start(Path.GetFullPath("Assets/Tools/DrawCallOptimizer/Documentation/Documentation.pdf"));
                }
                catch
                {
                    EditorUtility.DisplayDialog("Error",
                        "Could not locate help file, please have a look for a Documentation.pdf in the assets folder.",
                        "Okay");
                }

            if (GUILayout.Button("Report a bug"))
                if (EditorUtility.DisplayDialog("Report a bug", "This will open your browser to report a bug. Continue?",
                    "Continue", "Cancel"))
                    try
                    {
                        Process.Start("https://justassets.atlassian.net/servicedesk/customer/portal/3");
                    }
                    catch
                    {
                        EditorUtility.DisplayDialog("Error", "Could not open web browser.", "Okay");
                    }

            EditorGUILayout.EndVertical();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        internal class Styles
        {
            public static readonly Texture2D IconMeshRenderer = AssetPreview.GetMiniTypeThumbnail(typeof(TerrainCollider));

            public readonly GUIStyle Header;

            public Styles()
            {
                var largeLabel = new GUIStyle(EditorStyles.largeLabel);
                largeLabel.fontSize = 18;
                largeLabel.alignment = TextAnchor.MiddleLeft;
                Header = largeLabel;
            }
        }
    }
}