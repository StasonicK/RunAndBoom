﻿using System.Collections;
using Agava.YandexGames;
using CodeBase.Data;
using CodeBase.Services;
using CodeBase.Services.PlayerAuthorization;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.LeaderBoard
{
    public class LeaderBoardWindow : WindowBase
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private RawImage _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private GameObject _leaderBoard;
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private GameObject _playerContainer;

        private IAuthorization _authorization;
        private Scene _nextScene;
        private int _maxPrice;

        private void Start()
        {
            _leaderBoard.SetActive(false);
            _rankText.text = "";
            _iconImage.texture = null;
            _nameText.text = "";
            _scoreText.text = "";
            _playerContainer.SetActive(false);
        }

        private void OnEnable()
        {
            ClearLeaderBoard();
            _closeButton.onClick.AddListener(Close);

            if (Application.isEditor)
                return;

            if (AdsService != null)
            {
                AdsService.OnInitializeSuccess += AdsServiceInitializedSuccess;
                InitializeAdsSDK();
            }

            if (_authorization == null)
                _authorization = AllServices.Container.Single<IAuthorization>();

            _authorization.OnErrorCallback += ShowError;
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(Close);

            if (AdsService != null)
                AdsService.OnInitializeSuccess -= AdsServiceInitializedSuccess;

            if (_authorization != null)
                _authorization.OnErrorCallback -= ShowError;
        }

        public void Construct(GameObject hero) =>
            base.Construct(hero, WindowId.LeaderBoard);

        private void Close() =>
            gameObject.SetActive(false);

        protected override void AdsServiceInitializedSuccess()
        {
            Debug.Log("TryAuthorize");
            if (_authorization.IsAuthorized())
            {
                _authorization.OnAuthorizeSuccessCallback += RequestPersonalProfileDataPermission;
                RequestPersonalProfileDataPermission();
            }
            else
            {
                Authorize();
            }
        }

        private void RequestLeaderBoardData()
        {
            Debug.Log($"RequestLeaderBoardData");
            LeaderBoardService.OnInitializeSuccess -= RequestLeaderBoardData;
            LeaderBoardService.OnSuccessGetEntries += FillLeaderBoard;
            LeaderBoardService.GetEntries(Progress.Stats.CurrentLevelStats.Scene.GetLeaderBoardName());

            LeaderBoardService.OnSuccessGetEntry += FillPlayerInfo;
            LeaderBoardService.GetPlayerEntry(Progress.Stats.CurrentLevelStats.Scene.GetLeaderBoardName());
        }

        private void Authorize()
        {
            Debug.Log("Authorize");
            _authorization.OnAuthorizeSuccessCallback += RequestPersonalProfileDataPermission;
            _authorization.OnErrorCallback += ShowError;
            _authorization.Authorize();
        }

        private void RequestPersonalProfileDataPermission()
        {
            Debug.Log("RequestPersonalProfileDataPermission");
            _authorization.OnRequestPersonalProfileDataPermissionSuccessCallback += InitializeLeaderBoard;
            _authorization.OnAuthorizeSuccessCallback -= RequestPersonalProfileDataPermission;
            _authorization.OnErrorCallback += ShowError;
            _authorization.RequestPersonalProfileDataPermission();
        }

        private void InitializeLeaderBoard()
        {
            Debug.Log("InitializeLeaderBoard");
            _authorization.OnRequestPersonalProfileDataPermissionSuccessCallback -= InitializeLeaderBoard;
            LeaderBoardService.OnInitializeSuccess += RequestLeaderBoardData;

            if (LeaderBoardService.IsInitialized())
                RequestLeaderBoardData();
            else
                StartCoroutine(LeaderBoardService.Initialize());
        }

        private void ShowError(string error)
        {
            Debug.Log($"ServiceAuthorization ShowError {error}");
            _authorization.OnErrorCallback -= ShowError;
        }

        private void FillPlayerInfo(LeaderboardEntryResponse response)
        {
            Debug.Log("FillPlayerInfo");
            Debug.Log($"FillPlayerInfo rank {response.rank}");
            Debug.Log($"FillPlayerInfo publicName {response.player.publicName}");
            Debug.Log($"FillPlayerInfo score {response.score}");
            _rankText.text = $"#{response.rank}";
            StartCoroutine(LoadAvatar(response.player.scopePermissions.avatar, _iconImage, _playerContainer));
            _nameText.text = response.player.publicName;
            _scoreText.text = response.score.ToString();
            LeaderBoardService.OnSuccessGetEntry -= FillPlayerInfo;
        }

        private void FillLeaderBoard(LeaderboardGetEntriesResponse leaderboardGetEntriesResponse)
        {
            Debug.Log("FillLeaderBoard");
            LeaderboardEntryResponse[] leaderboardEntryResponses = leaderboardGetEntriesResponse.entries;
            Debug.Log($"entries count {leaderboardGetEntriesResponse.entries.Length}");

            foreach (var response in leaderboardEntryResponses)
            {
                var player = Instantiate(_playerPrefab, _leaderBoard.transform);
                player.SetActive(false);
                Debug.Log($"FillLeaderBoard rank {response.rank}");
                Debug.Log($"FillLeaderBoard publicName {response.player.publicName}");
                Debug.Log($"FillLeaderBoard score {response.score}");
                player.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    response.rank.ToString();
                RawImage image = player.transform.GetChild(1).GetComponent<RawImage>();
                StartCoroutine(LoadAvatar(response.player.scopePermissions.avatar, image, player));
                player.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text =
                    response.player.publicName;
                player.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text =
                    response.score.ToString();
            }

            LeaderBoardService.OnSuccessGetEntries -= FillLeaderBoard;
            _leaderBoard.SetActive(true);
        }

        private void ClearLeaderBoard()
        {
            if (_leaderBoard.transform.childCount > 0)
            {
                foreach (Transform child in _leaderBoard.transform)
                    Destroy(child.gameObject);
            }
        }

        private IEnumerator LoadAvatar(string avatarUrl, RawImage image, GameObject gameObject)
        {
            Debug.Log("LoadAvatar started");
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(avatarUrl);
            yield return request.SendWebRequest();

            if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log($"LoadAvatar {request.error}");
            }
            else
            {
                image.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                gameObject.SetActive(true);
            }

            Debug.Log("LoadAvatar finished");
        }
    }
}