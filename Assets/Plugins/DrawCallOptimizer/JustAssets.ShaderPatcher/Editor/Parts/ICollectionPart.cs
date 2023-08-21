using System.Collections.Generic;

namespace JustAssets.ShaderPatcher.Parts
{
    public interface ICollectionPart : IPart
    {
        IList<IPart> Parts { get; }
    }
}