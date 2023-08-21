using System.Collections.Generic;

namespace JustAssets.AtlasMapPacker.Rendering
{
    public class ShaderInfoFactory
    {
        private readonly Dictionary<string, IShaderInfo> _configCache = new Dictionary<string, IShaderInfo>();

        public IShaderInfo Create(string shaderName)
        {
            if (!_configCache.TryGetValue(shaderName, out var shaderInfo))
            {
                shaderInfo = _configCache[shaderName] = new XmlShaderInfo(shaderName);
            }

            return shaderInfo;
        }
    }
}