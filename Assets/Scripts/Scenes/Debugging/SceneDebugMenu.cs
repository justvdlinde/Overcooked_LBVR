using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Core.Events;
using Utils.Core.SceneManagement;
using Utils.Core.Services;

public class SceneDebugMenu : IDebugMenu
{
    private GameLevelsContainer levels;
    private Vector2 scrollPosition;
    private string delayText;
    private float delay = 1;

    private readonly SceneService sceneService;
    private readonly CoroutineService coroutineService;
    private readonly GlobalEventDispatcher globalEventDispatcher;

    public SceneDebugMenu(SceneService sceneService, CoroutineService coroutineService, GlobalEventDispatcher globalEventDispatcher)
    {
        this.sceneService = sceneService;
        this.coroutineService = coroutineService;
        this.globalEventDispatcher = globalEventDispatcher;
    }

    public void Open()
    {
        RefreshList();
        delayText = delay.ToString();
    }

    public void Close() { }

    private void RefreshList()
    {
        levels = Resources.Load<GameLevelsContainer>("GameLevels");
    }

    public void OnGUI(bool drawDeveloperOptions)
    {
        GUILayout.BeginVertical("box", GUILayout.MinWidth(400));
        GUILayout.Label("Current scene: " + SceneManager.GetActiveScene().name);
        GUILayout.Label("Total scenes: " + SceneManager.sceneCountInBuildSettings);
        GUILayout.Label("Total levels: " + levels.GameLevels.Count);
        if (GUILayout.Button("Refresh Scenes List"))
            RefreshList();

        delayText = GUILayout.TextField(delayText);
        if (float.TryParse(delayText, out float newDelay))
            delay = newDelay;

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < levels.GameLevels.Count; i++)
        {
            DrawScenePanel(levels.GameLevels[i]);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawScenePanel(GameLevel level)
    {
        if (level == null)
            return;

        GUILayout.BeginVertical(level.Title + " (" + level.Dimensions.x + "x" + level.Dimensions.y + ")", "window");
        
        if (level.Scene != null)
        {
            GUILayout.BeginHorizontal();
            if(level.Scene.SceneName == "")
            {
                GUILayout.Label("No Scene Asset");
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                return;
            }

            GUILayout.Label("Scene Name: " + level.Scene.SceneName);

#if UNITY_EDITOR
            if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
                UnityEditor.Selection.activeObject = level;
#endif
            GUILayout.EndHorizontal();

            if (sceneService.IsLoadingScene)
                GUI.enabled = false;
            else
                GUI.enabled = sceneService.ActiveScene.name != level.Scene.SceneName;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load", GUILayout.MaxWidth(200)))
                sceneService.LoadSceneAsync(level.Scene.SceneName);
            if (GUILayout.Button("Load Delayed", GUILayout.MaxWidth(200)))
                coroutineService.StartCoroutine(StartSceneLoadDelayed(level.Scene.SceneName, delay));
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label("No Scene Asset");
        }

        GUILayout.EndVertical();
    }

    protected virtual IEnumerator StartSceneLoadDelayed(string sceneName, float seconds)
    {
        globalEventDispatcher.Invoke(new StartDelayedSceneLoadEvent(sceneName, seconds));
        yield return new WaitForSeconds(seconds);
        sceneService.LoadSceneAsync(sceneName);
    }
}
