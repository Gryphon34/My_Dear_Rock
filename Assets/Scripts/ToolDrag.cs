using UnityEngine;
using UnityEngine.EventSystems; // 추가 필수

public class ToolDrag : MonoBehaviour
{
    private Vector3 startPos;
    private Camera mainCam;
    private float zDistance;

    void Start()
    {
        startPos = transform.position;
        mainCam = Camera.main;
    }

    // [수정] 인터페이스 함수들을 OnMouseDown/Drag/Up으로 복구
    private void OnMouseDown()
    {
        zDistance = mainCam.WorldToScreenPoint(transform.position).z;
    }

    private void OnMouseDrag()
    {
        // [수정] eventData 대신 Input.mousePosition을 직접 사용합니다.
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = zDistance;
        Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);
        transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
    }

    private void OnMouseUp()
    {
        transform.position = startPos;
    }
}