using UnityEngine;
using System;

public class RockCleaning : MonoBehaviour
{
    public enum DirtType { Dust, Moss }

    [Header("설정")]
    public DirtType currentType = DirtType.Dust;
    [Range(0, 1)] public float dirtyLevel = 0.0f;
    public float cleanSpeed = 1.5f;
    public float timeToFullDirty = 21600f;

    [Header("쉐이더 설정")]
    public string dustProperty = "_DustStrength";
    public float maxDustStrength = 6.0f; // [추가] DirtyLevel이 1일 때 적용될 쉐이더 값 (기본 6)

    [Header("연결: 이펙트 & 사운드")]
    public ParticleSystem dustEffect;
    public Renderer mossRenderer;
    public AudioClip cleanSound;

    private Renderer rockRenderer;
    private AudioSource audioSource;
    private bool isCleaned = false;

    // 최적화: 재질 속성 제어용 블록
    private MaterialPropertyBlock propBlock;
    private int dustID;

    void Start()
    {
        rockRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();

        propBlock = new MaterialPropertyBlock();
        dustID = Shader.PropertyToID(dustProperty);

        LoadRockStatus();
        UpdateVisuals();
    }

    void Update()
    {
        if (dirtyLevel < 1.0f)
        {
            dirtyLevel += Time.deltaTime / timeToFullDirty;
        }

        if (dirtyLevel > 0.8f && isCleaned)
        {
            isCleaned = false;
        }

        UpdateVisuals();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Cleaner"))
        {
            dirtyLevel -= Time.deltaTime * cleanSpeed;
            if (dirtyLevel < 0f) dirtyLevel = 0f;

            if (dustEffect != null && !dustEffect.isPlaying) dustEffect.Play();

            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.volume = SettingsManager.Instance.sfxVolume;
                audioSource.Play();
            }

            CheckReward();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cleaner"))
        {
            if (dustEffect != null) dustEffect.Stop();
        }
    }

    void CheckReward()
    {
        if (dirtyLevel <= 0.0f && !isCleaned)
        {
            isCleaned = true;
            GameManager.Instance?.AddPebbles(100);

            if (audioSource != null && cleanSound != null)
            {
                audioSource.PlayOneShot(cleanSound, SettingsManager.Instance.sfxVolume);
            }
        }
    }

    void UpdateVisuals()
    {
        dirtyLevel = Mathf.Clamp01(dirtyLevel);

        rockRenderer.GetPropertyBlock(propBlock);

        // [수정] DirtyLevel(0~1) * MaxStrength(6) = 쉐이더 값(0~6)
        float finalStrength = dirtyLevel * maxDustStrength;
        propBlock.SetFloat(dustID, finalStrength);

        rockRenderer.SetPropertyBlock(propBlock);
    }

    void OnApplicationQuit() => SaveRockStatus();
    void OnApplicationPause(bool pause) { if (pause) SaveRockStatus(); }

    void SaveRockStatus()
    {
        PlayerPrefs.SetFloat("SavedDirtyLevel", dirtyLevel);
        PlayerPrefs.SetString("LastSaveTime", DateTime.Now.ToString());
        PlayerPrefs.SetInt("IsCleanedStatus", isCleaned ? 1 : 0);
        PlayerPrefs.Save();
    }

    void LoadRockStatus()
    {
        dirtyLevel = PlayerPrefs.GetFloat("SavedDirtyLevel", 0f);
        isCleaned = PlayerPrefs.GetInt("IsCleanedStatus", 0) == 1;

        if (PlayerPrefs.HasKey("LastSaveTime"))
        {
            DateTime lastTime = DateTime.Parse(PlayerPrefs.GetString("LastSaveTime"));
            double secondsPassed = (DateTime.Now - lastTime).TotalSeconds;

            float addedDirt = (float)(secondsPassed / timeToFullDirty);
            dirtyLevel += addedDirt;
        }
    }
}