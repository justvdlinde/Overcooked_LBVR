using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

/// <summary>
/// Static helpers for OVRPointerEventData.
/// </summary>
public static class VRPointerEventDataExtension
{
    public static bool IsVRPointer(this PointerEventData pointerEventData)
    {
        return (pointerEventData is VRPointerEventData);
    }

    public static Ray GetRay(this PointerEventData pointerEventData)
    {
        VRPointerEventData vrPointerEventData = pointerEventData as VRPointerEventData;
        Assert.IsNotNull(vrPointerEventData);

        return vrPointerEventData.worldSpaceRay;
    }

    public static Vector2 GetSwipeStart(this PointerEventData pointerEventData)
    {
        VRPointerEventData vrPointerEventData = pointerEventData as VRPointerEventData;
        Assert.IsNotNull(vrPointerEventData);

        return vrPointerEventData.swipeStart;
    }

    public static void SetSwipeStart(this PointerEventData pointerEventData, Vector2 start)
    {
        VRPointerEventData vrPointerEventData = pointerEventData as VRPointerEventData;
        Assert.IsNotNull(vrPointerEventData);

        vrPointerEventData.swipeStart = start;
    }
}