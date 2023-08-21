namespace JustAssets.ShaderPatcher
{
    public static class CastExtension
    {
        public static T Cast<T>(this object that)
        {
            return (T) that;
        }
        public static T As<T>(this object that) where T : class
        {
            return that as T;
        }
    }
}