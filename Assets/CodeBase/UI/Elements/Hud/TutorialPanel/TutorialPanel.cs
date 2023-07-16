﻿using CodeBase.Services;
using CodeBase.Services.Input;
using CodeBase.UI.Elements.Hud.TutorialPanel.InnerPanels;
using CodeBase.UI.Services;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements.Hud.TutorialPanel
{
    public class TutorialPanel : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private Settings _settings;
        [SerializeField] private Look _look;
        [SerializeField] private Movement _movement;
        [SerializeField] private Shoot _shoot;
        [SerializeField] private InnerPanels.Weapons _weapons;

        private IInputService _inputService;
        private float _visibleTransparentValue = 0.07058824f;
        private bool _hidden;

        private void Awake()
        {
            _hidden = false;
            _inputService = AllServices.Container.Single<IInputService>();
        }

        private void Start()
        {
            if (_inputService is MobileInputService)
            {
                _look.ShowForMobile();
                _settings.ShowForMobile();
                _movement.ShowForMobile();
                _shoot.ShowForMobile();
                _weapons.ShowForMobile();
            }
            else
            {
                _look.ShowForPc();
                _settings.ShowForPc();
                _movement.ShowForPc();
                _shoot.ShowForPc();
                _weapons.ShowForPc();
            }

            _background.ChangeImageAlpha(_visibleTransparentValue);
        }

        private void Update()
        {
            if (_hidden)
                return;

            if (_inputService.IsAttackButtonUp())
                HidePanel();

            if (_inputService is MobileInputService && _inputService.LookAxis.magnitude > Constants.Epsilon)
                HidePanel();

            if (_inputService is MobileInputService && _inputService.MoveAxis.magnitude > Constants.Epsilon)
                HidePanel();

            if (Input.GetKeyDown(KeyCode.W))
                HidePanel();

            if (Input.GetKeyDown(KeyCode.S))
                HidePanel();

            if (Input.GetKeyDown(KeyCode.A))
                HidePanel();

            if (Input.GetKeyDown(KeyCode.D))
                HidePanel();

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) ||
                Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4))
                HidePanel();

            if (Input.GetKeyDown(KeyCode.Mouse0))
                HidePanel();

            if (Input.GetKeyDown(KeyCode.Escape))
                HidePanel();
        }

        public void HidePanel()
        {
            if (_hidden)
                return;

            _settings.Hide();
            _look.Hide();
            _movement.Hide();
            _shoot.Hide();
            _weapons.Hide();
            _background.ChangeImageAlpha(Constants.Invisible);
            _hidden = true;
        }
    }
}