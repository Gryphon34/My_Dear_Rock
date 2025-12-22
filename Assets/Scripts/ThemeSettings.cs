using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ThemeSettings : MonoBehaviour
{
    [Header("테마 기본 정보")]
    public string themeName;
    public int price;
    public Color textColor = Color.black;
    [HideInInspector] public bool hasOwned;

    [Header("UI 연결")] // [추가] 테마 버튼의 UI들
    public Outline outline;
    public TMP_Text priceText;

    [Header("테마 배경 설정")]
    public Material skybox;
    public AudioClip bgm;

    [Header("조명 설정")]
    public Color lightColor = Color.white;
    public Vector3 lightRotation;

    // [추가] 테마 버튼의 UI를 갱신하는 함수
    public void UpdateUI(Color baseColor, bool isCurrent, float alphaActive, float alphaLocked)
    {
        if (outline != null) outline.enabled = isCurrent;
        if (priceText != null)
        {
            // [로직 변경] 보유 중이면 이름만, 아니면 이름+가격을 표시합니다.
            if (isCurrent) priceText.text = $"{themeName} (In Use)";
            else if (hasOwned) priceText.text = $"{themeName}"; // 보유 시 가격 제거
            else priceText.text = $"{themeName} {price}";

            priceText.color = new Color(baseColor.r, baseColor.g, baseColor.b, hasOwned ? alphaActive : alphaLocked);
        }
    }

    public void ApplyTheme(Light directionalLight, AudioSource bgmSource)
    {
        if (skybox != null) RenderSettings.skybox = skybox;
        if (directionalLight != null)
        {
            directionalLight.color = lightColor;
            directionalLight.transform.eulerAngles = lightRotation;
        }
        if (bgmSource != null && bgm != null)
        {
            bgmSource.clip = bgm;
            bgmSource.Play();
        }
        DynamicGI.UpdateEnvironment();
    }
}