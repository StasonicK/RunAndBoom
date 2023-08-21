using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JustAssets.AtlasMapPacker.Rendering
{
    public struct Condition
    {
        public Condition(string attributeName, ConditionFilter conditionFilter, float value = 0)
        {
            AttributeName = attributeName;
            ConditionFilter = conditionFilter;
            Value = value;
        }

        public string AttributeName { get; set; }

        public ConditionFilter ConditionFilter { get; set; }

        public float Value { get; set; }

        public bool IsTrue(IEnumerable<Material> material)
        {
            switch (ConditionFilter)
            {
                case ConditionFilter.Nothing:
                    return true;
                case ConditionFilter.AttributeSet:
                    return Value > 0;
                case ConditionFilter.Any:
                {
                    Condition tmpThis = this;
                    return material.Any(x => x.HasProperty(tmpThis.AttributeName) && x.GetTexture(tmpThis.AttributeName) != null);
                }
                case ConditionFilter.Equals:
                    return Math.Abs(material.First().GetFloat(AttributeName) - Value) < 0.0001f;
            }

            return false;
        }
    }
}