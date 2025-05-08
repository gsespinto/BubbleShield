using System;
using System.Collections;
using UnityEngine;
using Unity.Services.LevelPlay;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class AdsManager : MonoBehaviour
{
    [Header("Game ID")]
    [SerializeField] string appKey;

    [Header("Ad ID")]
    [SerializeField] string interstitialAdUnitId = "Interstitial_ID";
    [SerializeField] string bannerAdUnitId = "Banner_ID";

    [Header("Ads Settings")]
    [SerializeField, Min(0)] private int levelAdThreshold;
    [SerializeField, Min(0)] private float rewardDelay = 0;
    [SerializeField] private bool useTestSuite = false;

    private static AdsManager instance;
    public static AdsManager Instance {  get { return instance; } }

    private static int currentAdThreshold = 0;

    public static bool showAds = true;

    private bool hasInitialized = false;
    public static bool HasInitialized
    {
        get { return instance && instance.hasInitialized; }
    }

    private Action completedCallback;
    private bool showingAd;
    public delegate void InitializedDelegate();
    public InitializedDelegate OnInitialized;


    private bool rewardedAvailable = false;
    public static bool RewardedAvailable {
        get { return instance && instance.rewardedAvailable; }
    }

    private LevelPlayBannerAd bannerAd;
    private LevelPlayInterstitialAd interstitialAd;

    private bool showImmediately = false;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeAds();
    }

    void OnApplicationPause(bool isPaused) { 	 
        IronSource.Agent.onApplicationPause(isPaused);	 
    }

    void InitializeAds()
    {
        if (useTestSuite){
            IronSource.Agent.setMetaData("is_test_suite", "enable");
        }

        // Init the SDK when implementing the Multiple Ad Units API for Interstitial and Banner formats, with Rewarded using legacy APIs 
        com.unity3d.mediation.LevelPlayAdFormat[] legacyAdFormats = new[] { com.unity3d.mediation.LevelPlayAdFormat.REWARDED };
        LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitializationFailedEvent;

        string userId = UnityServices.Instance != null && AuthenticationService.Instance != null ? 
            AuthenticationService.Instance.PlayerId : "";
        
        LevelPlay.Init(appKey, userId, legacyAdFormats);
    }

    public static void CheckForInterstitialAd(Action callback)
    {
        if (callback == null)
            return;

        currentAdThreshold++;
        if (currentAdThreshold >= instance.levelAdThreshold)
        {
            ShowAd(callback, EAdType.Interstitial);
            currentAdThreshold = 0;
            return;
        }

        callback.Invoke();
    }

    public static void ShowAd(Action callback, EAdType type)
    {
        if (callback == null)
        {
            Logger.Error("Invalid completed callback!", instance);
        }

        if (!showAds)
        {
            if (callback != null)
                callback();
            return;
        }

        if (!instance)
        {
            Logger.Error("Couldn't get valid instance of AdCanvas!");
            if (callback != null)
                callback();
            return;
        }

        if (instance.showingAd)
        {
            Logger.Error("Already showing ad!", instance);
            return;
        }

        instance.completedCallback = callback;
        instance.showingAd = true;

        switch (type)
        {
            case EAdType.Interstitial:
                if (instance.interstitialAd != null){
                    if (!instance.interstitialAd.IsAdReady()){
                        instance.showImmediately = true;
                        instance.interstitialAd.LoadAd();
                    } else {
                        instance.interstitialAd.ShowAd();
                    }
                } else {
                    instance.CallCallback();
                }
                break;

            case EAdType.Rewarded:
                if (RewardedAvailable){
                    // Customize placements according to target reward ???
                    //IronSource.Agent.showRewardedVideo("YOUR_PLACEMENT_NAME");
                    
                    IronSource.Agent.showRewardedVideo();
                }
                
                break;
        }
    }

    public static void FinishRewarded()
    {
        if (!instance)
        {
            Logger.Error("Missing valid AdsManager instance!");
            return;
        }

        instance.StartCoroutine(instance.RewardCallback());
    }

    void FinishInterstitial()
    {
        CallCallback();
        showImmediately = false;
        instance.interstitialAd.LoadAd();
    }

    IEnumerator RewardCallback()
    {
        yield return new WaitForSecondsRealtime(rewardDelay);
        CallCallback();
        yield return null;
    }

    public void CallCallback()
    {
        completedCallback?.Invoke();
        showingAd = false;
    }

    public static void ShowBannerAd()
    {
        if (!instance || instance.bannerAd == null) return;
        instance.bannerAd.ResumeAutoRefresh();
        instance.bannerAd.ShowAd();
    }
 
    // Implement a method to call when the Hide Banner button is clicked:
    public static void HideBannerAd()
    {
        if (!instance || instance.bannerAd ==  null) return;
        instance.bannerAd.HideAd();
        instance.bannerAd.PauseAutoRefresh();
    }

