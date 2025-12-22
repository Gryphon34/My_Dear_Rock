using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    [Header("부유 설정")]
    public float amplitude = 0.1f; // 위아래 이동 범위
    public float frequency = 0.5f; // 움직임 속도

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // [수정] 개별 변수 대신 themes 리스트의 인덱스 2번(Space)을 확인합니다.
        if (ShopManager.Instance != null && ShopManager.Instance.themes != null && ShopManager.Instance.themes.Count > 2)
        {
            // 인덱스 2번(우주 테마)이 비활성 상태라면 (우주 테마가 아니면)
            if (!ShopManager.Instance.themes[2].gameObject.activeSelf)
            {
                transform.position = startPos; // 원래 위치로 고정
                return; // 아래의 부유 로직을 실행하지 않음
            }
        }

        // 우주 테마가 켜져 있을 때만 실행되는 무중력 연출
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}