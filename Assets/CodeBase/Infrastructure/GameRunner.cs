using NTC.Global.Cache;

namespace CodeBase.Infrastructure
{
    public class GameRunner : MonoCache
    {
        public GameBootstrapper BootstrapperPrefab;

        private void Awake()
        {
            var bootstrapper = FindObjectOfType<GameBootstrapper>();

            if (bootstrapper != null) 
                return;

            Instantiate(BootstrapperPrefab);
        }
    }
}