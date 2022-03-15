using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Trigger), true)]
public class TriggerEditor : Editor
{
    private Trigger trigger;
    private Canvas canvas;
    private Vector3 originalCanvasScale;

    private void OnEnable()
    {
        trigger = target as Trigger;
        canvas = trigger.gameObject.GetComponentInChildren<Canvas>();
        if (canvas != null)
            originalCanvasScale = canvas.transform.lossyScale;
    }

    private void OnSceneGUI()
    {
        if (canvas != null)
            SetGlobalScale(canvas.transform, originalCanvasScale);
    }   

    public void SetGlobalScale(Transform transform, Vector3 globalScale)
    {
        transform.localScale = Vector3.one;
        transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
    }
}
