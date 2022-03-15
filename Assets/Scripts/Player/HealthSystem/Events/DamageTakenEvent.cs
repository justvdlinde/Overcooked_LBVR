using Utils.Core.Events;

public class DamageTakenEvent : IEvent
{
    public readonly IDamage Damage;

    public DamageTakenEvent(IDamage damage)
    {
        Damage = damage;
    }
}
