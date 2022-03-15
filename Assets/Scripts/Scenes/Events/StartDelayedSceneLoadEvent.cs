using Utils.Core.Events;

/// <summary>
/// Called before scene starts to load, with delay.
/// </summary>
public class StartDelayedSceneLoadEvent : IEvent
{
    /// <summary>
    /// Name of scene that will be loaded
    /// </summary>
    public readonly string SceneName;

    /// <summary>
    /// Delay before scene starts to actually load, in seconds
    /// </summary>
    public readonly float Delay;

    public StartDelayedSceneLoadEvent(string sceneName, float delay)
    {
        SceneName = sceneName;
        Delay = delay;
    }
}
