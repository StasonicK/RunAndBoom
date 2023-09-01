using NTC.Global.Cache;

namespace CodeBase.Infrastructure
{
    public class CoroutineRunner : MonoCache, ICoroutineRunner
    {
        private void Awake() =>
            DontDestroyOnLoad(this);
    }
}