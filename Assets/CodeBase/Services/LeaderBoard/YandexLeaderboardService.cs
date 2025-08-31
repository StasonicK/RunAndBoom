using System;
using System.Collections;
using BananaParty.YandexGames;

namespace CodeBase.Services.LeaderBoard
{
    public class YandexLeaderboardService : ILeaderboardService
    {
        private const int TopPlayersCount = 5;

        public event Action OnInitializeSuccess;
        public event Action<LeaderboardGetEntriesResponse> OnSuccessGetEntries;
        public event Action<LeaderboardEntryResponse> OnSuccessGetEntry;
        public event Action<string> OnGetEntryError;
        public event Action<string> OnGetEntriesError;
        public event Action<string> OnSetValueError;
        public event Action OnSetValueSuccess;

        public bool IsInitialized() =>
            YandexGamesSdk.IsInitialized;

        public IEnumerator Initialize()
        {
            yield return YandexGamesSdk.Initialize(OnInitializeSuccess);
        }

        public void GetPlayerEntry(string leaderboardName) =>
            Leaderboard.GetPlayerEntry(leaderboardName: leaderboardName,
                onSuccessCallback: OnSuccessGetEntry, onErrorCallback: OnGetEntryError);

        public void GetEntries(string leaderboardName) =>
            Leaderboard.GetEntries(leaderboardName: leaderboardName,
                onSuccessCallback: OnSuccessGetEntries, topPlayersCount: TopPlayersCount,
                onErrorCallback: OnGetEntriesError);

        public void SetValue(string leaderboardName, int value) =>
            Leaderboard.SetScore(leaderboardName: leaderboardName, score: value, onErrorCallback: OnSetValueError,
                onSuccessCallback: OnInitializeSuccess);
    }
}