#region  SDK Initialization
    private void SdkInitializationFailedEvent(LevelPlayInitError error)
    {
        Logger.Info($"Failed to initilize IronSource sdk!\n{error.ErrorCode} - {error.ErrorMessage}", this);
    }

    private void SdkInitializationCompletedEvent(LevelPlayConfiguration configuration)
    {
        if (useTestSuite){
            IronSource.Agent.launchTestSuite();
        }
        else{
            IronSource.Agent.shouldTrackNetworkState(true);

            SetupRewardedCallbacks();
            CreateInterstitialAd();
            CreateBannerAd();
        }

        Logger.Info("Initialized IronSource sdk successfully!", this);
        hasInitialized = true;
    }

    void SetupRewardedCallbacks()
    {
        //Add AdInfo Rewarded Video Events
        IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
        IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;
    }

    void CreateInterstitialAd()
    {
        if (interstitialAdUnitId == ""){
            Logger.Error("Couldn't create interstitial ad!");
            return;
        }

        interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);
                
        // Register to interstitial events
        interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
        interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
        interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
        interstitialAd.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
        interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
        interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
        interstitialAd.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;

        interstitialAd.LoadAd();
    }

    void CreateBannerAd() 
    {
        if (bannerAdUnitId == ""){
            Logger.Error("Couldn't create banner ad!");
            return;
        }

        //Create banner instance
        bannerAd = new LevelPlayBannerAd(bannerAdUnitId);

        //Subscribe BannerAd events
        bannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
        bannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;
        bannerAd.OnAdDisplayed += BannerOnAdDisplayedEvent;
        bannerAd.OnAdDisplayFailed += BannerOnAdDisplayFailedEvent;
        bannerAd.OnAdClicked += BannerOnAdClickedEvent;
        bannerAd.OnAdCollapsed += BannerOnAdCollapsedEvent;
        bannerAd.OnAdLeftApplication += BannerOnAdLeftApplicationEvent;
        bannerAd.OnAdExpanded += BannerOnAdExpandedEvent;

        bannerAd.LoadAd();
        bannerAd.PauseAutoRefresh();
    }
#endregion

#region  Rewarded Callbacks
    // Indicates that there’s an available ad.
    // The adInfo object includes information about the ad that was loaded successfully
    // This replaces the RewardedVideoAvailabilityChangedEvent(true) event
    void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo) {
        rewardedAvailable = true;
    }

    // Indicates that no ads are available to be displayed
    // This replaces the RewardedVideoAvailabilityChangedEvent(false) event
    void RewardedVideoOnAdUnavailable() {
        rewardedAvailable = false;
    }
    
    // The Rewarded Video ad view has opened. Your activity will loose focus.
    void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo){
        Logger.Info("Opened rewarded ad!", this);
    }
    
    // The Rewarded Video ad view is about to be closed. Your activity will regain its focus.
    void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo){
        Logger.Info("Closed rewarded ad!", this);
    }

    // The user completed to watch the video, and should be rewarded.
    // The placement parameter will include the reward data.
    // When using server-to-server callbacks, you may ignore this event and wait for the ironSource server callback.
    void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo){
        Logger.Info("Finished rewarded ad!", this);

        /*if(placement != null)
        {
            String rewardName = placement.getRewardName();
            int rewardAmount = placement.getRewardAmount();
        }*/

        FinishRewarded();
        showingAd = false;
    }

    // The rewarded video ad was failed to show.
    void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo){
        Logger.Error($"Failed to show rewarded ad!\n{ error.getErrorCode() } - { error.getDescription() }", this);
        showingAd = false;
    }

    // Invoked when the video ad was clicked.
    // This callback is not supported by all networks, and we recommend using it only if
    // it’s supported by all networks you included in your build.
    void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo){
        Logger.Info("Clicked rewarded ad!", this);
    }

#endregion

#region Interstitial Callbacks
    void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo){
        Logger.Info("Loaded interstitial ad!", this);
        if (showImmediately)
        {
            interstitialAd.ShowAd();
        }

        showImmediately = false;
    }

    void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error){
        Logger.Error($"Failed to load interstitial ad!\n{ error.ErrorCode } - { error.ErrorMessage }", this);
        FinishInterstitial();
    }
    
    void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo){
        Logger.Info("Displayed interstitial ad!", this);
    }
    
    void InterstitialOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError infoError){
        Logger.Error($"Failed to display interstitial ad!", this);
        FinishInterstitial();
    }
    
    void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo){
        Logger.Info("Clicked interstitial ad!", this);
    }
    
    void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo){
        Logger.Info("Closed interstitial ad!", this);
        FinishInterstitial();
    }
    
    void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo){
        Logger.Info($"Interstitial ad info changed!\n{ adInfo.ToString() }", this);
    }
#endregion

#region Banner Callbacks
    void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo) {}
    void BannerOnAdLoadFailedEvent(LevelPlayAdError ironSourceError) {}
    void BannerOnAdClickedEvent(LevelPlayAdInfo adInfo) {}
    void BannerOnAdDisplayedEvent(LevelPlayAdInfo adInfo) {}
    void BannerOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError adInfoError) {}
    void BannerOnAdCollapsedEvent(LevelPlayAdInfo adInfo) {}
    void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo) {}
    void BannerOnAdExpandedEvent(LevelPlayAdInfo adInfo) {}
#endregion
}

public enum EAdType
{
    Interstitial,
    Rewarded
}
