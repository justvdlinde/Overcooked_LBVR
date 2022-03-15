using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Core.Attributes;
using Utils.Core.Events;
using Utils.Core.SceneManagement;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup = null;

    [Tooltip("How fast to fade in")]
    [SerializeField] private float fadeInSpeed = 1f;

    [Tooltip("How fast to fade out")]
    [SerializeField] private float fadeOutSpeed = 1f;

    [Tooltip("Should the screen fade in when a new level is loaded")]
    [SerializeField] private bool fadeOnSceneLoad = true;

    [Tooltip("Wait X seconds before fading scene in")]
    [SerializeField] private float sceneFadeOutDelay = 1f;

    private Coroutine activeFadeCoroutine;
    private SceneService sceneService;
    private GlobalEventDispatcher globalEventDispatcher;

    public void InjectDependencies(SceneService sceneService, GlobalEventDispatcher globalEventDispatcher)
    {
        this.sceneService = sceneService;
        this.globalEventDispatcher = globalEventDispatcher;
    }

    protected void OnEnable()
    {
        globalEventDispatcher.Subscribe<StartDelayedSceneLoadEvent>(OnSceneLoadStartDelayEvent);
        sceneService.SceneLoadStartEvent += OnSceneLoadEvent;
        sceneService.SceneLoadFinishEvent += OnSceneLoadFinishEvent;
    }

    protected void OnDisable()
    {
        globalEventDispatcher.Unsubscribe<StartDelayedSceneLoadEvent>(OnSceneLoadStartDelayEvent);
        sceneService.SceneLoadStartEvent -= OnSceneLoadEvent;
        sceneService.SceneLoadFinishEvent -= OnSceneLoadFinishEvent;
    }

    protected void Start()
    {
        FadeIn(fadeInSpeed);
    }

    private void OnSceneLoadStartDelayEvent(StartDelayedSceneLoadEvent @event)
    {
        if(fadeOnSceneLoad)
            FadeIn(@event.Delay);
    }

    private void OnSceneLoadEvent(string sceneName)
    {
        if(fadeOnSceneLoad)
            UpdateImageAlpha(1);
    }

    protected virtual void OnSceneLoadFinishEvent(Scene scene, LoadSceneMode mode)
    {
        if (fadeOnSceneLoad)
            StartCoroutine(Wait(sceneFadeOutDelay, () => FadeOut(fadeOutSpeed)));
    }

    protected IEnumerator Wait(float speed, Action onDone)
    {
        yield return new WaitForSeconds(speed);
        onDone?.Invoke();
    }

    /// <summary>
    /// Fade from transparent to solid
    /// </summary>
    public virtual Coroutine FadeIn(float speed)
    {
        if (activeFadeCoroutine != null)
        {
            StopCoroutine(activeFadeCoroutine);
            activeFadeCoroutine = null;
        }

        activeFadeCoroutine = StartCoroutine(ExecuteFade(canvasGroup.alpha, 1, speed));
        return activeFadeCoroutine;
    }

    /// <summary>
    /// Fade from transparent to solid
    /// </summary>
    [Button]
    public virtual void FadeIn()
    {
        FadeIn(fadeInSpeed);
    }

    /// <summary>
    /// Fade from solid to transparent
    /// </summary>
    public virtual Coroutine FadeOut(float speed)
    {
        if (activeFadeCoroutine != null)
        {
            StopCoroutine(activeFadeCoroutine);
            activeFadeCoroutine = null;
        }

        activeFadeCoroutine = StartCoroutine(ExecuteFade(canvasGroup.alpha, 0, speed));
        return activeFadeCoroutine;
    }

    /// <summary>
    /// Fade from solid to transparent
    /// </summary>
    [Button]
    public virtual void FadeOut()
    {
        FadeOut(fadeOutSpeed);
    }

    public virtual void SetFadeLevel(float fadeLevel)
    {
        // Give priority to coroutine fade
        if (activeFadeCoroutine != null)
            return;

        UpdateImageAlpha(fadeLevel);
    }

    private IEnumerator ExecuteFade(float from, float to, float speed)
    {
        float alpha = from;
        UpdateImageAlpha(alpha);

        while (alpha != to)
        {
            if (from < to)
            {
                alpha += Time.deltaTime * speed;
                if (alpha > to)
                    break;
            }
            else
            {
                alpha -= Time.deltaTime * speed;
                if (alpha < to)
                    break;
            }

            UpdateImageAlpha(alpha);

            yield return null;
        }

        yield return new WaitForEndOfFrame();

        UpdateImageAlpha(to);
        activeFadeCoroutine = null;
    }

    protected virtual void UpdateImageAlpha(float alphaValue)
    {
        if (!canvasGroup.gameObject.activeSelf)
            canvasGroup.gameObject.SetActive(true);

        canvasGroup.alpha = alphaValue;

        if (alphaValue == 0 && canvasGroup.gameObject.activeSelf)
            canvasGroup.gameObject.SetActive(false);
    }
}
