using System;
using System.Collections.Generic;
using System.Linq;
using JustAssets.AtlasMapPacker.Rendering;
using UnityEditor;
using UnityEngine;

#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering;
#else
using ShaderPropertyType = UnityEditor.ShaderUtil.ShaderPropertyType;
#endif
using ShaderUtil = JustAssets.AtlasMapPacker.Rendering.ShaderUtil;

namespace JustAssets.AtlasMapPacker
{
    internal class ShaderMappingGUI
    {
        private static Dictionary<DataSource, (Texture2D, string)> _typeMapping =
            new Dictionary<DataSource, (Texture2D, string)>
            {
                {DataSource.TextureAttribute, (AssetPreview.GetMiniTypeThumbnail(typeof(Texture2D)), "Texture")},
                {DataSource.VectorAttribute, (AssetPreview.GetMiniTypeThumbnail(typeof(Transform)), "Vector")},
                {DataSource.FloatAttribute, (AssetPreview.GetMiniTypeThumbnail(typeof(MonoScript)), "Float")}
            };

        private readonly GUIData _data;

        private GUIContent _guiNormalizationLabel = new GUIContent("Normalization:",
            "Set this field if the float/vector attribute can be larger than 1. The values will then be normalized in the texture and the highest value stored in this field.");

        private GUIContent _guiTilingOffsetLabel = new GUIContent("Tiling from here:",
            "If this is activated the tiling and offset of this texture is used for all textures. Use this only if you use tiling or scaling on textures, because this will reduce sampling precision from float to half. This causes artifacts for textures larger than 4096^2.");

        private GUIStyle _lighterStyle;

        private GUIStyle _normalStyle;

        private Vector2 _scrollPos;

        private IShaderInfo _shaderInfo;

        private Dictionary<string, ShaderPropertyType> _targetShaderAttributes =
            new Dictionary<string, ShaderPropertyType>();

        private ValueComponent[] _validComponents;

        private string[] _validComponentTexts;

        private IList<ConditionFilter> _validConditionTypes;

        private string[] _validConditionTypeTexts;

        private string[] _validFloatSlotNames;

        private string[] _validTextureSlotNames;

        private XmlShaderInfo _xmlShaderInfo;

        public ShaderMappingGUI(GUIData data)
        {
            var texture2D = new Texture2D(1, 1);
            texture2D.SetPixels(new[] {new Color(1, 1, 1, 0.05f)});
            texture2D.Apply();

            _lighterStyle = new GUIStyle {normal = new GUIStyleState {background = texture2D}};
            _normalStyle = new GUIStyle();

            _data = data;
            if (_data.SourceShader != null)
                ReloadShaderData(_data.SourceShader, false);
        }

