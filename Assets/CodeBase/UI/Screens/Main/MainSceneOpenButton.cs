using CodeBase.Data;
using CodeBase.Infrastructure.States;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.SaveLoad;
using CodeBase.UI.Elements;
using CodeBase.UI.Services.Windows;
using Zenject;

namespace CodeBase.UI.Screens.Main
{
    public class MainSceneOpenButton : SceneOpenButton
    {
        protected override string Scene => Scenes.Main;
        protected override bool Checked => true;

        [Inject]
        public void Construct(IGameStateMachine stateMachine, IPlayerProgressService progressService, IWindowService windowService,
            ISaveLoadService saveLoadService)
        {
            base.Construct(stateMachine, progressService, windowService, saveLoadService);
        }
    }
}