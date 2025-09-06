using System.Collections;
using BananaParty.YandexGames;
using CodeBase.Data;
using CodeBase.Data.Progress;
using CodeBase.Services.Audio;
using CodeBase.UI.Elements.Hud;
using CodeBase.UI.Elements.Hud.MobileInputPanel;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Common;
using CodeBase.UI.Windows.GameFinished;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.LeaderBoard
{
    public class LeaderBoardWindow : WindowBase
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _toGameEndWindowButton;
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private RawImage _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private GameObject[] _players;
        [SerializeField] private GameObject _playerDataContainer;

        private SceneId _nextScene;
        private int _maxPrice;
        private bool _isCurrentScene = true;

        private void OnEnable()
        {
            ClearLeaderBoard();
            ClearPlayerData();
            _closeButton.onClick.AddListener(Close);
            _toGameEndWindowButton.onClick.AddListener(ToGameEndWindow);
            ActivateButtons();

            if (Application.isEditor || _leaderBoardService == null || ProgressData == null)
            {
                AddTestData();
                return;
            }

            _leaderBoardService.OnInitializeSuccess += RequestLeaderBoard;
            InitializeLeaderBoard();
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(Close);
            _toGameEndWindowButton.onClick.RemoveListener(ToGameEndWindow);

            if (_adsService != null)
                _adsService.OnInitializeSuccess -= RequestLeaderBoard;
        }

        public void Construct(GameObject hero, OpenSettings openSettings, MobileInput mobileInput) =>
            base.Construct(hero, WindowId.LeaderBoard, openSettings, mobileInput);

        public void SetGameLeaderBoard()
        {
            _isCurrentScene = false;
            ActivateButtons();
        }

        private void ActivateButtons()
        {
            if (_isCurrentScene)
            {
                _closeButton.gameObject.SetActive(true);
                _toGameEndWindowButton.gameObject.SetActive(false);
            }
            else
            {
                _closeButton.gameObject.SetActive(false);
                _toGameEndWindowButton.gameObject.SetActive(true);
            }
        }

        protected override void RequestLeaderBoard()
        {
            base.RequestLeaderBoard();
            GetLeaderBoardData();
        }

        private void ClearPlayerData()
        {
            _rankText.text = "";
            _iconImage.texture = null;
            _nameText.text = "";
            _scoreText.text = "";
            _playerDataContainer.SetActive(false);
        }

        private void AddTestData()
        {
            LeaderboardEntryResponse leaderboardEntryResponse1 = new LeaderboardEntryResponse();
            leaderboardEntryResponse1.rank = 2;
            leaderboardEntryResponse1.score = 500;
            PlayerAccountProfileDataResponse playerAccountProfileDataResponse1 = new PlayerAccountProfileDataResponse();
            playerAccountProfileDataResponse1.publicName = "KuKu";
            leaderboardEntryResponse1.player = playerAccountProfileDataResponse1;
            FillPlayerInfo(leaderboardEntryResponse1);

            LeaderboardEntryResponse leaderboardEntryResponse2 = new LeaderboardEntryResponse();
            leaderboardEntryResponse2.rank = 1;
            leaderboardEntryResponse2.score = 300;
            PlayerAccountProfileDataResponse playerAccountProfileDataResponse2 = new PlayerAccountProfileDataResponse();
            playerAccountProfileDataResponse2.publicName = "Lilo";
            leaderboardEntryResponse2.player = playerAccountProfileDataResponse2;

            LeaderboardGetEntriesResponse leaderboardGetEntriesResponse = new LeaderboardGetEntriesResponse();
            leaderboardGetEntriesResponse.entries = new[] { leaderboardEntryResponse1, leaderboardEntryResponse2 };
            FillLeaderBoard(leaderboardGetEntriesResponse);
        }

        private void Close() =>
            base.Close();

        private void ClearLeaderBoard()
        {
            foreach (GameObject player in _players)
                player.SetActive(false);
        }

        private void ShowGetEntriesError(string error)
        {
            Debug.Log($"ShowGetEntriesError {error}");
            _leaderBoardService.OnGetEntriesError -= ShowGetEntriesError;
        }

        private void ShowGetEntryError(string error)
        {
            Debug.Log($"ShowGetEntryError {error}");
            _leaderBoardService.OnGetEntryError -= ShowGetEntryError;
        }

        private void GetLeaderBoardData()
        {
            // Debug.Log("RequestLeaderBoardData");
            _leaderBoardService.OnSuccessGetEntries += FillLeaderBoard;
            _leaderBoardService.OnSuccessGetEntry += FillPlayerInfo;
            SceneId scene = ProgressData.AllStats.CurrentLevelStats.SceneId;
            // Debug.Log($"Scene {scene}");
            _leaderBoardService.OnGetEntriesError += ShowGetEntriesError;
            _leaderBoardService.OnGetEntryError += ShowGetEntryError;

            if (_isCurrentScene)
            {
                _leaderBoardService.GetEntries(scene.GetLeaderBoardName(ProgressData.IsAsianMode));
                _leaderBoardService.GetPlayerEntry(scene.GetLeaderBoardName(ProgressData.IsAsianMode));
            }
            else
            {
                _leaderBoardService.GetEntries(SceneId.Initial.GetLeaderBoardName(ProgressData.IsAsianMode));
                _leaderBoardService.GetPlayerEntry(SceneId.Initial.GetLeaderBoardName(ProgressData.IsAsianMode));
                _isCurrentScene = true;
            }
        }

        private void FillLeaderBoard(LeaderboardGetEntriesResponse leaderboardGetEntriesResponse)
        {
            // Debug.Log("FillLeaderBoard");
            LeaderboardEntryResponse[] leaderboardEntryResponses = leaderboardGetEntriesResponse.entries;
            // Debug.Log($"leaderboardEntryResponses {leaderboardEntryResponses.Length}");
            LeaderboardEntryResponse response;
            PlayerItem playerItem;

            for (int i = 0; i < leaderboardEntryResponses.Length; i++)
            {
                if (i >= _players.Length)
                    return;

                response = leaderboardEntryResponses[i];
                playerItem = _players[i].GetComponent<PlayerItem>();
                playerItem.Rank.text = response.rank.ToString();

                if (!Application.isEditor)
                    StartCoroutine(LoadAvatar(response.player.scopePermissions.avatar, playerItem.Icon));

                playerItem.Name.text = response.player.publicName;
                playerItem.Score.text = response.score.ToString();
                playerItem.gameObject.SetActive(true);
                // Debug.Log($"i {i}");
                // Debug.Log($"publicName {response.player.publicName}");
                // Debug.Log($"score {response.score}");
                // Debug.Log($"rank {response.rank}");
            }

            if (_leaderBoardService != null)
                _leaderBoardService.OnSuccessGetEntries -= FillLeaderBoard;
        }

        private void FillPlayerInfo(LeaderboardEntryResponse response)
        {
            // Debug.Log("FillPlayerInfo");
            if (!Application.isEditor)
                StartCoroutine(LoadAvatar(response.player.scopePermissions.avatar, _iconImage));

            _rankText.text = $"#{response.rank}";
            _nameText.text = response.player.publicName;
            _scoreText.text = response.score.ToString();

            if (_leaderBoardService != null)
                _leaderBoardService.OnSuccessGetEntry -= FillPlayerInfo;

            if (!string.IsNullOrEmpty(response.player.publicName))
                _playerDataContainer.SetActive(true);
            // Debug.Log($"publicName {response.player.publicName}");
            // Debug.Log($"score {response.score}");
            // Debug.Log($"rank {response.rank}");
        }

        private IEnumerator LoadAvatar(string avatarUrl, RawImage image)
        {
            // Debug.Log("LoadAvatar");
            image.gameObject.SetActive(false);
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(avatarUrl);
            yield return request.SendWebRequest();

            if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log($"LoadAvatar {request.error}");
            }
            else
            {
                image.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                image.gameObject.SetActive(true);
            }
        }

        private void ToGameEndWindow()
        {
            _windowService.Show<GameFinishedWindow>(WindowId.GameEnd);
            _audioService.LaunchGameEventSound(GameEventSoundId.GameWon, _hero.transform, _audioSource);
        }
    }
}