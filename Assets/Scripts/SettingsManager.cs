using Firebase.Auth;
using Google;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("UI 연결")]
    public GameObject settingsPanel;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public TMP_Text statusText;

    [Header("대상 오디오 소스")]
    public AudioSource bgmSource;
    public float sfxVolume = 1f;

    [Header("구글 로그인 설정")]
    public string webClientId = "여기에_웹_클라이언트_ID_입력";

    void Awake() { Instance = this; }

    void Start()
    {
        LoadSettings();
        settingsPanel.SetActive(false);
    }

    #region 설정 및 오디오 제어
    public void OpenSettings() => settingsPanel.SetActive(true);
    public void CloseSettings() => settingsPanel.SetActive(false);

    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null) bgmSource.volume = volume;
        PlayerPrefs.SetFloat("BGMVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    void LoadSettings()
    {
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
        if (bgmSlider) bgmSlider.value = bgmVol;
        if (sfxSlider) sfxSlider.value = sfxVol;
        if (bgmSource) bgmSource.volume = bgmVol;
        sfxVolume = sfxVol;
    }
    #endregion

    #region 통합 로그인 시스템 (FirebaseUser 전용 버전)

    public async void ClickGoogleLogin()
    {
#if UNITY_EDITOR
        await Task.Yield();
        HandleLoginSuccess("test_user_editor", "에디터 테스트 유저");
#else
        GoogleSignInConfiguration config = new GoogleSignInConfiguration {
            WebClientId = webClientId, RequestIdToken = true
        };
        GoogleSignIn.Configuration = config;

        try {
            GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();
            await SignInWithFirebase(googleUser.IdToken);
        } catch (Exception e) {
            Debug.LogError("구글 로그인 오류: " + e.Message);
        }
#endif
    }

    public async void ClickAnonymousLogin()
    {
        try
        {
            // 타입을 명시하지 않고 var로 받습니다.
            var task = FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync();
            await task;

            // 결과가 무엇이든 'dynamic'하게 접근하여 에러를 회피합니다.
            object result = task.GetType().GetProperty("Result").GetValue(task);

            string uid = "";
            // AuthResult인 경우
            if (result.GetType().Name.Contains("AuthResult"))
            {
                var userProp = result.GetType().GetProperty("User").GetValue(result);
                uid = (string)userProp.GetType().GetProperty("UserId").GetValue(userProp);
            }
            // FirebaseUser인 경우
            else
            {
                uid = (string)result.GetType().GetProperty("UserId").GetValue(result);
            }

            HandleLoginSuccess(uid, "익명 사용자");
        }
        catch (Exception e) { Debug.LogError(e.Message); }
    }

    private async Task SignInWithFirebase(string idToken)
    {
        try
        {
            Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
            // [해결] 에러 메시지에 따라 AuthResult 대신 FirebaseUser를 직접 받습니다.
            FirebaseUser user = await FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential);
            HandleLoginSuccess(user.UserId, user.DisplayName);
        }
        catch (Exception e)
        {
            Debug.LogError("인증 오류: " + e.Message);
        }
    }

    private void HandleLoginSuccess(string userId, string userName)
    {
        PlayerPrefs.SetString("FirebaseUserId", userId);
        if (GameManager.Instance != null) GameManager.Instance.LoadDataFromCloud(userId); //

        if (statusText != null)
        {
            string name = string.IsNullOrEmpty(userName) ? "사용자" : userName;
            statusText.text = name + "님 환영합니다!";
        }
    }
    #endregion
}