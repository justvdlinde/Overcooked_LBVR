using Utils.Core.Events;

public class ActiveOrderRemovedEvent : IEvent
{
    public readonly Order Order;

    public ActiveOrderRemovedEvent(Order order)
    {
        Order = order;
    }
}
