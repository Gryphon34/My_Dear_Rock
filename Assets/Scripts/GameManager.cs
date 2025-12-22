using UnityEngine;
using TMPro;
using Firebase.Database;
using System;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI 연결")]
    public TMP_Text pebbleText;

    [Header("게임 데이터")]
    public int pebbleCount = 0;
    private string userId;
    private DatabaseReference dbReference;

    void Awake()
    {
        Instance = this;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    void Start()
    {
        // 1. TitleManager에서 저장한 고유 ID를 가져옴
        userId = PlayerPrefs.GetString("FirebaseUserId", "");

        if (!string.IsNullOrEmpty(userId))
        {
            // 2. 서버에서 기존 데이터 로드
            LoadDataFromCloud(userId);
        }
    }

    // [추가] 서버에 상점 데이터를 통째로 저장하는 함수
    public void SaveShopDataToCloud(string jsonStoreData)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            // users > [UID] > shopData 경로에 JSON 문자열로 저장합니다.
            dbReference.Child("users").Child(userId).Child("shopData").SetRawJsonValueAsync(jsonStoreData);
        }
    }

    public async void LoadDataFromCloud(string uid)
    {
        try
        {
            DataSnapshot snapshot = await dbReference.Child("users").Child(uid).GetValueAsync();

            // 1. 조약돌 개수 로드
            if (snapshot.HasChild("pebbles"))
            {
                pebbleCount = int.Parse(snapshot.Child("pebbles").Value.ToString());
                UpdatePebbleUI();
            }

            // 2. 상점 데이터 로드 (추가됨)
            if (snapshot.HasChild("shopData"))
            {
                string json = snapshot.Child("shopData").GetRawJsonValue();
                // ShopManager에게 서버에서 받은 데이터를 적용하라고 전달합니다.
                ShopManager.Instance?.ApplyCloudShopData(json);
            }
        }
        catch (Exception e) { Debug.LogError("데이터 로드 실패: " + e.Message); }
    }

    // [중요] 돌 클릭(1개) 및 청소 완료(100개) 시 호출
    public void AddPebbles(int amount)
    {
        pebbleCount += amount;
        UpdatePebbleUI();
        SavePebblesToCloud();
    }

    // [중요] 상점 아이템 구매 시 호출
    public bool SpendPebbles(int amount)
    {
        if (pebbleCount >= amount)
        {
            pebbleCount -= amount;
            UpdatePebbleUI();
            SavePebblesToCloud();
            return true;
        }
        return false;
    }

    private void SavePebblesToCloud()
    {
        if (!string.IsNullOrEmpty(userId))
        {
            dbReference.Child("users").Child(userId).Child("pebbles").SetValueAsync(pebbleCount);
        }
    }

    public void UpdatePebbleUI()
    {
        if (pebbleText != null) pebbleText.text = pebbleCount.ToString();
    }
}