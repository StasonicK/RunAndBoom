using CodeBase.Services;
using CodeBase.UI.Elements.Hud.TutorialPanel;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.UI.Services.Factory
{
    public interface IUIFactory : IService
    {
        UniTask CreateUIRoot();
        Transform GetUIRoot();
        UniTask<GameObject> CreateHud(GameObject hero);
        UniTask<GameObject> CreateShopWindow();
        UniTask<GameObject> CreateDeathWindow();
        UniTask<GameObject> CreateSettingsWindow();
        UniTask<GameObject> CreateGiftsWindow();
        UniTask<GameObject> CreateResultsWindow();
        UniTask<GameObject> CreateAuthorizationWindow();
        UniTask<GameObject> CreateLeaderBoardWindow();
        UniTask<GameObject> CreateGameEndWindow();
        UniTask<GameObject> CreateStartWindow();
        UniTask<GameObject> CreateStartNewGameWindow();
        TutorialPanel GetTutorialPanel();
    }
}