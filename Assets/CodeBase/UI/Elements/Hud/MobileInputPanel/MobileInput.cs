using CodeBase.Services;
using CodeBase.Services.Input;
using UnityEngine;

namespace CodeBase.UI.Elements.Hud.MobileInputPanel
{
    public class MobileInput : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private VariableJoystick _moveJoystick;
        [SerializeField] private VariableJoystick _lookJoystick;

        public VariableJoystick MoveJoystick => _moveJoystick;
        public VariableJoystick LookJoystick => _lookJoystick;

        private void Awake() =>
            _panel.SetActive(AllServices.Container.Single<IInputService>() is MobileInputService);
    }
}