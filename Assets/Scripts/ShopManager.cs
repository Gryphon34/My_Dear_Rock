using System.Collections.Generic;
using UnityEngine;
using TMPro;

// [추가] 클라우드 저장용 데이터 묶음 클래스
[System.Serializable]
public class ShopCloudData
{
    public int currentTheme;
    public List<bool> themeOwned = new List<bool>();
    public List<bool> itemOwned = new List<bool>();
    public List<bool> itemEquipped = new List<bool>();
    public List<bool> effectOwned = new List<bool>();
    public List<bool> effectEquipped = new List<bool>();
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    public enum ThemeType { Default, Camping, Space }

    [Header("테마 리스트")]
    public List<ThemeSettings> themes;

    [Header("아이템/이펙트 리스트")]
    public List<ShopItem> shopItems;
    public List<ShopEffect> shopEffects;

    [Header("일괄 색상 조절 UI 리스트")]
    public List<TMP_Text> allUITexts;

    [Header("UI & 환경 컴포넌트")]
    public GameObject shopPanel;
    public GameObject themePage, itemPage, effectPage;
    public Light directionalLight;
    public AudioSource bgmSource, sfxSource;
    public AudioClip buySound;
    public TMP_Text pebbleCountText;
    public TMP_Text tabThemeText, tabItemText, tabEffectText;

    private ThemeType currentTheme = ThemeType.Default;
    private int currentTab = 0;
    public float alphaActive = 1.0f;
    public float alphaLocked = 0.5f;

    private void Awake() { Instance = this; }

    void Start()
    {
        LoadShopData(); // 로컬 데이터를 먼저 불러옴
        ApplyCurrentTheme();
        foreach (var item in shopItems) item.Apply();
        shopPanel.SetActive(false);
    }

    // --- 상점 버튼 클릭 이벤트들 ---
    public void OpenShop() {
        if (shopPanel)
        {
            shopPanel.SetActive(true);
            ClickTabTheme();
            // 광고 숨기기 (추가)
            AdMobManager.Instance?.SetBannerVisible(false);
        }
    }
    public void CloseShop() { shopPanel.SetActive(false); }
    public void ClickTabTheme() => SwitchTab(0);
    public void ClickTabItem() => SwitchTab(1);
    public void ClickTabEffect() => SwitchTab(2);

    private void SwitchTab(int index)
    {
        currentTab = index;
        if (themePage) themePage.SetActive(index == 0);
        if (itemPage) itemPage.SetActive(index == 1);
        if (effectPage) effectPage.SetActive(index == 2);
        UpdateShopUI();
    }

    public void ClickTheme(int index)
    {
        if (index >= themes.Count) return;
        ThemeSettings target = themes[index];

        if (index == 0 || target.hasOwned) { currentTheme = (ThemeType)index; }
        else
        {
            if (GameManager.Instance.SpendPebbles(target.price))
            {
                target.hasOwned = true;
                currentTheme = (ThemeType)index;
                sfxSource.PlayOneShot(buySound);
            }
        }
        ApplyCurrentTheme(); SaveShopData(); UpdateShopUI();
    }

    public void ClickItem(int index)
    {
        if (index >= shopItems.Count) return;
        var item = shopItems[index];

        if (!item.hasOwned)
        {
            if (GameManager.Instance.SpendPebbles(item.price))
            {
                item.hasOwned = true;
                item.isEquipped = true;
                sfxSource.PlayOneShot(buySound);
            }
        }
        else { item.isEquipped = !item.isEquipped; }

        if (item.isEquipped && !string.IsNullOrEmpty(item.itemCategory))
        {
            foreach (var otherItem in shopItems)
            {
                if (otherItem != item && otherItem.itemCategory == item.itemCategory)
                {
                    otherItem.isEquipped = false;
                    otherItem.Apply();
                }
            }
        }

        item.Apply(); SaveShopData(); UpdateShopUI();
    }

    public void ClickEffect(int index)
    {
        for (int i = 0; i < shopEffects.Count; i++)
        {
            if (i == index)
            {
                if (!shopEffects[i].hasOwned)
                {
                    if (GameManager.Instance.SpendPebbles(shopEffects[i].price))
                    {
                        shopEffects[i].hasOwned = true; shopEffects[i].isEquipped = true;
                        sfxSource.PlayOneShot(buySound);
                    }
                }
                else { shopEffects[i].isEquipped = !shopEffects[i].isEquipped; }
            }
            else { shopEffects[i].isEquipped = false; }
        }
        SaveShopData(); UpdateShopUI();
    }

    void ApplyCurrentTheme()
    {
        for (int i = 0; i < themes.Count; i++)
        {
            bool isActive = (i == (int)currentTheme);
            themes[i].gameObject.SetActive(isActive);
            if (isActive) themes[i].ApplyTheme(directionalLight, bgmSource);
        }
    }

