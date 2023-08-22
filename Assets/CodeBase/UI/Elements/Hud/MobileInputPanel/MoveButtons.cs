using System;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements.Hud.MobileInputPanel
{
    public class MoveButtons : MonoBehaviour
    {
        [SerializeField] private Button _leftButton;
        [SerializeField] private Button _rightButton;
        [SerializeField] private Button _upButton;
        [SerializeField] private Button _downButton;

        public event Action LeftPressed;
        public event Action RightPressed;
        public event Action UpPressed;
        public event Action DownPressed;

        private void OnEnable()
        {
            _leftButton.onClick.AddListener(PressedLeft);
            _rightButton.onClick.AddListener(PressedRight);
            _upButton.onClick.AddListener(PressedUp);
            _downButton.onClick.AddListener(PressedDown);
        }

        private void PressedLeft() =>
            LeftPressed?.Invoke();

        private void PressedRight() =>
            RightPressed?.Invoke();

        private void PressedUp() =>
            UpPressed?.Invoke();

        private void PressedDown() =>
            DownPressed?.Invoke();
    }
}