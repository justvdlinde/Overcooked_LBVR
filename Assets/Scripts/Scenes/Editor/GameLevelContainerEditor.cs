using System;
using UnityEditor;
using UnityEngine;
using Utils.Core;

[CustomEditor(typeof(GameLevelsContainer))]
public class GameLevelContainerEditor : Editor
{
    public const string DefaultPath = @"Assets/Data/Levels/Resources";

    [MenuItem("Levels/Select Game Levels")]
    private static void Select()
    {
        GameLevelsContainer levels = Resources.Load<GameLevelsContainer>("GameLevels");
        if (levels == null)
        {
            CreateNewContainer();
        }
        else
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = levels;
        }
    }

    private static void CreateNewContainer()
    {
        ScriptableObject asset = ScriptableObjectUtility.CreateAssetAtPath(typeof(GameLevelsContainer), DefaultPath, "GameLevels");
        EditorUtility.FocusProjectWindow();
        EditorUtility.SetDirty(asset);
        Selection.activeObject = asset;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Create and Add New Level"))
        {
            string path = ScriptableObjectUtility.GetSelectionAssetPath();

            if (path == null)
                throw new ArgumentNullException("path");

            if (path.Contains(@"\Resources"))
                path = path.Replace(@"\Resources", "");

            ScriptableObject asset = ScriptableObjectUtility.CreateAssetAtPath(typeof(GameLevel), path, "New GameLevel");
            EditorUtility.FocusProjectWindow();
            EditorUtility.SetDirty(asset);
            Selection.activeObject = asset;

            (target as GameLevelsContainer).GameLevels.Add(asset as GameLevel);
        }
    }
}