    void UpdateShopUI()
    {
        Color targetColor = themes[(int)currentTheme].textColor;
        foreach (var txt in allUITexts) { if (txt != null) txt.color = targetColor; }

        for (int i = 0; i < themes.Count; i++)
            themes[i].UpdateUI(targetColor, i == (int)currentTheme, alphaActive, alphaLocked);

        foreach (var item in shopItems) item.UpdateUI(targetColor, alphaActive, alphaLocked);
        foreach (var eff in shopEffects) eff.UpdateUI(targetColor, alphaActive, alphaLocked);

        if (pebbleCountText) pebbleCountText.text = GameManager.Instance.pebbleCount.ToString();

        SetTabStyle(tabThemeText, targetColor, currentTab == 0);
        SetTabStyle(tabItemText, targetColor, currentTab == 1);
        SetTabStyle(tabEffectText, targetColor, currentTab == 2);
    }

    void SetTabStyle(TMP_Text txt, Color c, bool isActive)
    {
        if (txt) txt.color = new Color(c.r, c.g, c.b, isActive ? alphaActive : alphaLocked);
    }

    // [수정] 데이터 저장 시 클라우드 전송 로직 추가
    void SaveShopData()
    {
        // 1. 로컬 저장 (PlayerPrefs)
        PlayerPrefs.SetInt("CurrentTheme", (int)currentTheme);
        for (int i = 0; i < themes.Count; i++) PlayerPrefs.SetInt("ThemeHas" + i, themes[i].hasOwned ? 1 : 0);
        for (int i = 0; i < shopItems.Count; i++)
        {
            PlayerPrefs.SetInt("ItemHas" + i, shopItems[i].hasOwned ? 1 : 0);
            PlayerPrefs.SetInt("ItemEquip" + i, shopItems[i].isEquipped ? 1 : 0);
        }
        for (int i = 0; i < shopEffects.Count; i++)
        {
            PlayerPrefs.SetInt("EffectHas" + i, shopEffects[i].hasOwned ? 1 : 0);
            PlayerPrefs.SetInt("EffectEquip" + i, shopEffects[i].isEquipped ? 1 : 0);
        }
        PlayerPrefs.Save();

        // 2. 클라우드 전송용 JSON 데이터 생성
        ShopCloudData cloudData = new ShopCloudData();
        cloudData.currentTheme = (int)currentTheme;
        foreach (var t in themes) cloudData.themeOwned.Add(t.hasOwned);
        foreach (var i in shopItems)
        {
            cloudData.itemOwned.Add(i.hasOwned);
            cloudData.itemEquipped.Add(i.isEquipped);
        }
        foreach (var e in shopEffects)
        {
            cloudData.effectOwned.Add(e.hasOwned);
            cloudData.effectEquipped.Add(e.isEquipped);
        }

        string json = JsonUtility.ToJson(cloudData);
        GameManager.Instance.SaveShopDataToCloud(json); // 서버로 쏴줌
    }

    void LoadShopData()
    {
        currentTheme = (ThemeType)PlayerPrefs.GetInt("CurrentTheme", 0);
        if (themes.Count > 0) themes[0].hasOwned = true;
        for (int i = 1; i < themes.Count; i++) themes[i].hasOwned = PlayerPrefs.GetInt("ThemeHas" + i, 0) == 1;
        for (int i = 0; i < shopItems.Count; i++)
        {
            shopItems[i].hasOwned = PlayerPrefs.GetInt("ItemHas" + i, 0) == 1;
            shopItems[i].isEquipped = PlayerPrefs.GetInt("ItemEquip" + i, 0) == 1;
        }
        for (int i = 0; i < shopEffects.Count; i++)
        {
            shopEffects[i].hasOwned = PlayerPrefs.GetInt("EffectHas" + i, 0) == 1;
            shopEffects[i].isEquipped = PlayerPrefs.GetInt("EffectEquip" + i, 0) == 1;
        }
    }

    // [추가] 클라우드에서 받은 JSON을 실제 게임 리스트에 적용
    public void ApplyCloudShopData(string json)
    {
        ShopCloudData data = JsonUtility.FromJson<ShopCloudData>(json);

        currentTheme = (ThemeType)data.currentTheme;

        for (int i = 0; i < themes.Count && i < data.themeOwned.Count; i++)
            themes[i].hasOwned = data.themeOwned[i];

        for (int i = 0; i < shopItems.Count && i < data.itemOwned.Count; i++)
        {
            shopItems[i].hasOwned = data.itemOwned[i];
            shopItems[i].isEquipped = data.itemEquipped[i];
            shopItems[i].Apply(); // 오브젝트 활성화 여부 적용
        }

        for (int i = 0; i < shopEffects.Count && i < data.effectOwned.Count; i++)
        {
            shopEffects[i].hasOwned = data.effectOwned[i];
            shopEffects[i].isEquipped = data.effectEquipped[i];
        }

        ApplyCurrentTheme();
        UpdateShopUI();
    }
}