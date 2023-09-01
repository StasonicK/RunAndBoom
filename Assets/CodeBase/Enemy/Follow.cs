using NTC.Global.Cache;

namespace CodeBase.Enemy
{
    public abstract class Follow : MonoCache
    {
        public abstract void Move();
        public abstract void Stop();
    }
}