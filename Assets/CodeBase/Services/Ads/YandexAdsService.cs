using System;
using System.Collections;
using BananaParty.YandexGames;

namespace CodeBase.Services.Ads
{
    public class YandexAdsService : IAdsService
    {
        public event Action OnInitializeSuccess;
        public event Action OnClosedVideoAd;
        public event Action<string> OnShowVideoAdError;
        public event Action OnRewardedAd;
        public event Action<bool> OnClosedInterstitialAd;
        public event Action<string> OnShowInterstitialAdError;
        public event Action OnOfflineInterstitialAd;

        public bool IsInitialized() =>
            YandexGamesSdk.IsInitialized;

        public IEnumerator Initialize()
        {
            yield return YandexGamesSdk.Initialize(OnInitializeSuccess);
        }

        public void ShowVideoAd() =>
            VideoAd.Show(onCloseCallback: OnClosedVideoAd, onErrorCallback: OnShowVideoAdError,
                onRewardedCallback: OnRewardedAd);

        public void ShowInterstitialAd() =>
            InterstitialAd.Show(onCloseCallback: OnClosedInterstitialAd, onErrorCallback: OnShowInterstitialAdError,
                onOfflineCallback: OnOfflineInterstitialAd);
    }
}