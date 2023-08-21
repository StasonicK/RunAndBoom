using System;
using System.Diagnostics;
using JustAssets.ColliderUtilityRuntime;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace JustAssets.ColliderUtilityEditor
{
    public class ColliderUtilityEditor : EditorWindow
    {
        [SerializeField]
        private bool _createConvexHull;

        [SerializeField]
        private float _decimateVertices = 0.001f;

        private bool _createGameObject;

        private bool _disableColliders;

        private Styles _styles;

        private bool _writeMeshToFilter;

        [SerializeField]
        private DefaultAsset _saveFolder;

        [SerializeField]
        private bool _saveToAsset;

        [SerializeField]
        private int _layersToConsider = -1;

        private readonly Version _version = new Version(2, 0, 1);

        [SerializeField]
        private float _faceNormalOffsetDegree = DefaultFaceAngle;

        [SerializeField]
        private bool _showAPICommand;

        private GameObject _activeGameObject;
        private bool _selectionHasColliderItself;

        [SerializeField]
        private bool _recursive = true;

        private const float DefaultFaceAngle = 2f;

        public void OnGUI()
        {
            if (_styles == null)
                InitStyles();

            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            HeaderBar();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(Styles.IconMeshCollider), GUILayout.Width(48), GUILayout.Height(48));
            EditorGUILayout.LabelField($"3D Collider Utility {_version.ToString(3)}", _styles.Header, GUILayout.Height(48), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            if (_activeGameObject == null)
            {
                EditorGUILayout.HelpBox("Select a game object in the hierarchy which contains at least one collider and the pick one of the options.",
                    MessageType.Info);
            }

            var colliders = _recursive ? "Selection + children" : "Selection";

            DrawHeader(_styles.GuiColliderIcon, colliders, _writeMeshToFilter ? _styles.GuiMeshIcon : _styles.GuiColliderIcon,
                _writeMeshToFilter ? "MeshFilter" : "MeshCollider");

            _recursive = EditorGUILayout.ToggleLeft("Include children", _recursive);

            var conditionsMet = ConditionsFulfilled(out var unmetReason);

            GUI.enabled = conditionsMet;
            if (GUILayout.Button(new GUIContent(_showAPICommand ? "Show command" : "Combine", conditionsMet ? "" : unmetReason), GUILayout.Height(30)))
            {
                if (_showAPICommand)
                {
                    var apiCommand = GenerateCommand();
                    if (EditorUtility.DisplayDialog("API command", $"Copy the API command to clipboard?\r\n\r\n{apiCommand}",
                        "Copy", "Cancel"))
                    {
                        EditorGUIUtility.systemCopyBuffer = apiCommand;
                    }
                }
                else
                    Perform();
            }
            GUI.enabled = true;

            EditorGUILayout.Space();

            _writeMeshToFilter = EditorGUILayout.ToggleLeft("Write result to MeshFilter", _writeMeshToFilter);

            GUI.enabled = !_writeMeshToFilter;
            _disableColliders = EditorGUILayout.ToggleLeft("Disable merged collider components", _disableColliders);
            GUI.enabled = true;

            GUI.enabled = _writeMeshToFilter;
            _createGameObject = EditorGUILayout.ToggleLeft(
                new GUIContent("Create result on new game object",
                    "Use this in combination with 'Write result to mesh filter' to get a new game object with Filter and Renderer"), _createGameObject);
            GUI.enabled = true;
            
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            
            _layersToConsider = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask((LayerMask) EditorGUILayout.MaskField("Layers to consider",
                InternalEditorUtility.LayerMaskToConcatenatedLayersMask(_layersToConsider), InternalEditorUtility.layers));
            
            if (_layersToConsider == 0)
            {
                EditorGUILayout.HelpBox("Your layer selection will result in an empty mesh. Please select a valid layer mask.",
                    MessageType.Error);
            }

            _createConvexHull = EditorGUILayout.ToggleLeft("Create convex hull of final mesh", _createConvexHull);

            EditorGUILayout.BeginHorizontal();
            var mergeCloseVertices = EditorGUILayout.ToggleLeft("Decimate vertices closer than", _decimateVertices > 0f);
            GUI.enabled = mergeCloseVertices;
            _decimateVertices = mergeCloseVertices
                ? EditorGUILayout.Slider(_decimateVertices, 0.001f, 1f, GUILayout.Width(160))
                : -1f;

            EditorGUILayout.EndHorizontal();

            _faceNormalOffsetDegree = mergeCloseVertices
                ? EditorGUILayout.Slider("Max Face Angle (deg)", _faceNormalOffsetDegree, 0f, 10f)
                : DefaultFaceAngle;

            GUI.enabled = true;

            _saveFolder = GUILayoutExtension.DrawSaveFolder(_saveFolder, "Assets/SavedMeshes");

            if (_saveFolder == null)
                EditorGUILayout.HelpBox("If no save folder is selected the mesh will be saved in the scene file.", MessageType.Info);

            GUI.enabled = _saveFolder != null;
            _saveToAsset = EditorGUILayout.ToggleLeft("Save mesh to file", _saveToAsset);
            GUI.enabled = true;
            _showAPICommand = EditorGUILayout.ToggleLeft("Show API command", _showAPICommand);
            EditorGUILayout.EndVertical();
        }

        public void OnEnable()
        {
            UpdateSelection();
        }

        private string GenerateCommand()
        {
            var combineSettings = $"    new {nameof(ColliderCombineSettings)}(" +
                                  $"\r\n        recursive: {_recursive.ToString().ToLower()}," +
                                  $"\r\n        finalConvexHull: {_createConvexHull.ToString().ToLower()}," +
                                  $"\r\n        layersToConsider: 0x{_layersToConsider:X}," +
                                  $"\r\n        decimateVertices: {_decimateVertices}f," +
                                  $"\r\n        faceNormalOffsetDegree: {_faceNormalOffsetDegree}f," +
                                  "\r\n        saveMesh: mesh => mesh)";

            var command = _writeMeshToFilter
                ? $"{nameof(ColliderUtility)}.{nameof(ColliderUtility.CreateFilter)}(\r\n    gameObject,\r\n    {combineSettings},\r\n    createGameObject: {_createGameObject.ToString().ToLower()});"
                : $"{nameof(ColliderUtility)}.{nameof(ColliderUtility.CreateCollider)}(\r\n    gameObject,\r\n    {combineSettings},\r\n    disableColliders: {_disableColliders.ToString().ToLower()});";

            return command;
        }

        private void Perform()
        {
            var colliderCombineSettings = CreateColliderCombineSettings(_recursive);
            if (_writeMeshToFilter)
                ColliderUtility.CreateFilter(_activeGameObject, colliderCombineSettings, _createGameObject);
            else
                ColliderUtility.CreateCollider(_activeGameObject, colliderCombineSettings, _disableColliders);
        }

        public void OnSelectionChange()
        {
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            _activeGameObject = Selection.activeGameObject;
            _selectionHasColliderItself = _activeGameObject?.GetComponent<Collider>() != null;
            
            Repaint();
        }

        private ColliderCombineSettings CreateColliderCombineSettings(bool recursive)
        {
            return new ColliderCombineSettings(recursive, _createConvexHull, _layersToConsider, _decimateVertices, _faceNormalOffsetDegree, SaveAsset);
        }

        private bool ConditionsFulfilled(out string unmetReason)
        {
            if (_activeGameObject == null)
            {
                unmetReason = "Please select a gameobject to combine.";
                return false;
            }

            if (!_recursive && !_selectionHasColliderItself)
            {
                unmetReason = "Please select recursive option or select a gameobject which contains a collider itself.";
                return false;
            }

            if (_layersToConsider == 0)
            {
                unmetReason = "Please select a valid layer mask to use for filtering the gameobjects to consider.";
                return false;
            }

            unmetReason = null;
            return true;
        }

        internal class Styles
        {
            public static readonly Texture2D IconMeshFilter = AssetPreview.GetMiniTypeThumbnail(typeof(MeshFilter));

            public static readonly Texture2D IconMeshCollider = AssetPreview.GetMiniTypeThumbnail(typeof(MeshCollider));

            public static GUIContent TitleContent = new GUIContent("Collider Utility",
                IconMeshCollider.ScaledTexture2D(16, 16), "Tool window to merge colliders");

            public readonly GUIContent GuiColliderIcon = new GUIContent(IconMeshCollider);

            public readonly GUIContent GuiMeshIcon = new GUIContent(IconMeshFilter);

            public readonly GUIStyle Header;

            public Styles()
            {
                var largeLabel = new GUIStyle(EditorStyles.largeLabel);
                largeLabel.fontSize = 18;
                largeLabel.alignment = TextAnchor.MiddleLeft;
                Header = largeLabel;
            }
        }

        [MenuItem("Tools/Colliders/Selection To Collider (recursive)")]
        private static void CollidersToMesh()
        {
            ColliderUtility.CreateCollider(Selection.activeGameObject, new ColliderCombineSettings());
        }

        [MenuItem("Tools/Colliders/Selection To Collider")]
        private static void ColliderToMesh()
        {
            ColliderUtility.CreateCollider(Selection.activeGameObject, new ColliderCombineSettings(false));
        }

        private void DrawHeader(GUIContent icon, string text, GUIContent secondIcon, string secondText)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(icon, GUILayout.Width(20), GUILayout.Height(20));
            EditorGUILayout.LabelField(text, GUILayout.Width(120));
            EditorGUILayout.LabelField("->", GUILayout.Width(20));
            EditorGUILayout.LabelField(secondIcon, GUILayout.Width(20), GUILayout.Height(20));
            EditorGUILayout.LabelField(secondText, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
        }

        private static void HeaderBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                EditorGUILayout.LabelField("", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Licenses", EditorStyles.toolbarButton, GUILayout.Width(70)))
                {
                    EditorUtility.DisplayDialog("Licenses",
                        "This tool uses the following libraries published under the MIT license:\n\n- MIConvexHull\n- MeshSimplifier\n\nFor more details see license.txt",
                        "Close");
                }

                if (GUILayout.Button("Support", EditorStyles.toolbarButton, GUILayout.Width(70)))
                {
                    if (EditorUtility.DisplayDialog("Support",
                            "If you found a bug or have a feature request please get in contact with me using the support webpage.", "Open webpage", "Cancel"))
                        Process.Start("https://justassets.atlassian.net/servicedesk");
                }

            }
            EditorGUILayout.EndHorizontal();
        }

        private void InitStyles()
        {
            _styles = new Styles();
        }

        [MenuItem("Tools/Colliders/Collider Utility...")]
        private static void Open()
        {
            var window = GetWindow<ColliderUtilityEditor>("Collider Utility");
            window.titleContent = Styles.TitleContent;
            window.Show();
        }

        private Mesh SaveAsset(Mesh meshInMemory)
        {
            if (_saveToAsset && _saveFolder != null)
            {
                return AssetDatabaseHelper.SaveAsset(meshInMemory, meshInMemory.name + ".asset", AssetDatabase.GetAssetPath(_saveFolder), string.Empty);
            }

            return meshInMemory;
        }
    }
}