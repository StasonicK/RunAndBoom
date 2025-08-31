using System;
using BananaParty.YandexGames;

namespace CodeBase.Services.PlayerAuthorization
{
    public interface IAuthorization : IService
    {
        public event Action OnAuthorizeSuccessCallback;
        public event Action OnRequestPersonalProfileDataPermissionSuccessCallback;
        public event Action<PlayerAccountProfileDataResponse> OnGetProfileDataSuccessCallback;
        public event Action<string> OnGetPlayerDataSuccessCallback;
        public event Action OnSetPlayerDataSuccessCallback;
        public event Action<string> OnAuthorizeErrorCallback;
        public event Action<string> OnRequestErrorCallback;
        public event Action<string> OnGetDataErrorCallback;

        bool IsAuthorized();
        void Authorize();
        void RequestPersonalProfileDataPermission();
        void GetProfileData();
        void GetPlayerData();
        void SetPlayerData(string data);
    }
}