using Utils.Core.Events;

/// <summary>
/// Global player death event 
/// </summary>
public class PlayerDeathEvent : IEvent
{
    public readonly PlayerDeathInfo Info;

    public PlayerDeathEvent(PlayerDeathInfo info)
    {
        Info = info;
    }
}
