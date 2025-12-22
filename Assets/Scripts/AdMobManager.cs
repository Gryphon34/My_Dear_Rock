using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdMobManager : MonoBehaviour
{
    public static AdMobManager Instance;

    [Header("광고 단위 ID")]
    public string bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111"; // 테스트용 배너
    public string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; // 테스트용 보상형 [추가]

    private BannerView bannerView;
    private RewardedAd rewardedAd; // [추가]

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        // 애드몹 초기화 및 광고 로드 시작
        MobileAds.Initialize(initStatus => {
            Debug.Log("AdMob Initialized");
            RequestBanner();
            LoadRewardedAd(); // [추가] 보상형 광고 미리 불러오기
        });
    }

    // --- 배너 광고 기능 ---
    public void RequestBanner()
    {
        if (bannerView != null) bannerView.Destroy();
        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);
    }

    // --- 보상형 광고 기능 [추가] ---
    public void LoadRewardedAd()
    {
        if (rewardedAd != null) rewardedAd.Destroy();

        AdRequest request = new AdRequest();
        RewardedAd.Load(rewardedAdUnitId, request, (RewardedAd ad, LoadAdError error) => {
            if (error != null)
            {
                Debug.LogError("보상형 광고 로드 실패: " + error.GetMessage());
                return;
            }
            rewardedAd = ad;
            Debug.Log("보상형 광고 로드 성공!");

            // 광고가 닫혔을 때 자동으로 다음 광고 로드 준비
            rewardedAd.OnAdFullScreenContentClosed += () => { LoadRewardedAd(); };
        });
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) => {
                // [핵심] 광고 시청 완료 시 조약돌 500개 지급!
                Debug.Log("광고 시청 완료: " + reward.Type + " " + reward.Amount);
                GameManager.Instance?.AddPebbles(500);
            });
        }
        else
        {
            Debug.Log("광고가 아직 준비되지 않았습니다. 다시 로드합니다.");
            LoadRewardedAd();
        }
    }

    public void SetBannerVisible(bool visible)
    {
        if (bannerView == null) return;
        if (visible) bannerView.Show();
        else bannerView.Hide();
    }
}