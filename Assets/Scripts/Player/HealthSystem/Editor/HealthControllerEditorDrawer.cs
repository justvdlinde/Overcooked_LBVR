using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HealthController), true)]
public class HealthControllerEditorDrawer : Editor
{
    private HealthController healthController;
    private float debugDamage = 10;

    private void OnEnable()
    {
        healthController = target as HealthController;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(10);
        GUI.enabled = false;
        EditorGUILayout.TextField("State ", healthController.State.ToString());
        EditorGUILayout.FloatField("Health", healthController.CurrentHealth);
        GUI.enabled = true;

        EditorGUILayout.Space(10);
        debugDamage = EditorGUILayout.FloatField("debugDamage ", debugDamage);

        EditorGUILayout.BeginHorizontal();
        GUI.enabled = healthController.State == PlayerState.Alive;
        if (GUILayout.Button("Take Damage"))
            healthController.ApplyDamage(new Damage(debugDamage));

        if (GUILayout.Button("Kill"))
            healthController.ApplyDamage(new Damage(healthController.CurrentHealth));

        GUI.enabled = healthController.State == PlayerState.Dead;
        if (GUILayout.Button("Respawn"))
            healthController.Respawn();

        EditorGUILayout.EndHorizontal();
        GUI.enabled = true;
    }
}
