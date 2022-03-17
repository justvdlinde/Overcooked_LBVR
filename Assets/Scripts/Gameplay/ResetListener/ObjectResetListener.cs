using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Events;
using Utils.Core.Services;

public class ObjectResetListener : MonoBehaviour
{
    [SerializeField] private GameObject[] resettables = null;

    private GlobalEventDispatcher eventDispatcher;

    private void Awake()
    {
        eventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
    }

    private void OnEnable()
    {
        eventDispatcher.Subscribe<ReplayEvent>(OnReplayEvent);
        eventDispatcher.Subscribe<GameModeChangedEvent>(OnGameModeChangedEvent);
    }

    private void OnDisable()
    {
        eventDispatcher.Unsubscribe<ReplayEvent>(OnReplayEvent);
        eventDispatcher.Unsubscribe<GameModeChangedEvent>(OnGameModeChangedEvent);
    }

    private void OnGameModeChangedEvent(GameModeChangedEvent obj)
    {
        ResetObjects();
    }

    private void OnReplayEvent(ReplayEvent @event)
    {
        ResetObjects();
    }

    [Button]
    private void ResetObjects()
    {
        foreach (GameObject o in resettables)
        {
            foreach (Component c in o.GetComponents<Component>())
            {
                if (c is IResettable)
                    (c as IResettable).Reset();
            }
        }
    }
}
