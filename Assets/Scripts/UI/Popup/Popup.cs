using UnityEngine;
using Utils.Core.Attributes;

public class Popup : MonoBehaviour
{
    [SerializeField] protected bool openOnStart = true;
    [SerializeField] protected bool dontDestroyOnLoad = true;
    [SerializeField] protected bool destroyOnClose = true;
    [SerializeField] protected CanvasGroup canvasGroup = null;

    [Header("Animation")]
    [SerializeField] protected bool playAnimation = true;
    [SerializeField] protected Animator animator = null;
    [SerializeField] protected AnimationClip openAnimation = null;
    [SerializeField] protected AnimationClip closeAnimation = null;

    protected const string OpenStateName = "Open";
    protected const string CloseStateName = "Close";
    protected const string OpenTriggerName = "Open";

    protected virtual void Awake()
    {
        if(dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }

    protected virtual void Start()
    {
        if(canvasGroup != null)
            canvasGroup.blocksRaycasts = false;

        if (openOnStart)
            Open();
    }

    public virtual void Open()
    {
        animator.SetBool(OpenTriggerName, true);

        if(!playAnimation)
            animator.CrossFade(OpenStateName, 0f, 0, 1f);
    }

    public virtual void Close()
    {
        if(canvasGroup != null)
            canvasGroup.blocksRaycasts = false;

        animator.SetBool(OpenTriggerName, false);
        if (!playAnimation)
            animator.CrossFade(CloseStateName, 0f, 0, 1f);
    }

    // Called by animation event:
    public virtual void OnOpenDoneEvent() 
    { 
        if(canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }

    // Called by animation event:
    public virtual void OnCloseDoneEvent()
    {
        if (destroyOnClose)
            Destroy(gameObject);
    }

#if UNITY_EDITOR
    [Button]
    public void OpenDebug()
    {
        Open();
    }

    [Button]
    public void CloseDebug()
    {
        Close();
    }
#endif
}
