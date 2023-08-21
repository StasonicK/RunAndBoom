using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering;
#else
using ShaderPropertyType = UnityEditor.ShaderUtil.ShaderPropertyType;
#endif

namespace JustAssets.AtlasMapPacker.Rendering
{
    public class XmlShaderInfo : IShaderInfo
    {
        private readonly string _shaderName;

        private ShaderInfoData _data;

        private string _filePath;

        public XmlShaderInfo(string shaderName)
        {
            _shaderName = shaderName;

            ReloadAsset();
            _filePath = Path.GetFullPath($"{ShaderUtil.BasePath}/ShaderDefinitions/{_shaderName}.xml");

            var directoryName = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            if (File.Exists(_filePath))
                ReadInstructions(_filePath);
            else
            {
                CreateNewInstructions(UnityEngine.Shader.Find(shaderName));
                WriteInstructions(_filePath);
            }
        }

        public TextAsset Asset { get; private set; }

        public virtual string TargetShaderName
        {
            get => _data.TargetShaderName;
            set => _data.TargetShaderName = value;
        }

        public List<AttributePointerPair> ShaderAttributesToTransfer => _data.AttributeTransferInstructions;

        public List<KeywordTransferInstruction> KeywordTransferInstructions => _data.KeywordTransferInstructions;

        public List<RenderTypeSelectionInstruction> RenderTypeMapping => _data.RenderTypeMapping;

        public List<RenderQueueSelectionInstruction> RenderQueueMapping => _data.RenderQueueMapping;

        public List<NamingRule> NamingRules => _data.NamingRule;

        public List<string> EvaluateNaming(Material material)
        {
            var result = new List<string>();
            foreach (NamingRule namingRule in NamingRules)
            {
                var ruleResult = namingRule.Evaluate(material);

                if (ruleResult != null)
                    result.Add(ruleResult);
            }

            return result;
        }

        public TextAsset Save()
        {
            WriteInstructions(_filePath);
            ReloadAsset();
            return Asset;
        }

        public void SetReplacements(Dictionary<string, (string Name, int Component)> replacements)
        {
            foreach (var replacement in replacements)
            {
                AttributePointerPair attribute = ShaderAttributesToTransfer.FirstOrDefault(x => x.Source.Name == replacement.Key);
                if (attribute == null)
                    continue;

                attribute.Target.Name = replacement.Value.Name;
                attribute.Target.Component = (ValueComponent) replacement.Value.Component;
            }
        }

        private void CreateNewInstructions(UnityEngine.Shader shader)
        {
            _data = new ShaderInfoData();

            var targetSlots = shader.GetProperties().ToList();
            targetSlots.Sort((a, b) => b.Type.CompareTo(a.Type));

            foreach (UnityExtension.ShaderProperty shaderProperty in targetSlots)
            {
                _data.AttributeTransferInstructions.Add(new AttributePointerPair
                {
                    Source = new AttributePointer(shaderProperty.Name, ToDataSource(shaderProperty.Type)),
                    Target = new AttributeTargetPointer(shaderProperty.Name, DataSource.TextureAttribute)
                });
            }
        }

        private void ReadInstructions(string filePath)
        {
            var serializer = new XmlSerializer(typeof(ShaderInfoData));
            using (FileStream fileStream = File.OpenRead(filePath))
                _data = (ShaderInfoData) serializer.Deserialize(fileStream);
        }

        private void ReloadAsset()
        {
            var localAssetPath = $"{ShaderUtil.BasePath}/ShaderDefinitions/{_shaderName}.xml";
            AssetDatabase.ImportAsset(localAssetPath);
            Asset = AssetDatabase.LoadAssetAtPath<TextAsset>(localAssetPath);
        }

        private DataSource ToDataSource(ShaderPropertyType shaderPropertyType)
        {
            switch (shaderPropertyType)
            {
                case ShaderPropertyType.Color:
                case ShaderPropertyType.Vector:
                    return DataSource.VectorAttribute;
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    return DataSource.FloatAttribute;
                default:
                    return DataSource.TextureAttribute;
            }
        }

        private void WriteInstructions(string filePath)
        {
            var serializer = new XmlSerializer(typeof(ShaderInfoData));
            using (FileStream fileStream = File.Create(filePath))
                serializer.Serialize(fileStream, _data);
        }
    }
}