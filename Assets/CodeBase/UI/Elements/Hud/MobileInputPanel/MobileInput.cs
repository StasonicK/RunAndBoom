using CodeBase.Services;
using CodeBase.Services.Input;
using UnityEngine;

namespace CodeBase.UI.Elements.Hud.MobileInputPanel
{
    public class MobileInput : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;

        private void Awake() =>
            _panel.SetActive(AllServices.Container.Single<IInputService>() is MobileInputService);
    }
}