using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    [Header("회전 설정")]
    public Vector3 rotationSpeed = new Vector3(0, 10f, 0); // Y축 기준으로 초당 10도 회전

    void Update()
    {
        // 매 프레임마다 지정된 속도로 회전
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}