using System.Threading.Tasks;
using CodeBase.Services;
using CodeBase.UI.Elements.Hud.TutorialPanel;
using UnityEngine;

namespace CodeBase.UI.Services.Factory
{
    public interface IUIFactory : IService
    {
        Task CreateUIRoot();
        Transform GetUIRoot();
        Task<GameObject> CreateHud(GameObject hero);
        Task<GameObject> CreateShopWindow();
        Task<GameObject> CreateDeathWindow();
        Task<GameObject> CreateSettingsWindow();
        Task<GameObject> CreateGiftsWindow();
        Task<GameObject> CreateResultsWindow();
        Task<GameObject> CreateAuthorizationWindow();
        Task<GameObject> CreateLeaderBoardWindow();
        Task<GameObject> CreateGameEndWindow();
        TutorialPanel GetTutorialPanel();
    }
}