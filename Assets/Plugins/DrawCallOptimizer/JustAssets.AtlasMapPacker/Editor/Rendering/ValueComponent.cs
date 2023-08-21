using System;

namespace JustAssets.AtlasMapPacker.Rendering
{
    [Flags]
    public enum ValueComponent
    {
        X = 1,

        Y = 2,

        Z = 4,

        W = 8,

        All = X | Y | Z | W
    }
}