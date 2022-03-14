using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerCollisionDetector))]
public class CollisionDetectorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUI.enabled = false;
        EditorGUILayout.Toggle("IsColliding", (target as PlayerCollisionDetector).IsColliding);
        GUI.enabled = true;
    }
}
