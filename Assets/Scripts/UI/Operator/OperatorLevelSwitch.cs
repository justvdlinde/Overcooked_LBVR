using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Core.SceneManagement;
using Utils.Core.Services;

public class OperatorLevelSwitch : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown = null;
    [SerializeField] private GameLevelsContainer levelsContainer = null;

    private SceneService sceneService;

    private void Awake()
    {
        sceneService = GlobalServiceLocator.Instance.Get<SceneService>();
    }
    private void Start()
    {
        dropdown.ClearOptions();
        List<string> levels = new List<string>();
        for (int i = 0; i < levelsContainer.GameLevels.Count; i++)
        {
            levels.Add(levelsContainer.GameLevels[i].Title);
        }
        dropdown.AddOptions(levels);
    }

    private void OnEnable()
    {
        dropdown.onValueChanged.AddListener(OnLevelSelected);
        SceneManager.activeSceneChanged += OnSceneChangedEvent;
    }

    private void OnDisable()
    {
        dropdown.onValueChanged.RemoveListener(OnLevelSelected);
        SceneManager.activeSceneChanged -= OnSceneChangedEvent;
    }

    private void OnSceneChangedEvent(Scene oldScene, Scene newScene)
    {
        UpdateDropdownToCurrentLevel(newScene);
    }

    private void UpdateDropdownToCurrentLevel(Scene scene)
    {
        for (int i = 0; i < levelsContainer.GameLevels.Count; i++)
        {
            if (scene.name == levelsContainer.GameLevels[i].Scene.SceneName)
            {
                dropdown.SetValueWithoutNotify(i);
                return;
            }
        }
    }

    private void OnLevelSelected(int index)
    {
        GameLevel level = levelsContainer.GameLevels[index];
        sceneService.LoadSceneAsync(level.Scene);
    }
}
