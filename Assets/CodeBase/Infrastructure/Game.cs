using CodeBase.Data.Settings;
using CodeBase.Infrastructure.States;
using CodeBase.Services;
using NTC.Global.Cache;

namespace CodeBase.Infrastructure
{
    public class Game : MonoCache
    {
        public readonly GameStateMachine StateMachine;

        public Game(ICoroutineRunner coroutineRunner, ILoadingCurtain loadingCurtain, IAdListener adListener,
            Language language)
        {
            StateMachine =
                new GameStateMachine(new SceneLoader(coroutineRunner), loadingCurtain, adListener,
                    AllServices.Container, language);
        }
    }
}