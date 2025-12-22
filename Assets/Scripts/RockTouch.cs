using UnityEngine;
using UnityEngine.EventSystems;

public class RockTouch : MonoBehaviour
{
    [Header("소리 설정")]
    public AudioSource audioSource; // [추가] 소리를 재생할 컴포넌트
    public AudioClip touchSound;    // [추가] 재생할 돌 클릭 소리

    [Header("이펙트 설정")]
    public Transform effectSpawnPoint;

    [Header("돌멩이 연출")]
    public float squatAmount = 0.8f;
    public float smooth = 15f;
    private Vector3 originalScale;
    private Vector3 targetScale;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        // [추가] AudioSource가 연결 안 되어 있다면 자동으로 가져오기 시도
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * smooth);
        targetScale = originalScale;
    }

    private void OnPointerDown(PointerEventData eventData)
    {
        // UI 클릭 중이거나 상점이 열려있으면 소리도 재생하지 않음
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (ShopManager.Instance != null && ShopManager.Instance.shopPanel.activeInHierarchy) return;

        // 1. 조약돌 지급
        if (GameManager.Instance != null) GameManager.Instance.AddPebbles(1);

        // 2. 소리 재생
        if (audioSource != null && touchSound != null)
        {
            audioSource.volume = SettingsManager.Instance.sfxVolume;
            audioSource.pitch=Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(touchSound);
        }

        // 3. 고정 위치에 이펙트 생성
        SpawnTouchEffect();

        // 4. 찌그러짐 효과
        targetScale = new Vector3(originalScale.x / squatAmount, originalScale.y * squatAmount, originalScale.z / squatAmount);
    }

    void SpawnTouchEffect()
    {
        GameObject effectToSpawn = null;
        if (ShopManager.Instance != null && ShopManager.Instance.shopEffects != null)
        {
            foreach (var effect in ShopManager.Instance.shopEffects)
            {
                if (effect != null && effect.isEquipped)
                {
                    effectToSpawn = effect.effectPrefab;
                    break;
                }
            }
        }

        if (effectToSpawn != null && effectSpawnPoint != null)
        {
            GameObject eff = Instantiate(effectToSpawn, effectSpawnPoint.position, effectSpawnPoint.rotation);
            Destroy(eff, 2.0f);
        }
    }
}