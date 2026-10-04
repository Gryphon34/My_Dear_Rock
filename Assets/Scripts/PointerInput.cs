using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// 마우스/터치 입력을 하나로 묶어주는 헬퍼
// Player Settings의 Active Input Handling이 Old / New / Both 어느 것이어도 동작합니다.
public static class PointerInput
{
    public static bool PressedThisFrame
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }
    }

    public static bool IsPressed
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return Pointer.current != null && Pointer.current.press.isPressed;
#else
            return Input.GetMouseButton(0);
#endif
        }
    }

    public static bool ReleasedThisFrame
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return Pointer.current != null && Pointer.current.press.wasReleasedThisFrame;
#else
            return Input.GetMouseButtonUp(0);
#endif
        }
    }

    public static Vector2 Position
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }
    }

    // 현재 포인터 위치에서 카메라 레이를 쏴서 가장 먼저 맞은 콜라이더가 target인지 확인 (OnMouseDown과 같은 방식)
    public static bool HitsCollider(Camera cam, Collider target)
    {
        if (cam == null || target == null) return false;
        Ray ray = cam.ScreenPointToRay(Position);
        return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity) && hit.collider == target;
    }
}
