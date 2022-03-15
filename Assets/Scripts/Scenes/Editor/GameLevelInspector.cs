using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameLevel))]
public class GameLevelInspector : Editor
{
    private SerializedProperty dimensions;
    private SerializedProperty dimensionsInFeet;

    private void OnEnable()
    {
        dimensions = serializedObject.FindProperty("dimensions");
        dimensionsInFeet = serializedObject.FindProperty("dimensionsInFeet");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            float x = dimensions.vector2Value.x;
            float y = dimensions.vector2Value.y;
            float xFeet = (float)DimensionsHelper.MetersToFeet(x);
            float yFeet = (float)DimensionsHelper.MetersToFeet(y);
            dimensionsInFeet.vector2Value = new Vector2(xFeet, yFeet);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
