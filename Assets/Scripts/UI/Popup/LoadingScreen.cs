using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils.Core.Events;
using Utils.Core.SceneManagement;
using Utils.Core.Services;

public class LoadingScreen : Popup
{
    [SerializeField] protected Slider progressBar = null;

    protected SceneService sceneService;
    protected GlobalEventDispatcher globalEventDispatcher;

    protected override void Awake()
    {
        base.Awake();

        sceneService = GlobalServiceLocator.Instance.Get<SceneService>();
        globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();

        globalEventDispatcher.Subscribe<StartDelayedSceneLoadEvent>(OnSceneLoadStartDelayEvent);
        sceneService.SceneLoadStartEvent += OnLoadSceneStartEvent;
        sceneService.SceneLoadFinishEvent += OnSceneLoadedEvent;
    }

    protected virtual void OnDestroy()
    {
        globalEventDispatcher.Unsubscribe<StartDelayedSceneLoadEvent>(OnSceneLoadStartDelayEvent);
        sceneService.SceneLoadStartEvent -= OnLoadSceneStartEvent;
        sceneService.SceneLoadFinishEvent -= OnSceneLoadedEvent;
    }

    protected virtual void OnSceneLoadedEvent(Scene scene, LoadSceneMode loadMode)
    {
        Close();
    }

    protected virtual void OnSceneLoadStartDelayEvent(StartDelayedSceneLoadEvent @event)
    {
        Open();
    }

    protected virtual void OnLoadSceneStartEvent(string sceneName)
    {
        Open();
        StartCoroutine(UpdateProgressBar());
    }

    protected virtual IEnumerator UpdateProgressBar()
    {
        while (sceneService.SceneLoadOperation != null && !sceneService.SceneLoadOperation.isDone)
        {
            progressBar.value = sceneService.LoadingProgress;
            yield return null;
        }

        progressBar.value = 1;
    }
}
