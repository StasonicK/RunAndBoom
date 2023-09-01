using CodeBase.Services;
using CodeBase.Services.Input;
using CodeBase.UI.Elements.Hud.TutorialPanel.InnerPanels;
using CodeBase.UI.Services;
using NTC.Global.Cache;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements.Hud.TutorialPanel
{
    public class TutorialPanel : MonoCache
    {
        [SerializeField] private Image _background;
        [SerializeField] private Settings _settings;
        [SerializeField] private Look _look;
        [SerializeField] private Movement _movement;
        [SerializeField] private Shoot _shoot;
        [SerializeField] private InnerPanels.Weapons _weapons;
        [SerializeField] private LeaderBoard _leaderBoard;
        [SerializeField] public bool _hideAtStage;

        private IInputService _inputService;
        private float _visibleTransparentValue = 0.07058824f;
        private bool _hidden;

        private void Awake()
        {
            _inputService = AllServices.Container.Single<IInputService>();
            _hidden = false;
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
                _leaderBoard.ShowForMobile();
            }
            else
            {
                _look.ShowForPc();
                _settings.ShowForPc();
                _movement.ShowForPc();
                _shoot.ShowForPc();
                _weapons.ShowForPc();
                _leaderBoard.ShowForPc();
            }

            _background.ChangeImageAlpha(_visibleTransparentValue);
        }

        protected override void Run()
        {
            if (_hideAtStage)
                return;

            if (_hidden)
                return;

            // if (_inputService.IsAttackButtonUp())
            //     HidePanel();
            //
            // if (_inputService is MobileInputService && _inputService.LookAxis.magnitude > Constants.RotationEpsilon)
            //     HidePanel();
            //
            // if (_inputService is MobileInputService && _inputService.MoveAxis.magnitude > Constants.MovementEpsilon)
            //     HidePanel();
            //
            // if (Input.GetKeyDown(KeyCode.W))
            //     HidePanel();
            //
            // if (Input.GetKeyDown(KeyCode.S))
            //     HidePanel();
            //
            // if (Input.GetKeyDown(KeyCode.A))
            //     HidePanel();
            //
            // if (Input.GetKeyDown(KeyCode.D))
            //     HidePanel();
            //
            // if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) ||
            //     Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4))
            //     HidePanel();
            //
            // if (Input.GetKeyDown(KeyCode.Mouse0))
            //     HidePanel();
            //
            // if (Input.GetKeyDown(KeyCode.Escape))
            //     HidePanel();
        }

        public void HidePanel()
        {
            if (_hideAtStage)
                return;

            ForceHidePanel();
        }

        public void ForceHidePanel()
        {
            if (_hidden)
                return;

            _settings.Hide();
            _look.Hide();
            _movement.Hide();
            _shoot.Hide();
            _weapons.Hide();
            _leaderBoard.Hide();
            _background.ChangeImageAlpha(Constants.Invisible);
            _hidden = true;
            _hideAtStage = false;
        }
    }
}