using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopEffect : MonoBehaviour
{
    public string effectName;
    public int price;
    public GameObject effectPrefab;
    public Outline outline;
    public TMP_Text priceText;

    [HideInInspector] public bool hasOwned;
    [HideInInspector] public bool isEquipped;

    public void UpdateUI(Color baseColor, float alphaActive, float alphaLocked)
    {
        if (outline != null) outline.enabled = isEquipped;
        if (priceText != null)
        {
            // [로직 변경] 장착/보유/미보유 상태를 명확히 나눕니다.
            if (isEquipped) priceText.text = $"{effectName} (In Use)";
            else if (hasOwned) priceText.text = $"{effectName}"; // 보유 시 가격 제거
            else priceText.text = $"{effectName} {price}";

            priceText.color = new Color(baseColor.r, baseColor.g, baseColor.b, hasOwned ? alphaActive : alphaLocked);
        }
    }
}