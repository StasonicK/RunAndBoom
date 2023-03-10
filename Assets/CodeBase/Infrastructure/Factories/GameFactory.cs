﻿using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Data;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.Registrator;
using CodeBase.Services.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.Factories
{
    public class GameFactory : IGameFactory
    {
        private const float Yaddition = 0.5f;
        private readonly IAssets _assets;
        private readonly IPlayerProgressService _progressService;
        private readonly IStaticDataService _staticData;
        private readonly IRegistratorService _registratorService;
        private GameObject _heroGameObject;

        public List<IProgressReader> ProgressReaders { get; set; } = new List<IProgressReader>();
        public List<IProgressSaver> ProgressWriters { get; set; } = new List<IProgressSaver>();

        public GameFactory(IAssets assets, IPlayerProgressService progressService, IStaticDataService staticData,
            IRegistratorService registratorService)
        {
            _assets = assets;
            _progressService = progressService;
            _staticData = staticData;
            _registratorService = registratorService;

            SetProgressReadersWriters(registratorService);
        }

        public GameObject GetHero() =>
            _heroGameObject;

        private void SetProgressReadersWriters(IRegistratorService registratorService)
        {
            ProgressReaders = registratorService.ProgressReaders;
            ProgressWriters = registratorService.ProgressWriters;
        }

        public async Task WarmUp()
        {
            _assets.Initialize();
            // await _assets.Load<GameObject>(AssetAddresses.Spawner);
        }

        public async Task<GameObject> CreateHero(Vector3 at)
        {
            _heroGameObject = await _registratorService.InstantiateRegisteredAsync(AssetAddresses.Hero, at.AddY(Yaddition));
            _registratorService.RegisterProgressWatchers(_heroGameObject);
            return _heroGameObject;
        }

        public void CleanUp()
        {
            ProgressReaders.Clear();
            ProgressWriters.Clear();

            _assets.CleanUp();
        }
    }
}