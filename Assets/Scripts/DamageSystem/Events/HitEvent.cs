using Utils.Core.Events;

public class HitEvent : IEvent
{
    public readonly HitData Data;

    public HitEvent(HitData data)
    {
        Data = data;
    }
}