        public void Draw()
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));
            EditorGUILayout.BeginHorizontal();
            var sourceShader =
                (UnityEngine.Shader) EditorGUILayout.ObjectField("Shader to inspect", _data.SourceShader, typeof(UnityEngine.Shader), true);
            if (GUILayout.Button("Open", GUILayout.Width(80)))
                ShaderUtil.OpenSourceShader(_data.SourceShader);

            EditorGUILayout.EndHorizontal();
        
            EditorGUILayout.BeginHorizontal();
            var targetShader =
                (UnityEngine.Shader) EditorGUILayout.ObjectField("Atlas shader to map to", _data.TargetShader, typeof(UnityEngine.Shader), true);
            if (targetShader == null && GUILayout.Button("Create", GUILayout.Width(80)))
            {
                targetShader = ShaderUtil.CreateAtlasShader(_data.SourceShader, _xmlShaderInfo);
                _data.ShaderConfiguration = _xmlShaderInfo.Save();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _data.ShaderConfiguration =
                (TextAsset) EditorGUILayout.ObjectField("Configuration", _data.ShaderConfiguration, typeof(TextAsset),
                    false);
            if (GUILayout.Button("Save", GUILayout.Width(80)))
                _data.ShaderConfiguration = _xmlShaderInfo.Save();

            EditorGUILayout.EndHorizontal();

            var targetShaderChanged = targetShader != _data.TargetShader;
            _data.TargetShader = targetShader;

            if (sourceShader != _data.SourceShader || targetShaderChanged)
                ReloadShaderData(sourceShader, targetShaderChanged);

            _data.SourceShader = sourceShader;

            if (_data.SourceShader != null && _data.TargetShader != null)
                EditorGUILayout.LabelField($"Source: {_data.SourceShader.name} > Target: {_data.TargetShader.name}",
                    EditorStyles.helpBox);

            var indent = 10;
            if (_xmlShaderInfo != null)
            {
                DrawNamingSection(_xmlShaderInfo.NamingRules, "Naming Rules", "Rule", indent);

                DrawAttributeSection(_xmlShaderInfo.ShaderAttributesToTransfer, indent);

                DrawInstructionSection(_xmlShaderInfo.KeywordTransferInstructions, "Keyword Mapping", "Keyword",
                    (instruction, width) => instruction.Keyword = EditorGUILayout.TextField(instruction.Keyword, width),
                    indent);

                DrawInstructionSection(_xmlShaderInfo.RenderTypeMapping, "Render Type Selection", "Type",
                    (instruction, width) => instruction.TargetRenderType =
                        EditorGUILayout.TextField(instruction.TargetRenderType, width), indent);

                DrawInstructionSection(_xmlShaderInfo.RenderQueueMapping, "Render Queue Selection", "Queue",
                    (instruction, width) => instruction.TargetRenderQueue =
                        EditorGUILayout.IntField(instruction.TargetRenderQueue, width), indent);
            }

            GUILayout.EndScrollView();
        }

        private void DrawAttributeSection(IEnumerable<AttributePointerPair> items, int indent)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Attribute Mapping", EditorStyles.whiteLargeLabel, GUILayout.Height(26));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(indent));
            EditorGUILayout.BeginVertical();
            Color guiBackground = GUI.backgroundColor;

            if (items != null)
            {
                if (_data.TargetShader == null)
                    EditorGUILayout.HelpBox("Please select a target atlas shader for mapping.", MessageType.Warning);

                var isEven = false;
                foreach (AttributePointerPair attribute in items)
                {
                    var propName = attribute.Source.Name;
                    DataSource type = attribute.Source.DataSource;

                    EditorGUILayout.BeginHorizontal(isEven ? _normalStyle : _lighterStyle, GUILayout.Height(24));
                    isEven = !isEven;

                    GUILayoutOption nameSlotWidth = GUILayout.Width(140);

                    DrawShaderType(type);

                    EditorGUILayout.LabelField(propName, EditorStyles.boldLabel, nameSlotWidth);

                    if (_data.TargetShader != null)
                    {
                        EditorGUILayout.LabelField(">", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(20));

                        if (!IsTargetValid(attribute))
                            GUI.backgroundColor = Color.red;

                        // Show possible targets
                        var validDataSourcesTexts = GetValidDataSourcesTexts(attribute.Source.DataSource);
                        var validDataSources = GetValidDataSources(attribute.Source.DataSource);
                        const int width = 70;
                        if (attribute.Target != null)
                        {
                            attribute.Target.DataSource = DrawFilteredTextPopup(attribute.Target.DataSource,
                                validDataSources, validDataSourcesTexts, DataSource.Invalid, width: width);

                            if (!attribute.Target.DataSource.IsTexCoord())
                            {
                                switch (attribute.Target.DataSource)
                                {
                                    case DataSource.TextureAttribute:
                                    {
                                        attribute.Target.Name = DrawFilteredTextPopup(attribute.Target.Name,
                                            _validTextureSlotNames, _validTextureSlotNames, "");

                                        if (attribute.Source.DataSource == DataSource.FloatAttribute &&
                                            attribute.Target.DataSource == DataSource.TextureAttribute)
                                            attribute.Target.Component = DrawFilteredTextPopup(attribute.Target.Component,
                                                _validComponents, _validComponentTexts, ValueComponent.X, width: 40);
                                        else
                                            EditorGUILayout.LabelField("", EditorStyles.miniLabel, GUILayout.Width(40));
                                        break;
                                    }
                                    case DataSource.FloatAttribute:
                                        EditorGUILayout.LabelField("use most common value", EditorStyles.miniLabel,
                                            GUILayout.Width(120));
                                        EditorGUILayout.LabelField("", EditorStyles.miniLabel, GUILayout.Width(40));
                                        break;
                                }

                                EditorGUILayout.LabelField("", GUILayout.Width(10));
                                
                                switch (attribute.Source.DataSource)
                                {
                                    case DataSource.FloatAttribute:
                                    case DataSource.VectorAttribute:
                                        EditorGUILayout.LabelField(_guiNormalizationLabel, EditorStyles.whiteMiniLabel,
                                            GUILayout.Width(80));
                                        attribute.Target.MaximumAttribute = DrawFilteredTextPopup(attribute.Target.MaximumAttribute, _validFloatSlotNames, _validFloatSlotNames,
                                            string.Empty);
                                        break;
                                    case DataSource.TextureAttribute:
                                        EditorGUILayout.LabelField(_guiTilingOffsetLabel, EditorStyles.whiteMiniLabel,
                                            GUILayout.Width(80));

                                        bool isOn = GUILayout.Toggle(attribute.Target.UseTiling, "");
                                        if (isOn != attribute.Target.UseTiling)
                                        {
                                            foreach (var item in items)
                                            {
                                                if (item?.Target != null)
                                                    item.Target.UseTiling = false;
                                            }

                                            attribute.Target.UseTiling = isOn;
                                        }

                                        break;
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField("using TexCoord", EditorStyles.miniLabel, GUILayout.Width(120));
                                EditorGUILayout.LabelField("", EditorStyles.miniLabel, GUILayout.Width(40));
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField("", EditorStyles.miniLabel, GUILayout.Width(width));
                            EditorGUILayout.LabelField("discarding data", EditorStyles.miniLabel, GUILayout.Width(120));
                            EditorGUILayout.LabelField("", EditorStyles.miniLabel, GUILayout.Width(40));
                        }

                        EditorGUILayout.Space();
                        if (GUILayout.Button(attribute.Target != null ? "Discard" : "Create", GUILayout.Width(80)))
                            attribute.Target = attribute.Target != null ? null : new AttributeTargetPointer();

                        GUI.backgroundColor = guiBackground;
                    }

                    EditorGUILayout.EndHorizontal();
                    GUILayoutUtility.GetRect(5f, 5f);
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private Condition DrawCondition(Condition instructionCondition, out bool doDelete)
        {
            doDelete = false;
            EditorGUILayout.LabelField("if", EditorStyles.miniLabel, GUILayout.Width(20));

            const int width = 70;
            ConditionFilter newConditionFilter = DrawFilteredTextPopup(instructionCondition.ConditionFilter,
                _validConditionTypes, _validConditionTypeTexts, ConditionFilter.AttributeSet, width: width);

            string newAttributeName = instructionCondition.AttributeName;
            float newAttributeValue = instructionCondition.Value;
            if (instructionCondition.ConditionFilter != ConditionFilter.Nothing)
            {
                EditorGUILayout.LabelField("Source attribute", EditorStyles.miniLabel, GUILayout.Width(120));
                newAttributeName = EditorGUILayout.TextField(newAttributeName, GUILayout.Width(100));
                if (instructionCondition.ConditionFilter != ConditionFilter.Equals)
                {
                    EditorGUILayout.LabelField("", EditorStyles.miniLabel, GUILayout.Width(20));
                    EditorGUILayout.LabelField("", EditorStyles.miniLabel, GUILayout.Width(30));
                }
                else
                {
                    EditorGUILayout.LabelField("is", EditorStyles.miniLabel, GUILayout.Width(20));
                    newAttributeValue = EditorGUILayout.FloatField(newAttributeValue, GUILayout.Width(30));
                }
            }
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Delete", GUILayout.Width(80)))
                doDelete = true;

            if (newConditionFilter != instructionCondition.ConditionFilter ||
                newAttributeName != instructionCondition.AttributeName ||
                Math.Abs(newAttributeValue - instructionCondition.Value) > 0.00001)
                return new Condition(newAttributeName, newConditionFilter, newAttributeValue);

            return instructionCondition;
        }

        private static T DrawFilteredTextPopup<T>(T currentValue, IList<T> validDataSources, string[] validDataSourcesTexts,
            T invalid = default, GUIStyle style = null, int width = 120)
        {
            if (validDataSources == null)
                return currentValue;

            var oldIndex = validDataSources.IndexOf(currentValue);

            GUIStyle guiStyle = style;
            if (guiStyle == null)
                guiStyle = EditorStyles.popup;

            var newIndex = EditorGUILayout.Popup(oldIndex, validDataSourcesTexts, guiStyle, GUILayout.Width(width));
            return newIndex != oldIndex ? newIndex >= 0 ? validDataSources[newIndex] : invalid : currentValue;
        }

        private void DrawInstructionSection<T>(ICollection<T> instructions, string header, string rowHeader,
            Action<T, GUILayoutOption> mainSection, int indent) where T : class, IConditionalInstruction, new()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(header, EditorStyles.whiteLargeLabel, GUILayout.Height(26), GUILayout.Width(180));
            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(30)))
                instructions.Add(new T());
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(indent));
            EditorGUILayout.BeginVertical();
            DrawMapping(rowHeader, instructions, mainSection);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawLineHeader(Texture2D texture, string label)
        {
            EditorGUILayout.LabelField(new GUIContent(texture), GUILayout.Width(24), GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, EditorStyles.toolbarButton, GUILayout.Width(60),
                GUILayout.ExpandWidth(false));
        }

        private void DrawMapping<T>(string label, ICollection<T> items, Action<T, GUILayoutOption> mainField)
            where T : class, IConditionalInstruction
        {
            Color guiBackground = GUI.backgroundColor;
            if (items == null)
                return;

            var isEven = false;
            T toDelete = default;

            foreach (T instruction in items)
            {
                EditorGUILayout.BeginHorizontal(isEven ? _normalStyle : _lighterStyle, GUILayout.Height(24));
                isEven = !isEven;

                GUILayoutOption nameSlotWidth = GUILayout.Width(140);

                DrawLineHeader(AssetPreview.GetMiniTypeThumbnail(typeof(MonoScript)), label);

                mainField.Invoke(instruction, nameSlotWidth);

                instruction.Condition = DrawCondition(instruction.Condition, out var doDelete);

                if (doDelete)
                    toDelete = instruction;

                GUI.backgroundColor = guiBackground;

                EditorGUILayout.EndHorizontal();
                GUILayoutUtility.GetRect(5f, 5f);
            }

            if (toDelete != null)
                items.Remove(toDelete);
        }

        private void DrawNamingSection(ICollection<NamingRule> items, string header, string rowHeader, int indent)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(header, EditorStyles.whiteLargeLabel, GUILayout.Height(26), GUILayout.Width(180));
            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(30)))
                items.Add(new NamingRule());
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(indent));
            EditorGUILayout.BeginVertical();

            Color guiBackground = GUI.backgroundColor;
            if (items != null)
            {
                var isEven = false;
                NamingRule toDelete = default;

                foreach (NamingRule namingRule in items)
                {
                    EditorGUILayout.BeginHorizontal(isEven ? _normalStyle : _lighterStyle, GUILayout.Height(24));
                    isEven = !isEven;

                    DrawLineHeader(AssetPreview.GetMiniTypeThumbnail(typeof(MonoScript)), rowHeader);

                    namingRule.Type = (NamingRuleType) EditorGUILayout.EnumPopup(namingRule.Type, GUILayout.Width(140));

                    GUILayoutOption width = GUILayout.Width(120);
                    if (namingRule.Type == NamingRuleType.FloatAttribute)
                        namingRule.AttributeName = EditorGUILayout.TextField(namingRule.AttributeName, width);
                    else
                        EditorGUILayout.LabelField("", width);

                    EditorGUILayout.LabelField("", GUILayout.ExpandWidth(true));

                    if (GUILayout.Button("Delete", GUILayout.Width(80)))
                        toDelete = namingRule;

                    GUI.backgroundColor = guiBackground;

                    EditorGUILayout.EndHorizontal();
                    GUILayoutUtility.GetRect(5f, 5f);
                }

                if (toDelete != null)
                    items.Remove(toDelete);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawShaderType(DataSource attributeType)
        {
            if (!_typeMapping.TryGetValue(attributeType, out (Texture2D, string) representation))
                return;

            Texture2D texture = representation.Item1;
            var label = representation.Item2;
            DrawLineHeader(texture, label);
        }

        private static DataSource[] GetValidDataSources(DataSource dataSource)
        {
            DataSource[] result;
            switch (dataSource)
            {
                case DataSource.TextureAttribute:
                    result = new[] {DataSource.TextureAttribute};
                    break;
                case DataSource.FloatAttribute:
                    result = new[] {DataSource.TextureAttribute, DataSource.FloatAttribute};
                    break;
                case DataSource.VectorAttribute:
                    result = new[] {DataSource.TextureAttribute, DataSource.Color};
                    break;
                default:
                    result = new[]
                    {
                        DataSource.Color, DataSource.UV3, DataSource.UV4, DataSource.UV5, DataSource.UV6, DataSource.UV7
                    };
                    break;
            }

            return result;
        }

        private string[] GetValidDataSourcesTexts(DataSource dataSource)
        {
            return GetValidDataSources(dataSource).Select(x => x.ToString().Replace("Attribute", "")).ToArray();
        }

        private bool IsTargetValid(AttributePointerPair attribute)
        {
            if (attribute.Target == null)
                return true;

            if (attribute.Target.DataSource.IsTexCoord())
                return true;

            if (attribute.Source.DataSource == DataSource.FloatAttribute &&
                attribute.Target.DataSource == DataSource.FloatAttribute)
                return true;

            var targetName = attribute.Target.Name;

            if (targetName == null && !attribute.Target.DataSource.IsTexCoord())
                return false;

            if (string.IsNullOrEmpty(targetName))
                return false;

            if (!_targetShaderAttributes.ContainsKey(targetName))
                return false;

            // Mapping of single value to range
            if (attribute.Source.DataSource == DataSource.FloatAttribute &&
                attribute.Target.Component == ValueComponent.All)
                return false;

            return true;
        }

        private void ReloadShaderData(UnityEngine.Shader sourceShader, bool targetShaderChanged)
        {
            _shaderInfo = sourceShader != null ? new ShaderInfoFactory().Create(sourceShader.name) : null;

            if (_shaderInfo is XmlShaderInfo xmlShaderInfo)
            {
                _xmlShaderInfo = xmlShaderInfo;
                _data.ShaderConfiguration = xmlShaderInfo.Asset;
            }
            else
            {
                _data.ShaderConfiguration = null;
            }

            if (targetShaderChanged && _shaderInfo != null)
                _shaderInfo.TargetShaderName = _data.TargetShader != null ? _data.TargetShader.name : null;
            else
                _data.TargetShader = !string.IsNullOrEmpty(_shaderInfo?.TargetShaderName)
                    ? UnityEngine.Shader.Find(_shaderInfo.TargetShaderName)
                    : null;

#if UNITY_2019_3_OR_NEWER
            const ShaderPropertyType texture = ShaderPropertyType.Texture;
#else
            const ShaderPropertyType texture = ShaderPropertyType.TexEnv;
#endif

            _targetShaderAttributes = _data.TargetShader.GetProperties().ToDictionary(x => x.Name, x => x.Type);
            _validTextureSlotNames = _targetShaderAttributes.Where(x =>
                {
                    return x.Value == texture;
                })
                .Select(x => x.Key).ToArray();
            _validFloatSlotNames = _targetShaderAttributes.Where(x => x.Value == ShaderPropertyType.Float)
                .Select(x => x.Key).Union(new[] {"-"}).ToArray();
            _validComponents = new[] {ValueComponent.X, ValueComponent.Y, ValueComponent.Z, ValueComponent.W};
            _validComponentTexts = _validComponents.Select(x => x.ToString()).ToArray();
            _validConditionTypes = (ConditionFilter[]) Enum.GetValues(typeof(ConditionFilter));
            _validConditionTypeTexts = Enum.GetNames(typeof(ConditionFilter));
        }

        private int SelectedIndex<T>(string propName, T[] array)
        {
            if (array == null || propName == null)
                return -1;
            return Math.Max(0, Array.IndexOf(array, propName));
        }
    }
}
