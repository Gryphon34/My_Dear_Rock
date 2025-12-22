using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using TMPro;
using Firebase.Auth;
using Google;
using System;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TitleManager : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject loginButtonsGroup;
    public GameObject startPanel;
    public TMP_Text statusText;

    [Header("효과 연결")]
    public RectTransform touchTextTransform;
    public CanvasGroup fadeCanvasGroup;

    [Header("구글 로그인 설정")]
    public string webClientId = "여기에_웹_클라이언트_ID_입력";

    private bool isReadyToStart = false;
    private bool isTransitioning = false;

    async void Start()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (loginButtonsGroup != null) loginButtonsGroup.SetActive(true);
        isReadyToStart = false;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            await FadeEffect(0f, 1.0f);
            fadeCanvasGroup.gameObject.SetActive(false);
        }

        CheckExistingLogin();
    }

    void Update()
    {
        // Input.GetMouseButtonDown(0) 대신 새로운 방식을 사용합니다.
        if (isReadyToStart && !isTransitioning && Pointer.current.press.wasPressedThisFrame)
        {
            StartGameSequence();
        }
    }

    private void CheckExistingLogin()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
            HandleLoginSuccess(user.UserId, user.DisplayName);
        }
    }

    public async void ClickGoogleLogin()
    {
#if UNITY_EDITOR
        await Task.Yield();
        HandleLoginSuccess("editor_test_id", "에디터 테스트 유저");
#else
        GoogleSignInConfiguration config = new GoogleSignInConfiguration {
            WebClientId = webClientId, RequestIdToken = true
        };
        GoogleSignIn.Configuration = config;

        try {
            GoogleSignInUser googleUser = await GoogleSignIn.DefaultInstance.SignIn();
            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
            
            // [통합 해결] 타입을 직접 명시하지 않고 Task의 결과를 리플렉션으로 처리합니다.
            var task = FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential);
            await task;
            object result = task.GetType().GetProperty("Result").GetValue(task);

            string uid = "";
            string displayName = "";

            if (result.GetType().Name.Contains("AuthResult"))
            {
                var userProp = result.GetType().GetProperty("User").GetValue(result);
                uid = (string)userProp.GetType().GetProperty("UserId").GetValue(userProp);
                displayName = (string)userProp.GetType().GetProperty("DisplayName").GetValue(userProp);
            }
            else
            {
                uid = (string)result.GetType().GetProperty("UserId").GetValue(result);
                displayName = (string)result.GetType().GetProperty("DisplayName").GetValue(result);
            }

            HandleLoginSuccess(uid, displayName);
        } catch (Exception e) {
            statusText.text = "Google Login Failed";
            Debug.LogError(e.Message);
        }
#endif
    }

    public async void ClickAnonymousLogin()
    {
        try
        {
            statusText.text = "Guest Login Attempt...";
            // [통합 해결] SettingsManager와 동일한 우회 방식을 적용합니다.
            var task = FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync();
            await task;
            object result = task.GetType().GetProperty("Result").GetValue(task);

            string uid = "";

            if (result.GetType().Name.Contains("AuthResult"))
            {
                var userProp = result.GetType().GetProperty("User").GetValue(result);
                uid = (string)userProp.GetType().GetProperty("UserId").GetValue(userProp);
            }
            else
            {
                uid = (string)result.GetType().GetProperty("UserId").GetValue(result);
            }

            HandleLoginSuccess(uid, "Guest User");
        }
        catch (Exception e)
        {
            statusText.text = "Login Failed";
            Debug.LogError(e.Message); // CS0168 경고 해결
        }
    }

    private void HandleLoginSuccess(string userId, string userName)
    {
        PlayerPrefs.SetString("FirebaseUserId", userId);
        if (loginButtonsGroup != null) loginButtonsGroup.SetActive(false);
        if (startPanel != null) startPanel.SetActive(true);
        if (statusText != null)
        {
            string name = string.IsNullOrEmpty(userName) ? "User" : userName;
            statusText.text = name + " Welcome!";
        }
        isReadyToStart = true;
    }

    private async Task FadeEffect(float targetAlpha, float duration)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            await Task.Yield();
        }
        fadeCanvasGroup.alpha = targetAlpha;
    }

    private async Task ScaleEffect(float targetScale, float duration)
    {
        Vector3 startScale = touchTextTransform.localScale;
        Vector3 endScale = new Vector3(targetScale, targetScale, 1f);
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            touchTextTransform.localScale = Vector3.Lerp(startScale, endScale, time / duration);
            await Task.Yield();
        }
        touchTextTransform.localScale = endScale;
    }

    private async void StartGameSequence()
    {
        isTransitioning = true;
        if (touchTextTransform != null)
        {
            await ScaleEffect(1.2f, 0.15f);
            await ScaleEffect(1.0f, 0.15f);
        }
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            await FadeEffect(1f, 0.8f);
        }
        SceneManager.LoadScene("PetRockScene");
    }
}