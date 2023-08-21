using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JustAssets.AtlasMapPacker.Rendering
{
    public readonly struct MaterialConfiguration
    {
        public IShaderInfo ShaderInfo { get; }

        public UnityEngine.Shader Shader { get; }

        public bool Equals(MaterialConfiguration other)
        {
            return DisplayName == other.DisplayName && Details.SequenceEqual(other.Details);
        }

        public override bool Equals(object obj)
        {
            return obj is MaterialConfiguration other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DisplayName != null ? DisplayName.GetHashCode() : 0;

                if (Details != null)
                    foreach (var detailsValue in Details)
                        hashCode = (hashCode * 397) ^ (detailsValue?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        public string DisplayName { get; }

        public MaterialConfiguration(ShaderInfoFactory shaderInfoFactory, Material material)
        {
            Shader = material.shader;
            ShaderInfo = shaderInfoFactory.Create(Shader.name);
            Details = ShaderInfo.EvaluateNaming(material);
            
            var replacements = new[] {(" ", "_"), (")", ""), ("(", ""), ("\\", "_"), ("/", "_")};

            var shaderName = Shader.name;
            foreach (var (a, b) in replacements)
                shaderName = shaderName.Replace(a, b);

            DisplayName = shaderName;

            if (Details.Count > 0)
                DisplayName += $"_{string.Join("_", Details)}";
        }

        public List<string> Details { get; }

        public static void CopyAttributesToMesh(List<AttributePointerPair> shaderAttributesToTransfer, Material sourceData, Mesh targetMesh)
        {
            var usedChannels = new Dictionary<string, DataSource>();
            Dictionary<string, Vector4> channelDatas = new Dictionary<string, Vector4>();

            // Get vertex data mappings
            var attributes = shaderAttributesToTransfer.Where(x =>
                    x.Target != null && x.Target.DataSource >= DataSource.Color &&
                    x.Target.DataSource <= DataSource.UV7)
                .ToList();

            foreach (var attribute in attributes)
            {
                var valueVertexChannel = attribute.Target.DataSource + attribute.Target.Name;
                if (!channelDatas.ContainsKey(valueVertexChannel))
                    channelDatas[valueVertexChannel] = Vector4.zero;

                usedChannels[valueVertexChannel] = attribute.Target.DataSource;

                var channelData = channelDatas[valueVertexChannel];
                switch (attribute.Source.Component)
                {
                    case ValueComponent.All:
                        channelDatas[valueVertexChannel] = sourceData.GetVector(attribute.Source.Name);
                        break;
                    case ValueComponent.X:
                    {
                        var temp = channelData;
                        temp.x = sourceData.GetFloat(attribute.Source.Name);
                        channelDatas[valueVertexChannel] = temp;
                        break;
                    }
                    case ValueComponent.Y:
                    {
                        var temp = channelData;
                        temp.y = sourceData.GetFloat(attribute.Source.Name);
                        channelDatas[valueVertexChannel] = temp;
                        break;
                    }
                    case ValueComponent.Z:
                    {
                        var temp = channelData;
                        temp.z = sourceData.GetFloat(attribute.Source.Name);
                        channelDatas[valueVertexChannel] = temp;
                        break;
                    }
                } 
            }

            var uvLength = targetMesh.uv.Length;

            foreach (var usedChannel in usedChannels)
            {
                var channelData = channelDatas[usedChannel.Key];
                if (usedChannel.Value == DataSource.Color)
                {
                    var colorArray = new Color32[uvLength];
                    for (int i = 0; i < colorArray.Length; i++)
                    {
                        colorArray[i] = new Color(channelData.x, channelData.y, channelData.z, channelData.w);
                    }

                    targetMesh.colors32 = colorArray;
                }
                else if (usedChannel.Value == DataSource.TextureAttribute)
                {
                    //TODO
                }
                else
                {
                    var dataArray = new List<Vector4>(uvLength);
                    for (int i = 0; i < uvLength; i++)
                    {
                        dataArray.Add(channelData);
                    }

                    targetMesh.SetUVs((int) usedChannel.Value, dataArray);
                }
            }
        }
    }
}