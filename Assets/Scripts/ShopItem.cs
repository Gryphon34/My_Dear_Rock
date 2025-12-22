using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public string itemName;
    public int price;
    public string itemCategory;
    public GameObject itemObject;
    public Outline outline;
    public TMP_Text priceText;

    [HideInInspector] public bool hasOwned;
    [HideInInspector] public bool isEquipped;

    public void Apply() { if (itemObject != null) itemObject.SetActive(isEquipped); }

    public void UpdateUI(Color baseColor, float alphaActive, float alphaLocked)
    {
        if (outline != null) outline.enabled = isEquipped;
        if (priceText != null)
        {
            if (isEquipped) priceText.text = $"{itemName} (In Use)";
            else if (hasOwned) priceText.text = $"{itemName}";
            else priceText.text = $"{itemName} {price}";

            priceText.color = new Color(baseColor.r, baseColor.g, baseColor.b, hasOwned ? alphaActive : alphaLocked);
        }
    }
}