// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace JustAssets.AtlasMapPacker.Shader
{
    internal class AtlasStandardShaderGUI : ShaderGUI
    {
        private enum WorkflowMode
        {
            Specular,
            Metallic,
            Dielectric
        }

        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
        }

        private static class Styles
        {
            public static readonly GUIContent AlbedoText = EditorGUIUtility.TrTextContent("Albedo", "Albedo (RGB) and Transparency (A)");
            public static readonly GUIContent SpecularMapText = EditorGUIUtility.TrTextContent("Specular", "Specular (RGB) and Smoothness (A)");
            public static readonly GUIContent MetallicMapText = EditorGUIUtility.TrTextContent("Metallic", "Metallic (R) and Smoothness (A)");
            public static readonly GUIContent NormalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Normal Map");
            public static readonly GUIContent HeightMapText = EditorGUIUtility.TrTextContent("Height Map", "Height Map (G)");
            public static readonly GUIContent OcclusionText = EditorGUIUtility.TrTextContent("Occlusion", "Occlusion (G)");
            
            public static readonly GUIContent LightmapEmissiveLabel = EditorGUIUtility.TrTextContent("Global Illumination", "Controls if the emission is baked or realtime.\n\nBaked only has effect in scenes where baked global illumination is enabled.\n\nRealtime uses realtime global illumination if enabled in the scene. Otherwise the emission won't light up other objects.");
            public static readonly GUIContent EmissionText = EditorGUIUtility.TrTextContent("Emission Map", "Emission (RGB)");
            public static readonly GUIContent EmissionColorText = EditorGUIUtility.TrTextContent("Emission Color", "Emission Color (RGB)");

            public static readonly GUIContent GlsSmcParBusText = EditorGUIUtility.TrTextContent("Gloss Scale / Smoothness Chan / Parallax / Bumpmap Scale", "Threshold for alpha cutoff");

            public static readonly GUIContent CutOccGloText = EditorGUIUtility.TrTextContent("Cutoff / Occ. Strength / Gloss");
            
            public static readonly GUIContent HighlightsText = EditorGUIUtility.TrTextContent("Specular Highlights", "Specular Highlights");
            public static readonly GUIContent ReflectionsText = EditorGUIUtility.TrTextContent("Reflections", "Glossy Reflections");
            
            public static readonly GUIContent AtlasWidthText = EditorGUIUtility.TrTextContent("Atlas Map Texture Width", "The size in pixels of the atlas map's width.");
            public static readonly GUIContent AtlasHeightText = EditorGUIUtility.TrTextContent("Atlas Map Texture Height", "The size in pixels of the atlas map's height.");
            public static readonly GUIContent MaxMipMapText = EditorGUIUtility.TrTextContent("Maximum Mip Map Level", "This depends on the padding of the tiles in the atlas map. A padding of 2 allows a mip-map of 0-1. A padding of 16 allows a mip-map of 0-3.");

            public static GUIContent[] LightmapEmissiveStrings = {
                EditorGUIUtility.TrTextContent("Realtime"),
                EditorGUIUtility.TrTextContent("Baked"),
                EditorGUIUtility.TrTextContent("None")
            };
            public static int[] LightmapEmissiveValues = {
                1,
                2,
                0
            };
            
            public static string PrimaryMapsText = "Main Maps";
            public static string ForwardText = "Forward Rendering Options";
            public static string BakedProperties = "Baked Properties";
            public static string AtlasText = "Atlas Mapping";
            public static string RenderingMode = "Rendering Mode";
            public static string AdvancedText = "Advanced Options";
            public static readonly string[] BLEND_NAMES = Enum.GetNames(typeof(BlendMode));
        }

        private MaterialProperty _blendMode;

        private MaterialProperty _albedoMap;

        private MaterialProperty _specularMap;

        private MaterialProperty _metallicMap;

        private MaterialProperty _bumpMap;

        private MaterialProperty _occlusionMap;

        private MaterialProperty _heightMap;

        private MaterialProperty _emissionMap;

        private MaterialProperty _emissionColorMap;

        private MaterialProperty _maxMipLevel;
        
        private MaterialProperty _atlasWidth;

        private MaterialProperty _atlasHeight;

        private MaterialEditor _materialEditor;

        private WorkflowMode _mWorkflowMode = WorkflowMode.Specular;

        private bool _mFirstTimeApply = true;

        private MaterialProperty _glsSmcParBusMap;

        private MaterialProperty _bumpMapMax;

        private MaterialProperty _cutOccGloMap;

        private MaterialProperty _highlights;

        private MaterialProperty _reflections;

        private MaterialProperty _emissionScale;

        public void FindProperties(MaterialProperty[] props)
        {
            _blendMode = FindProperty("_Mode", props);
            _albedoMap = FindProperty("_MainTex", props);
            _specularMap = FindProperty("_SpecGlossMap", props, false);
            _metallicMap = FindProperty("_MetallicGlossMap", props, false);
            if (_specularMap != null)
                _mWorkflowMode = WorkflowMode.Specular;
            else if (_metallicMap != null)
                _mWorkflowMode = WorkflowMode.Metallic;
            else
                _mWorkflowMode = WorkflowMode.Dielectric;
            _bumpMap = FindProperty("_BumpMap", props);
            _heightMap = FindProperty("_ParallaxMap", props);
            _occlusionMap = FindProperty("_OcclusionMap", props);

            _emissionMap = FindProperty("_EmissionMap", props);
            _emissionColorMap = FindProperty("_EmissionColorMap", props);
            _emissionScale = FindProperty("_EmissionScale", props);

            _glsSmcParBusMap = FindProperty("_GlsSmcParBus", props);
            _bumpMapMax = FindProperty("_BumpMapMax", props);
            _cutOccGloMap = FindProperty("_CutOccGlo", props);

            _highlights = FindProperty("_SpecularHighlights", props, false);
            _reflections = FindProperty("_GlossyReflections", props, false);

            _atlasWidth = FindProperty("_AtlasWidth", props, false);
            _atlasHeight = FindProperty("_AtlasHeight", props, false);
            _maxMipLevel = FindProperty("_MaxMipLevel", props, false);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
            _materialEditor = materialEditor;
            Material material = materialEditor.target as Material;

            // Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing
            // material to a standard shader.
            // Do this before any GUI code has been issued to prevent layout issues in subsequent GUILayout statements (case 780071)
            if (_mFirstTimeApply)
            {
                MaterialChanged(material, _mWorkflowMode);
                _mFirstTimeApply = false;
            }

            ShaderPropertiesGUI();
        }

        public void ShaderPropertiesGUI()
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {
                BlendModePopup();

                // Primary properties
                GUILayout.Label(Styles.PrimaryMapsText, EditorStyles.boldLabel);
                _materialEditor.TexturePropertySingleLine(Styles.AlbedoText, _albedoMap, null);
                DoSpecularMetallicArea();
                _materialEditor.TexturePropertySingleLine(Styles.NormalMapText, _bumpMap,  _bumpMapMax);
                _materialEditor.TexturePropertySingleLine(Styles.HeightMapText, _heightMap, null);
                _materialEditor.TexturePropertySingleLine(Styles.OcclusionText, _occlusionMap, null);
                
                DoEmissionArea();
                
                EditorGUI.BeginChangeCheck();
                _materialEditor.TextureScaleOffsetProperty(_albedoMap);
                
                if (EditorGUI.EndChangeCheck())
                    _emissionMap.textureScaleAndOffset = _albedoMap.textureScaleAndOffset; // Apply the main texture scale and offset to the emission texture as well, for Enlighten's sake

                EditorGUILayout.Space();

                // Atlas section
                GUILayout.Label(Styles.AtlasText, EditorStyles.boldLabel);
                if (_atlasWidth != null)
                    _materialEditor.ShaderProperty(_atlasWidth, Styles.AtlasWidthText);
                if (_atlasHeight != null)
                    _materialEditor.ShaderProperty(_atlasHeight, Styles.AtlasHeightText);
                if (_maxMipLevel != null)
                    _materialEditor.ShaderProperty(_maxMipLevel, Styles.MaxMipMapText);
                
                EditorGUILayout.Space();

                // Third properties
                GUILayout.Label(Styles.BakedProperties, EditorStyles.boldLabel);
                _materialEditor.TexturePropertySingleLine(Styles.GlsSmcParBusText, _glsSmcParBusMap,  null);
                _materialEditor.TexturePropertySingleLine(Styles.CutOccGloText, _cutOccGloMap,  null);

                // Third properties
                GUILayout.Label(Styles.ForwardText, EditorStyles.boldLabel);
                if (_highlights != null)
                    _materialEditor.ShaderProperty(_highlights, Styles.HighlightsText);
                if (_reflections != null)
                    _materialEditor.ShaderProperty(_reflections, Styles.ReflectionsText);
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in _blendMode.targets)
                    MaterialChanged((Material)obj, _mWorkflowMode);
            }

            EditorGUILayout.Space();

            // NB renderqueue editor is not shown on purpose: we want to override it based on blend mode
            GUILayout.Label(Styles.AdvancedText, EditorStyles.boldLabel);
            _materialEditor.EnableInstancingField();
            _materialEditor.DoubleSidedGIField();
        }

        internal void DetermineWorkflow(MaterialProperty[] props)
        {
            if (FindProperty("_SpecGlossMap", props, false) != null)
                _mWorkflowMode = WorkflowMode.Specular;
            else if (FindProperty("_MetallicGlossMap", props, false) != null)
                _mWorkflowMode = WorkflowMode.Metallic;
            else
                _mWorkflowMode = WorkflowMode.Dielectric;
        }

        public override void AssignNewShaderToMaterial(Material material, UnityEngine.Shader oldShader, UnityEngine.Shader newShader)
        {
            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
                return;
            }

            BlendMode blendMode = BlendMode.Opaque;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                blendMode = BlendMode.Cutout;
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                blendMode = BlendMode.Fade;
            }
            material.SetFloat("_Mode", (float)blendMode);

            DetermineWorkflow(MaterialEditor.GetMaterialProperties(new Object[] { material }));
            MaterialChanged(material, _mWorkflowMode);
        }

        private void BlendModePopup()
        {
            EditorGUI.showMixedValue = _blendMode.hasMixedValue;
            var mode = (BlendMode)_blendMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (BlendMode)EditorGUILayout.Popup(Styles.RenderingMode, (int)mode, Styles.BLEND_NAMES);
            if (EditorGUI.EndChangeCheck())
            {
                _materialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                _blendMode.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;
        }

        private void DoEmissionArea()
        {
            // Texture and HDR color controls
            _materialEditor.TexturePropertySingleLine(Styles.EmissionText, _emissionMap, null);
            _materialEditor.TexturePropertySingleLine(Styles.EmissionColorText, _emissionColorMap, _emissionScale);

            // change the GI flag and fix it up with emissive as black if necessary
            Material[] materialArray = Array.ConvertAll(_materialEditor.targets, o => (Material) o);
            EditorGUI.BeginChangeCheck();
            EditorGUI.indentLevel += MaterialEditor.kMiniTextureFieldLabelIndentLevel;
            var illuminationFlags3 = (MaterialGlobalIlluminationFlags) EditorGUILayout.IntPopup(Styles.LightmapEmissiveLabel, (int) (materialArray[0].globalIlluminationFlags & MaterialGlobalIlluminationFlags.AnyEmissive), Styles.LightmapEmissiveStrings, Styles.LightmapEmissiveValues);
            EditorGUI.indentLevel -= MaterialEditor.kMiniTextureFieldLabelIndentLevel;
            bool changed = EditorGUI.EndChangeCheck();
            foreach (Material mat in materialArray)
            {
                if (changed)
                    mat.globalIlluminationFlags = illuminationFlags3;

                FixupEmissiveFlag(mat);
            }
        }

        private void DoSpecularMetallicArea()
        {
            switch (_mWorkflowMode)
            {
                case WorkflowMode.Specular:
                    _materialEditor.TexturePropertySingleLine(Styles.SpecularMapText, _specularMap, null);
                    break;
                case WorkflowMode.Metallic:
                    _materialEditor.TexturePropertySingleLine(Styles.MetallicMapText, _metallicMap, null);
                    break;
            }
        }

        public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    SetKeyword(material, "_ALPHATEST_ON", false);
                    SetKeyword(material, "_ALPHABLEND_ON", false);
                    SetKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
                    material.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    SetKeyword(material, "_ALPHATEST_ON", true);
                    SetKeyword(material, "_ALPHABLEND_ON", false);
                    SetKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
                    material.renderQueue = (int)RenderQueue.AlphaTest;
                    break;
                case BlendMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    SetKeyword(material, "_ALPHATEST_ON", false);
                    SetKeyword(material, "_ALPHABLEND_ON", true);
                    SetKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
                    material.renderQueue = (int)RenderQueue.Transparent;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    SetKeyword(material, "_ALPHATEST_ON", false);
                    SetKeyword(material, "_ALPHABLEND_ON", false);
                    SetKeyword(material, "_ALPHAPREMULTIPLY_ON", true);
                    material.renderQueue = (int)RenderQueue.Transparent;
                    break;
            }
        }

        private static void SetMaterialKeywords(Material material, WorkflowMode workflowMode)
        {
            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)
            SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));
            if (workflowMode == WorkflowMode.Specular)
                SetKeyword(material, "_SPECGLOSSMAP", material.GetTexture("_SpecGlossMap"));
            else if (workflowMode == WorkflowMode.Metallic)
                SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));
            SetKeyword(material, "_PARALLAXMAP", material.GetTexture("_ParallaxMap"));

            // A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
            // or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
            // The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.

            FixupEmissiveFlag(material);
        }

        private static void FixupEmissiveFlag(Material material)
        {
            Texture hasColorMap = material.GetTexture("_EmissionColorMap");
            MaterialGlobalIlluminationFlags flags = material.globalIlluminationFlags;
            if (!flags.HasFlag(MaterialGlobalIlluminationFlags.BakedEmissive) && hasColorMap == null)
                flags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            else if (flags != MaterialGlobalIlluminationFlags.EmissiveIsBlack)
                flags &= MaterialGlobalIlluminationFlags.AnyEmissive;

            if (flags != material.globalIlluminationFlags)
            {
                material.globalIlluminationFlags = flags;
                Debug.LogWarning($"Shader GUI changes emissive flag to {flags}");
            }
        }

        private static void MaterialChanged(Material material, WorkflowMode workflowMode)
        {
            SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));

            SetMaterialKeywords(material, workflowMode);
        }

        private static void SetKeyword(Material m, string keyword, bool state)
        {
            bool wasEnabled = m.IsKeywordEnabled(keyword);

            if (wasEnabled != state)
            {
                Debug.LogWarning($"Shader GUI modifies material keyword '{keyword}' to '{state}'. This should not happen unless something is changed by the user in this GUI.");
            }

            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }
    }
} // namespace UnityEditor
