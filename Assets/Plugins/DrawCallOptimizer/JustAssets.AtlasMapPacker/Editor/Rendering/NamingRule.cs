using UnityEngine;

namespace JustAssets.AtlasMapPacker.Rendering
{
    public class NamingRule
    {
        public string AttributeName { get; set; }

        public NamingRuleType Type { get; set; }

        public string Evaluate(Material material)
        {
            switch (Type)
            {
                case NamingRuleType.FloatAttribute:
                    if (!material.HasProperty(AttributeName))
                        return null;

                    return $"{AttributeName.Replace("_", "")}_{material.GetFloat(AttributeName):F0}";
                case NamingRuleType.EmissionCheck:
                    return EmissionCheck(material);
            }

            return null;
        }

        private static string EmissionCheck(Material material)
        {
            var name = "_EmissionColor";
            if (material.globalIlluminationFlags.HasFlag(MaterialGlobalIlluminationFlags.EmissiveIsBlack) &&
                material.HasProperty(name))
                if (material.GetColor(name).maxColorComponent != 0)
                    material.SetColor(name, Color.black);

            if (material.globalIlluminationFlags.HasFlag(MaterialGlobalIlluminationFlags.RealtimeEmissive) &&
                !material.globalIlluminationFlags.HasFlag(MaterialGlobalIlluminationFlags.EmissiveIsBlack))
                return "RealtimeEmissive";

            return null;
        }
    }
}