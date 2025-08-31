using System;
using System.Collections;
using BananaParty.YandexGames;

namespace CodeBase.Services.LeaderBoard
{
    public interface ILeaderboardService : IService
    {
        public event Action OnInitializeSuccess;
        public event Action<LeaderboardGetEntriesResponse> OnSuccessGetEntries;
        public event Action<LeaderboardEntryResponse> OnSuccessGetEntry;
        public event Action<string> OnGetEntryError;
        public event Action<string> OnGetEntriesError;
        public event Action<string> OnSetValueError;

        bool IsInitialized();
        IEnumerator Initialize();
        void GetPlayerEntry(string leaderboardName);
        void GetEntries(string leaderboardName);
        void SetValue(string leaderboardName, int value);
        event Action OnSetValueSuccess;
    }
}