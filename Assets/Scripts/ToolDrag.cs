using UnityEngine;
using UnityEngine.EventSystems; // 추가 필수

public class ToolDrag : MonoBehaviour
{
    private Vector3 startPos;
    private Camera mainCam;
    private float zDistance;
    private Collider myCollider;
    private bool isDragging;

    void Start()
    {
        startPos = transform.position;
        mainCam = Camera.main;
        myCollider = GetComponent<Collider>();
    }

    // OnMouseDown/Drag/Up 대신 PointerInput으로 직접 처리 (모바일 터치 대응)
    void Update()
    {
        if (!isDragging && PointerInput.PressedThisFrame && PointerInput.HitsCollider(mainCam, myCollider))
        {
            isDragging = true;
            zDistance = mainCam.WorldToScreenPoint(transform.position).z;
        }

        if (!isDragging) return;

        if (PointerInput.IsPressed)
        {
            Vector3 mousePos = PointerInput.Position;
            mousePos.z = zDistance;
            Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);
            transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
        }
        else
        {
            isDragging = false;
            transform.position = startPos;
        }
    }
}