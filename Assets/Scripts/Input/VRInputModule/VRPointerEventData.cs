using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

/// <summary>
/// Extension of Unity's PointerEventData to support ray based pointing and also touchpad swiping
/// </summary>
public class VRPointerEventData : PointerEventData
{
    public Ray worldSpaceRay;
    public Vector2 swipeStart;

    public VRPointerEventData(EventSystem eventSystem) : base(eventSystem) { }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<b>Position</b>: " + position);
        sb.AppendLine("<b>delta</b>: " + delta);
        sb.AppendLine("<b>eligibleForClick</b>: " + eligibleForClick);
        sb.AppendLine("<b>pointerEnter</b>: " + pointerEnter);
        sb.AppendLine("<b>pointerPress</b>: " + pointerPress);
        sb.AppendLine("<b>lastPointerPress</b>: " + lastPress);
        sb.AppendLine("<b>pointerDrag</b>: " + pointerDrag);
        sb.AppendLine("<b>worldSpaceRay</b>: " + worldSpaceRay);
        sb.AppendLine("<b>swipeStart</b>: " + swipeStart);
        sb.AppendLine("<b>Use Drag Threshold</b>: " + useDragThreshold);
        return sb.ToString();
    }
}

