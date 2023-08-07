using System;
using Agava.YandexGames;

namespace CodeBase.Services.PlayerAuthorization
{
    public class YandexAuthorization : IAuthorization
    {
        public event Action OnAuthorizeSuccessCallback;
        public event Action OnRequestPersonalProfileDataPermissionSuccessCallback;
        public event Action<PlayerAccountProfileDataResponse> OnGetProfileDataSuccessCallback;
        public event Action<string> OnGetPlayerDataSuccessCallback;
        public event Action OnSetPlayerDataSuccessCallback;
        public event Action<string> OnAuthorizeErrorCallback;
        public event Action<string> OnRequestErrorCallback;
        public event Action<string> OnGetDataErrorCallback;
        public event Action<string> OnSetDataErrorCallback;

        public bool IsAuthorized() =>
            PlayerAccount.IsAuthorized;

        public void Authorize() =>
            PlayerAccount.Authorize(OnAuthorizeSuccessCallback, OnAuthorizeErrorCallback);

        public void RequestPersonalProfileDataPermission() =>
            PlayerAccount.RequestPersonalProfileDataPermission(
                OnRequestPersonalProfileDataPermissionSuccessCallback,
                OnRequestErrorCallback);

        public void GetProfileData() =>
            PlayerAccount.GetProfileData(OnGetProfileDataSuccessCallback, OnGetDataErrorCallback);

        public void GetPlayerData() =>
            PlayerAccount.GetCloudSaveData(OnGetPlayerDataSuccessCallback, OnGetDataErrorCallback);

        public void SetPlayerData(string data) =>
            PlayerAccount.SetCloudSaveData(data, OnSetPlayerDataSuccessCallback, OnSetDataErrorCallback);
    }
}