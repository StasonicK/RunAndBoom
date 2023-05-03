﻿using System;
using CodeBase.Data;
using CodeBase.Infrastructure.States;
using CodeBase.Services;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.SaveLoad;
using CodeBase.UI.Windows.Common;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.Finish
{
    public class FinishButtons : MonoBehaviour
    {
        [SerializeField] private Button _addCoinsButton;
        [SerializeField] private Button _toNextLevelButton;
        [SerializeField] private FinishWindow _finishWindow;
        [SerializeField] private ItemsGeneratorBase _generator;

        private ISaveLoadService _saveLoadService;
        private IGameStateMachine _gameStateMachine;
        private Scene _scene;

        private void Awake()
        {
            _saveLoadService = AllServices.Container.Single<ISaveLoadService>();
            _gameStateMachine = AllServices.Container.Single<IGameStateMachine>();
            _toNextLevelButton.onClick.AddListener(ToNextLevel);
            _generator.GenerationStarted += DisableRefreshButtons;
            _generator.GenerationEnded += CheckRefreshButtons;
        }

        private void OnEnable()
        {
            _toNextLevelButton.onClick.AddListener(ToNextLevel);
        }

        private void OnDisable()
        {
            _toNextLevelButton.onClick.RemoveListener(ToNextLevel);
        }

        public void Construct(Scene scene)
        {
            _scene = scene;
            GenerateItems();
        }

        private void DisableRefreshButtons()
        {
        }

        private void CheckRefreshButtons()
        {
        }

        private void Start() =>
            Cursor.lockState = CursorLockMode.Confined;

        private void ToNextLevel()
        {
            _saveLoadService.SaveProgress();
            _gameStateMachine.Enter<LoadPlayerProgressState, Scene>(_scene);
            CloseWindow();
        }

        private void CloseWindow()
        {
            _finishWindow.Hide();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void ShowAds()
        {
            //TODO ShowAds screen
        }

        private void GenerateItems() =>
            _generator.Generate();
    }
}