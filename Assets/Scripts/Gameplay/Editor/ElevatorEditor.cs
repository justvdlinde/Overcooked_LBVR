using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Elevator))]
public class ElevatorEditor : Editor
{
    private Elevator elevator;

    private void OnEnable()
    {
        elevator = target as Elevator;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUI.enabled = false;
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("State: ", elevator.CurrentState.ToString());
        EditorGUILayout.Toggle("IsMoving: ", elevator.IsMoving);
        GUI.enabled = true;

        GUI.enabled = Application.isPlaying;
        if(GUILayout.Button("Move"))
        {
            elevator.Move();
        }
        GUI.enabled = true;
    }
}
