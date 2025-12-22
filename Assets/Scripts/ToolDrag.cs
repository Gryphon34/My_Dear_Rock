using UnityEngine;
using UnityEngine.EventSystems; // 추가 필수

public class ToolDrag : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Vector3 startPos;
    private Camera mainCam;
    private float zDistance;

    void Start()
    {
        startPos = transform.position;
        mainCam = Camera.main;
    }

    // OnMouseDown 대체
    public void OnPointerDown(PointerEventData eventData)
    {
        zDistance = mainCam.WorldToScreenPoint(transform.position).z;
    }

    // OnMouseDrag 대체
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 mousePos = eventData.position; // New Input 방식의 좌표
        mousePos.z = zDistance;
        Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);
        transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
    }

    // OnMouseUp 대체
    public void OnPointerUp(PointerEventData eventData)
    {
        transform.position = startPos;
    }
}